#!/bin/bash
# generate-expectations.sh — Generate test expectations from SPEC-REFERENCE.md
#
# Reads the spec and produces expectations.json describing what each E2E test
# case should demonstrate. This is the "ground truth" that traces are graded against.
#
# Usage: ./scripts/evals/generate-expectations.sh [--spec FILE] [--output FILE]
#
# Output: expectations.json with array of test expectations

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Defaults
SPEC_FILE="$REPO_ROOT/SPEC-REFERENCE.md"
OUTPUT_FILE=""

while [[ $# -gt 0 ]]; do
  case $1 in
    --spec) SPEC_FILE="$2"; shift 2 ;;
    --output) OUTPUT_FILE="$2"; shift 2 ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

if [[ ! -f "$SPEC_FILE" ]]; then
  echo "ERROR: Spec file not found: $SPEC_FILE" >&2
  exit 1
fi

PROMPT=$(cat <<'PROMPT_EOF'
You are generating E2E test expectations from a full-stack application specification. The spec contains BOTH API endpoints AND frontend UI behaviors. Read the ENTIRE spec and produce a JSON array of test expectations.

These expectations are used to grade Playwright E2E test traces. They describe what a test should demonstrate from the user's perspective in the browser, backed by network evidence.

Each expectation describes ONE testable behavior. Group by feature area: auth, profiles, articles, comments, favorites, feed, tags, settings, users, dashboard, mobile, multitenancy, swagger, editor.

For each expectation, produce:

```json
{
  "id": "auth-001",
  "name": "UserCanRegisterAsFirstUser",
  "category": "happy_path" | "validation" | "permissions" | "screenshots",
  "spec_section": "Authentication > Registration Flow",
  "feature_area": "auth",
  "expected_behavior": "First user registers via /register, gets 204, can then login and receives a JWT token. First user gets ADMIN + USER roles.",
  "key_assertions": [
    "POST /api/identity/register returns 204",
    "POST /api/identity/login returns accessToken",
    "User sees authenticated UI (settings link visible, sign-in link hidden)",
    "Users nav item visible (user is ADMIN)"
  ],
  "ui_indicators": [
    "Registration form accepts email + password",
    "After login, sidebar shows Settings and Users links",
    "Username displayed in navigation matches email"
  ],
  "network_indicators": [
    "POST /api/identity/register → 204",
    "POST /api/identity/login?useCookies=false → 200 with accessToken",
    "GET /api/user → 200 with user object"
  ],
  "preconditions": "No existing users (fresh tenant)",
  "data_setup": "none"
}
```

Rules:
1. COMPREHENSIVENESS IS CRITICAL. Cover ALL endpoints AND ALL frontend UI behaviors in the spec. Do not skip any section. Every API endpoint, every UI behavior bullet point, every screenshot test, every validation scenario must have at least one expectation.
2. Include happy_path, validation, permissions, and screenshots categories
3. `key_assertions` — what MUST be true for this test to pass. Include BOTH:
   - UI-level assertions: DOM state, visible text, URL changes, error messages displayed, elements visible/hidden
   - API-level assertions: status codes, response fields (these support the UI assertions)
4. `ui_indicators` — what a human looking at screenshots/DOM would see. Be specific: name the exact text, link, button, or element.
5. `network_indicators` — API calls that should appear in the network trace
6. Keep descriptions concrete and verifiable, not vague
7. For validation cases: include BOTH the API response (e.g., "returns 400") AND the UI outcome (e.g., "error message is displayed in the DOM"). Both matter.
8. For permissions/redirect cases: include BOTH the API response (e.g., "returns 401") AND the frontend behavior (e.g., "browser URL changes to /login").
9. For frontend-only behaviors (mobile layout, tag input UI, pagination navigation, feature flags, i18n): there may be NO network indicators — that's fine. The key assertions are purely UI-based.
10. IDs should be sequential within each feature area (auth-001, auth-002, etc.)
11. Do NOT collapse multiple distinct behaviors into one expectation. Each separately testable behavior gets its own entry.
12. Generate SEPARATE expectations for:
    - Each distinct validation scenario (duplicate email vs duplicate username are separate)
    - Each settings field validation (duplicate username on settings page vs duplicate email on settings page are separate)
    - Each screenshot/visual regression test mentioned in the spec
    - Each mobile-specific behavior
    - Each protected route redirect
    - Viewing own profile vs viewing another user's profile

Output ONLY the JSON array, no markdown fencing, no explanation.

Here is the spec:

PROMPT_EOF
)

echo "Generating expectations from: $SPEC_FILE" >&2

# Combine prompt with spec content
FULL_PROMPT="$PROMPT

$(cat "$SPEC_FILE")"

# Call Claude to generate expectations
RESULT=$(echo "$FULL_PROMPT" | claude -p --output-format json 2>/dev/null)

# Extract the text content from Claude's response
# Claude --output-format json wraps the response in a JSON envelope
EXPECTATIONS=$(echo "$RESULT" | jq -r '
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

# Validate it's valid JSON array
if ! echo "$EXPECTATIONS" | jq 'if type == "array" then . else error("not an array") end' >/dev/null 2>&1; then
  # Try to extract JSON array from the response (model may have wrapped it in text)
  EXPECTATIONS=$(echo "$EXPECTATIONS" | grep -o '\[.*\]' | head -1)
  if ! echo "$EXPECTATIONS" | jq '.' >/dev/null 2>&1; then
    echo "ERROR: Failed to generate valid JSON expectations" >&2
    echo "Raw output saved to /tmp/expectations-raw.txt" >&2
    echo "$RESULT" > /tmp/expectations-raw.txt
    exit 1
  fi
fi

# Add metadata
FINAL=$(jq -n \
  --argjson expectations "$EXPECTATIONS" \
  --arg spec_file "$(basename "$SPEC_FILE")" \
  --arg generated_at "$(date -Iseconds)" \
  '{
    generated_at: $generated_at,
    spec_file: $spec_file,
    total_expectations: ($expectations | length),
    by_category: ($expectations | group_by(.category) | map({(.[0].category): length}) | add),
    by_feature: ($expectations | group_by(.feature_area) | map({(.[0].feature_area): length}) | add),
    expectations: $expectations
  }')

if [[ -n "$OUTPUT_FILE" ]]; then
  echo "$FINAL" | jq '.' > "$OUTPUT_FILE"
  echo "Wrote $(echo "$FINAL" | jq '.total_expectations') expectations to $OUTPUT_FILE" >&2
else
  echo "$FINAL" | jq '.'
fi
