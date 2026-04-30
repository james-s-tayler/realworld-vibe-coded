#!/bin/bash
# extract-master-list.sh — Reverse-engineer test coverage from existing E2E test suite
#
# Reads all test files in Test/e2e/E2eTests/Tests/, extracts test method
# signatures and code, then uses Claude to produce a structured master list
# describing what each test proves.
#
# Usage: ./scripts/evals/extract-master-list.sh [--tests-dir DIR] [--output FILE]
#
# Output: master-list.json with the known test coverage baseline

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

TESTS_DIR="$REPO_ROOT/Test/e2e/E2eTests/Tests"
OUTPUT_FILE=""

while [[ $# -gt 0 ]]; do
  case $1 in
    --tests-dir) TESTS_DIR="$2"; shift 2 ;;
    --output) OUTPUT_FILE="$2"; shift 2 ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

if [[ ! -d "$TESTS_DIR" ]]; then
  echo "ERROR: Tests directory not found: $TESTS_DIR" >&2
  exit 1
fi

# Step 1: Collect all test source code into a single document
# Organized by page/category for the model to parse
echo "Collecting test files from $TESTS_DIR..." >&2

TEST_CODE=""
TEST_COUNT=0

for test_file in "$TESTS_DIR"/*/*.cs; do
  [[ -f "$test_file" ]] || continue
  relative_path="${test_file#$REPO_ROOT/}"
  file_content=$(cat "$test_file")

  # Skip files with no [Fact] attributes (empty test files)
  if ! echo "$file_content" | grep -q '\[Fact\]'; then
    continue
  fi

  method_count=$(echo "$file_content" | grep -c '\[Fact\]' || true)
  TEST_COUNT=$((TEST_COUNT + method_count))

  TEST_CODE+="
=== FILE: $relative_path ===
$file_content
"
done

echo "Found $TEST_COUNT test methods across $(echo "$TEST_CODE" | grep -c '=== FILE:') files" >&2

# Step 2: Also collect page model source for context
PAGE_MODELS_DIR="$REPO_ROOT/Test/e2e/E2eTests/PageModels"
PAGE_MODEL_CODE=""

if [[ -d "$PAGE_MODELS_DIR" ]]; then
  for pm_file in "$PAGE_MODELS_DIR"/*.cs; do
    [[ -f "$pm_file" ]] || continue
    relative_path="${pm_file#$REPO_ROOT/}"
    PAGE_MODEL_CODE+="
=== FILE: $relative_path ===
$(cat "$pm_file")
"
  done
fi

# Step 3: Send to Claude for structured extraction
PROMPT=$(cat <<'PROMPT_EOF'
You are analyzing an existing E2E test suite to extract a master list of test coverage.

Below are the test files and their page models. For each [Fact] test method, produce a JSON object describing what it tests.

For each test, produce:
```json
{
  "id": "page-category-NNN",
  "test_class": "Tests.LoginPage.HappyPath",
  "test_method": "UserCanSignIn_WithExistingCredentials",
  "file": "Test/e2e/E2eTests/Tests/LoginPage/HappyPath.cs",
  "category": "happy_path" | "validation" | "permissions" | "screenshots" | "multitenancy",
  "feature_area": "auth" | "articles" | "editor" | "feed" | "profiles" | "settings" | "users" | "swagger" | "multitenancy",
  "behavior_tested": "One sentence: what behavior this test verifies",
  "key_verifications": ["List of specific things this test checks — derived from the Expect() and assert calls in the code"],
  "pages_involved": ["LoginPage", "SettingsPage"],
  "api_setup": ["CreateUserAsync", "InviteUserAsync"] or [],
  "ui_actions": ["Navigate to /login", "Fill email", "Fill password", "Click Sign in"],
  "requires_auth": true/false
}
```

Rules:
1. Every [Fact] method gets exactly one entry
2. `key_verifications` must be derived from actual Expect()/assert calls in the code, not inferred
3. `behavior_tested` should be concrete: "User can log in with valid credentials" not "Tests login"
4. `category` is derived from the file: HappyPath.cs → happy_path, Validation.cs → validation, etc.
5. `feature_area` is derived from the parent folder name
6. IDs: use format "{feature_area}-{category}-{NNN}" (e.g., "auth-happy-001")
7. Include ALL test methods — do not skip any

Output ONLY the JSON array, no markdown fencing, no explanation.

## Test Files

PROMPT_EOF
)

echo "Sending to Claude for analysis..." >&2

FULL_PROMPT="$PROMPT

$TEST_CODE

## Page Models (for context — these show what locators/actions are available)

$PAGE_MODEL_CODE"

RESULT=$(echo "$FULL_PROMPT" | claude -p --output-format json 2>/dev/null)

# Extract text content from Claude response
MASTER_LIST=$(echo "$RESULT" | jq -r '
  if type == "array" then
    map(select(.type == "text") | .text) | join("")
  elif .result then
    .result
  elif .content then
    if (.content | type) == "array" then
      .content | map(select(.type == "text") | .text) | join("")
    else
      .content
    end
  else
    tostring
  end
' 2>/dev/null || echo "$RESULT")

# Validate JSON array
if ! echo "$MASTER_LIST" | jq 'if type == "array" then . else error("not an array") end' >/dev/null 2>&1; then
  MASTER_LIST=$(echo "$MASTER_LIST" | grep -o '\[.*\]' | head -1)
  if ! echo "$MASTER_LIST" | jq '.' >/dev/null 2>&1; then
    echo "ERROR: Failed to generate valid JSON master list" >&2
    echo "Raw output saved to /tmp/master-list-raw.txt" >&2
    echo "$RESULT" > /tmp/master-list-raw.txt
    exit 1
  fi
fi

# Add metadata wrapper
ACTUAL_COUNT=$(echo "$MASTER_LIST" | jq 'length')
FINAL=$(jq -n \
  --argjson tests "$MASTER_LIST" \
  --arg tests_dir "$TESTS_DIR" \
  --arg generated_at "$(date -Iseconds)" \
  --argjson expected_count "$TEST_COUNT" \
  '{
    generated_at: $generated_at,
    source: $tests_dir,
    expected_test_count: $expected_count,
    actual_test_count: ($tests | length),
    count_match: ($expected_count == ($tests | length)),
    by_category: ($tests | group_by(.category) | map({(.[0].category): length}) | add),
    by_feature: ($tests | group_by(.feature_area) | map({(.[0].feature_area): length}) | add),
    tests: $tests
  }')

if [[ -n "$OUTPUT_FILE" ]]; then
  echo "$FINAL" | jq '.' > "$OUTPUT_FILE"
  echo "Wrote $ACTUAL_COUNT test descriptions to $OUTPUT_FILE (expected: $TEST_COUNT)" >&2
  if [[ "$ACTUAL_COUNT" -ne "$TEST_COUNT" ]]; then
    echo "WARNING: Count mismatch — model may have missed or duplicated some tests" >&2
  fi
else
  echo "$FINAL" | jq '.'
fi
