#!/bin/bash
# grade-trace.sh — Grade a single extracted trace against its expectation
#
# Takes the JSON output of extract-trace.sh and an expectation object,
# sends both to Claude for independent grading.
#
# Usage: ./scripts/evals/grade-trace.sh <extracted-trace.json> <expectation.json>
#    or: ./scripts/evals/grade-trace.sh --trace-json <json> --expectation-json <json>
#
# Output: JSON verdict to stdout

set -euo pipefail

TRACE_JSON=""
EXPECTATION_JSON=""
TRACE_FILE=""
EXPECTATION_FILE=""

# Parse args
while [[ $# -gt 0 ]]; do
  case $1 in
    --trace-json) TRACE_JSON="$2"; shift 2 ;;
    --expectation-json) EXPECTATION_JSON="$2"; shift 2 ;;
    *)
      if [[ -z "$TRACE_FILE" ]]; then
        TRACE_FILE="$1"; shift
      elif [[ -z "$EXPECTATION_FILE" ]]; then
        EXPECTATION_FILE="$1"; shift
      else
        echo "Unknown argument: $1" >&2; exit 1
      fi
      ;;
  esac
done

# Load from files if not passed inline
if [[ -z "$TRACE_JSON" && -n "$TRACE_FILE" ]]; then
  TRACE_JSON=$(cat "$TRACE_FILE")
fi
if [[ -z "$EXPECTATION_JSON" && -n "$EXPECTATION_FILE" ]]; then
  EXPECTATION_JSON=$(cat "$EXPECTATION_FILE")
fi

if [[ -z "$TRACE_JSON" || -z "$EXPECTATION_JSON" ]]; then
  echo "ERROR: Both trace and expectation JSON are required" >&2
  echo "Usage: grade-trace.sh <trace.json> <expectation.json>" >&2
  exit 1
fi

# Build the grading prompt
PROMPT=$(cat <<'PROMPT_EOF'
You are an independent evaluator grading a Playwright E2E test trace against a spec expectation.

You will receive:
1. A TEST EXPECTATION — what this test is supposed to demonstrate
2. A PLAYWRIGHT TRACE — the actual sequence of actions, network requests, DOM snapshots, and console messages from the test run

Your job: determine whether the trace ACTUALLY demonstrates what the expectation claims. The test may have passed mechanically but still be wrong (weak assertions, testing the wrong thing, checking stale state).

## Grading rules

- **PASS**: The trace demonstrates ALL key assertions from the expectation. Every important behavior is both performed AND verified. Network calls confirm the backend responded correctly. DOM/UI indicators match what's expected.

- **WEAK**: The feature *appears* to work (network calls succeed, DOM looks right) but the test doesn't fully verify the key claims. Examples:
  - Checks visibility of an element but not its content
  - Doesn't verify the correct API response status/body
  - Asserts navigation happened but not that the right data is displayed
  - Missing assertions for some key_assertions items

- **FAIL**: The trace does NOT demonstrate the expected behavior. Examples:
  - Actions don't match the expected flow
  - API returned error status codes
  - DOM doesn't show expected content
  - Console has errors that indicate failure
  - Test errored out or timed out

## Focus on evidence

Look at what the trace data ACTUALLY shows:
- Do the actions match the expected user flow?
- Do the network requests match network_indicators? Are status codes correct?
- Do DOM snapshots contain the expected ui_indicators text?
- Are there console errors that suggest something broke?
- Did any actions fail (passed: false)?

A test that mechanically passes but has weak evidence gets WEAK, not PASS.

## Output format

Respond with ONLY a JSON object (no markdown fencing):

{
  "verdict": "PASS" | "WEAK" | "FAIL",
  "confidence": 0.0 to 1.0,
  "demonstrated": ["list of key_assertions that ARE demonstrated by the trace"],
  "not_demonstrated": ["list of key_assertions that are NOT demonstrated"],
  "concerns": ["any issues: weak assertions, missing checks, timing problems, console errors"],
  "evidence": {
    "actions_match": true/false,
    "network_match": true/false,
    "ui_match": true/false,
    "no_errors": true/false
  },
  "reasoning": "One paragraph explaining the verdict"
}
PROMPT_EOF
)

# Truncate trace data if too large (keep under ~50k chars to avoid context issues)
TRACE_TRIMMED=$(echo "$TRACE_JSON" | jq '{
  source_file: .source_file,
  metadata: .metadata,
  summary: .summary,
  actions: (.actions[:50]),
  network: (.network[:30]),
  dom_snapshots: (.dom_snapshots[:10]),
  console_messages: .console_messages
}')

FULL_PROMPT=$(cat <<EOF
$PROMPT

## TEST EXPECTATION

$EXPECTATION_JSON

## PLAYWRIGHT TRACE

$TRACE_TRIMMED
EOF
)

# Call Claude for grading
RESULT=$(echo "$FULL_PROMPT" | claude -p --output-format json 2>/dev/null)

# Extract the text content
VERDICT=$(echo "$RESULT" | jq -r '
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

# Validate it's valid JSON
if ! echo "$VERDICT" | jq '.' >/dev/null 2>&1; then
  # Try to extract JSON object from response
  VERDICT=$(echo "$VERDICT" | grep -o '{.*}' | head -1)
  if ! echo "$VERDICT" | jq '.' >/dev/null 2>&1; then
    # Return a structured error
    jq -n \
      --arg raw "$RESULT" \
      '{
        verdict: "ERROR",
        confidence: 0,
        demonstrated: [],
        not_demonstrated: [],
        concerns: ["Failed to parse grading model response"],
        evidence: { actions_match: null, network_match: null, ui_match: null, no_errors: null },
        reasoning: "Grading model returned unparseable response",
        raw_response: ($raw | .[0:500])
      }'
    exit 0
  fi
fi

# Add the expectation ID to the verdict for cross-referencing
EXPECTATION_ID=$(echo "$EXPECTATION_JSON" | jq -r '.id // "unknown"')
EXPECTATION_NAME=$(echo "$EXPECTATION_JSON" | jq -r '.name // "unknown"')

echo "$VERDICT" | jq \
  --arg id "$EXPECTATION_ID" \
  --arg name "$EXPECTATION_NAME" \
  '. + { expectation_id: $id, expectation_name: $name }'
