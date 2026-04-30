#!/bin/bash
# eval-coverage.sh — Assert generated expectations cover the master list
#
# Compares expectations.json (generated from spec) against master-list.json
# (extracted from existing tests) to find gaps. Uses Claude for semantic
# matching since names won't match exactly.
#
# Usage: ./scripts/evals/eval-coverage.sh <master-list.json> <expectations.json> [--output FILE]
#
# Output: coverage-report.json with matched/unmatched/weak coverage analysis

set -euo pipefail

MASTER_LIST_FILE="${1:?Usage: eval-coverage.sh <master-list.json> <expectations.json> [--output FILE]}"
EXPECTATIONS_FILE="${2:?Usage: eval-coverage.sh <master-list.json> <expectations.json> [--output FILE]}"
OUTPUT_FILE=""

shift 2 || true
while [[ $# -gt 0 ]]; do
  case $1 in
    --output) OUTPUT_FILE="$2"; shift 2 ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

if [[ ! -f "$MASTER_LIST_FILE" ]]; then
  echo "ERROR: Master list not found: $MASTER_LIST_FILE" >&2
  exit 1
fi

if [[ ! -f "$EXPECTATIONS_FILE" ]]; then
  echo "ERROR: Expectations file not found: $EXPECTATIONS_FILE" >&2
  exit 1
fi

MASTER_LIST=$(cat "$MASTER_LIST_FILE")
EXPECTATIONS=$(cat "$EXPECTATIONS_FILE")

MASTER_COUNT=$(echo "$MASTER_LIST" | jq '.actual_test_count')
EXPECTATIONS_COUNT=$(echo "$EXPECTATIONS" | jq '.total_expectations')

echo "Master list: $MASTER_COUNT tests" >&2
echo "Expectations: $EXPECTATIONS_COUNT expectations" >&2
echo "" >&2

# Build the comparison prompt
PROMPT=$(cat <<'PROMPT_EOF'
You are comparing two sets of test coverage descriptions:

1. MASTER LIST — extracted from an existing, hand-written E2E test suite. This is the ground truth of what we know should be tested.
2. GENERATED EXPECTATIONS — produced by a model reading the API spec. This is what the generator thinks should be tested.

Your task: for each test in the master list, determine whether the generated expectations adequately cover that test case.

For each master list entry, produce:
```json
{
  "master_id": "auth-happy-001",
  "master_method": "UserCanSignIn_WithExistingCredentials",
  "master_behavior": "User can log in with valid credentials",
  "match_status": "covered" | "weak" | "missing",
  "matched_expectation_id": "auth-001" or null,
  "match_reasoning": "Why this is covered/weak/missing"
}
```

Match criteria:
- **covered**: An expectation exists that tests the same behavior AND has key_assertions that would verify the same things the master test checks
- **weak**: An expectation exists for the same feature area but doesn't fully cover the specific behavior (e.g., expectation tests login but doesn't check the specific error message the master test verifies)
- **missing**: No expectation covers this behavior at all

Rules:
1. Every master list entry must get exactly one match result
2. Multiple master entries CAN match the same expectation (that's fine — one expectation may cover what the existing suite splits across multiple tests)
3. Be strict about "covered" — the expectation must actually verify the same thing, not just be in the same area
4. Screenshots tests can be marked "covered" if any expectation covers the same page (screenshot tests are visual regression, not behavioral)
5. For multitenancy tests, look for isolation/tenant-specific expectations

Output ONLY the JSON array, no markdown fencing, no explanation.

## MASTER LIST (ground truth — from existing tests)

PROMPT_EOF
)

MASTER_TESTS=$(echo "$MASTER_LIST" | jq '[.tests[] | {id, test_method, behavior_tested, key_verifications, feature_area, category}]')
EXPECTATION_LIST=$(echo "$EXPECTATIONS" | jq '[.expectations[] | {id, name, expected_behavior, key_assertions, feature_area, category}]')

FULL_PROMPT="$PROMPT

$MASTER_TESTS

## GENERATED EXPECTATIONS (from spec — must cover master list)

$EXPECTATION_LIST"

echo "Sending to Claude for coverage analysis..." >&2

RESULT=$(echo "$FULL_PROMPT" | claude -p --output-format json 2>/dev/null)

# Extract text content
MATCHES=$(echo "$RESULT" | jq -r '
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

# Validate JSON
if ! echo "$MATCHES" | jq 'if type == "array" then . else error("not an array") end' >/dev/null 2>&1; then
  MATCHES=$(echo "$MATCHES" | grep -o '\[.*\]' | head -1)
  if ! echo "$MATCHES" | jq '.' >/dev/null 2>&1; then
    echo "ERROR: Failed to parse coverage analysis" >&2
    echo "Raw output saved to /tmp/coverage-raw.txt" >&2
    echo "$RESULT" > /tmp/coverage-raw.txt
    exit 1
  fi
fi

# Compute summary stats
COVERED=$(echo "$MATCHES" | jq '[.[] | select(.match_status == "covered")] | length')
WEAK=$(echo "$MATCHES" | jq '[.[] | select(.match_status == "weak")] | length')
MISSING=$(echo "$MATCHES" | jq '[.[] | select(.match_status == "missing")] | length')
TOTAL=$(echo "$MATCHES" | jq 'length')

# Coverage score: covered=1.0, weak=0.5, missing=0.0
if [[ "$TOTAL" -gt 0 ]]; then
  COVERAGE_SCORE=$(echo "scale=1; ($COVERED * 100 + $WEAK * 50) / $TOTAL" | bc)
else
  COVERAGE_SCORE=0
fi

# Build report
REPORT=$(jq -n \
  --argjson matches "$MATCHES" \
  --argjson covered "$COVERED" \
  --argjson weak "$WEAK" \
  --argjson missing "$MISSING" \
  --argjson total "$TOTAL" \
  --argjson coverage_score "$COVERAGE_SCORE" \
  --argjson master_count "$MASTER_COUNT" \
  --argjson expectations_count "$EXPECTATIONS_COUNT" \
  --arg generated_at "$(date -Iseconds)" \
  '{
    generated_at: $generated_at,
    score: {
      coverage: $coverage_score,
      max: 100,
      formula: "covered=1.0, weak=0.5, missing=0.0"
    },
    summary: {
      master_test_count: $master_count,
      generated_expectation_count: $expectations_count,
      total_evaluated: $total,
      covered: $covered,
      weak: $weak,
      missing: $missing,
      covered_pct: (if $total > 0 then (($covered * 100) / $total | floor) else 0 end),
      gap_count: ($weak + $missing)
    },
    missing_tests: [
      $matches[] | select(.match_status == "missing") |
      { master_id, master_method, master_behavior, match_reasoning }
    ],
    weak_coverage: [
      $matches[] | select(.match_status == "weak") |
      { master_id, master_method, master_behavior, matched_expectation_id, match_reasoning }
    ],
    all_matches: $matches
  }')

# Output
if [[ -n "$OUTPUT_FILE" ]]; then
  echo "$REPORT" | jq '.' > "$OUTPUT_FILE"
  echo "Wrote coverage report to $OUTPUT_FILE" >&2
else
  echo "$REPORT" | jq '.'
fi

# Print summary to stderr
echo "" >&2
echo "=== Coverage Results ===" >&2
echo "Score:    $COVERAGE_SCORE / 100" >&2
echo "Covered:  $COVERED / $TOTAL" >&2
echo "Weak:     $WEAK / $TOTAL" >&2
echo "Missing:  $MISSING / $TOTAL" >&2
echo "" >&2

if [[ "$MISSING" -gt 0 ]]; then
  echo "=== Missing Test Cases ===" >&2
  echo "$REPORT" | jq -r '.missing_tests[] | "  [\(.master_id)] \(.master_method): \(.master_behavior)"' >&2
fi

if [[ "$WEAK" -gt 0 ]]; then
  echo "" >&2
  echo "=== Weak Coverage ===" >&2
  echo "$REPORT" | jq -r '.weak_coverage[] | "  [\(.master_id)] \(.master_method): \(.match_reasoning)"' >&2
fi

# Exit with failure if coverage is below threshold
if [[ "$MISSING" -gt 0 ]]; then
  echo "" >&2
  echo "FAIL: $MISSING test cases from the master list are not covered by generated expectations" >&2
  exit 1
fi
