#!/bin/bash
# eval-traces.sh — Orchestrate the full trace eval pipeline
#
# Given a directory of Playwright trace zips and an expectations file,
# extracts each trace, grades it against matched expectations, and
# produces a summary report with composite scores.
#
# Usage: ./scripts/evals/eval-traces.sh <traces-dir> <expectations.json> [--output DIR]
#
# Trace files are matched to expectations by convention:
#   - Trace filename contains the test name (e.g., "UserCanSignIn_trace_20260407.zip")
#   - Matched against expectation.name using fuzzy substring match
#   - Unmatched traces are graded with a generic "describe what this demonstrates" prompt
#
# Output: eval-report.json with all verdicts + composite score

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

TRACES_DIR="${1:?Usage: eval-traces.sh <traces-dir> <expectations.json> [--output DIR]}"
EXPECTATIONS_FILE="${2:?Usage: eval-traces.sh <traces-dir> <expectations.json> [--output DIR]}"
OUTPUT_DIR=""

shift 2 || true
while [[ $# -gt 0 ]]; do
  case $1 in
    --output) OUTPUT_DIR="$2"; shift 2 ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

if [[ ! -d "$TRACES_DIR" ]]; then
  echo "ERROR: Traces directory not found: $TRACES_DIR" >&2
  exit 1
fi

if [[ ! -f "$EXPECTATIONS_FILE" ]]; then
  echo "ERROR: Expectations file not found: $EXPECTATIONS_FILE" >&2
  exit 1
fi

# Set up output directory
if [[ -z "$OUTPUT_DIR" ]]; then
  OUTPUT_DIR="$TRACES_DIR/eval-results"
fi
mkdir -p "$OUTPUT_DIR/extracted" "$OUTPUT_DIR/verdicts" "$OUTPUT_DIR/screenshots"

echo "=== Trace Eval Pipeline ===" >&2
echo "Traces:       $TRACES_DIR" >&2
echo "Expectations: $EXPECTATIONS_FILE" >&2
echo "Output:       $OUTPUT_DIR" >&2
echo "" >&2

# Load expectations
EXPECTATIONS=$(cat "$EXPECTATIONS_FILE")
TOTAL_EXPECTATIONS=$(echo "$EXPECTATIONS" | jq '.total_expectations')
echo "Loaded $TOTAL_EXPECTATIONS expectations" >&2

# Find all trace zips
TRACE_ZIPS=()
for f in "$TRACES_DIR"/*.zip; do
  [[ -f "$f" ]] && TRACE_ZIPS+=("$f")
done

if [[ ${#TRACE_ZIPS[@]} -eq 0 ]]; then
  echo "ERROR: No .zip trace files found in $TRACES_DIR" >&2
  exit 1
fi

echo "Found ${#TRACE_ZIPS[@]} trace files" >&2
echo "" >&2

# Match a trace filename to an expectation by fuzzy name matching
match_expectation() {
  local trace_name="$1"
  # Normalize: remove _trace_, timestamps, .zip, convert to lowercase
  local normalized
  normalized=$(echo "$trace_name" | sed 's/_trace_[0-9_]*//g; s/\.zip$//; s/_/ /g' | tr '[:upper:]' '[:lower:]')

  # Try to match against expectation names
  echo "$EXPECTATIONS" | jq -r --arg search "$normalized" '
    .expectations[] |
    .name as $name |
    ($name | gsub("(?<a>[A-Z])"; " \(.a)") | ltrimstr(" ") | ascii_downcase) as $normalized_name |
    select(
      ($search | contains($normalized_name)) or
      ($normalized_name | contains($search)) or
      # Try matching on individual words
      ([$search | split(" ")[] | select(length > 3)] | any(. as $word | $normalized_name | contains($word)))
    ) |
    .id
  ' | head -1
}

# Process each trace
VERDICTS=()
PROCESSED=0
PASS_COUNT=0
WEAK_COUNT=0
FAIL_COUNT=0
ERROR_COUNT=0

for trace_zip in "${TRACE_ZIPS[@]}"; do
  trace_name=$(basename "$trace_zip")
  PROCESSED=$((PROCESSED + 1))
  echo "[$PROCESSED/${#TRACE_ZIPS[@]}] Processing: $trace_name" >&2

  # Step 1: Extract trace
  extracted_file="$OUTPUT_DIR/extracted/${trace_name%.zip}.json"
  echo "  Extracting..." >&2
  if ! "$SCRIPT_DIR/extract-trace.sh" "$trace_zip" \
    --screenshots-dir "$OUTPUT_DIR/screenshots/${trace_name%.zip}" \
    > "$extracted_file" 2>/dev/null; then
    echo "  ERROR: Extraction failed, skipping" >&2
    ERROR_COUNT=$((ERROR_COUNT + 1))
    continue
  fi

  action_count=$(jq '.summary.total_actions' "$extracted_file")
  network_count=$(jq '.summary.total_network_calls' "$extracted_file")
  echo "  Extracted: $action_count actions, $network_count network calls" >&2

  # Step 2: Match to expectation
  matched_id=$(match_expectation "$trace_name")
  if [[ -n "$matched_id" ]]; then
    expectation_json=$(echo "$EXPECTATIONS" | jq --arg id "$matched_id" '.expectations[] | select(.id == $id)')
    echo "  Matched expectation: $matched_id" >&2
  else
    # No match — create a generic expectation asking the model to describe what it sees
    echo "  No expectation match — using generic grading" >&2
    expectation_json=$(jq -n \
      --arg name "$trace_name" \
      '{
        id: "unmatched",
        name: $name,
        category: "unknown",
        spec_section: "Unknown",
        feature_area: "unknown",
        expected_behavior: "Describe what this test demonstrates based on the trace evidence. Grade PASS if the test appears to exercise a meaningful feature successfully, WEAK if it runs but does not verify much, FAIL if it errors or demonstrates nothing.",
        key_assertions: ["Test completes without errors", "At least one meaningful user action is performed", "Network calls return success status codes"],
        ui_indicators: [],
        network_indicators: []
      }')
  fi

  # Step 3: Grade
  verdict_file="$OUTPUT_DIR/verdicts/${trace_name%.zip}-verdict.json"
  echo "  Grading..." >&2
  if ! "$SCRIPT_DIR/grade-trace.sh" \
    --trace-json "$(cat "$extracted_file")" \
    --expectation-json "$expectation_json" \
    > "$verdict_file" 2>/dev/null; then
    echo "  ERROR: Grading failed" >&2
    ERROR_COUNT=$((ERROR_COUNT + 1))
    continue
  fi

  verdict=$(jq -r '.verdict' "$verdict_file")
  confidence=$(jq -r '.confidence' "$verdict_file")
  echo "  Verdict: $verdict (confidence: $confidence)" >&2

  case "$verdict" in
    PASS) PASS_COUNT=$((PASS_COUNT + 1)) ;;
    WEAK) WEAK_COUNT=$((WEAK_COUNT + 1)) ;;
    FAIL) FAIL_COUNT=$((FAIL_COUNT + 1)) ;;
    *)    ERROR_COUNT=$((ERROR_COUNT + 1)) ;;
  esac

  VERDICTS+=("$verdict_file")
  echo "" >&2
done

# Step 4: Compute composite score
GRADED=$((PASS_COUNT + WEAK_COUNT + FAIL_COUNT))
if [[ $GRADED -eq 0 ]]; then
  echo "ERROR: No traces were successfully graded" >&2
  exit 1
fi

# Score formula:
#   PASS  = 1.0 points
#   WEAK  = 0.4 points (feature works but test doesn't prove it well)
#   FAIL  = 0.0 points
#   Normalize to 0-100
SCORE=$(echo "scale=1; ($PASS_COUNT * 100 + $WEAK_COUNT * 40) / $GRADED" | bc)

# Coverage: unique feature areas with at least one PASS
FEATURES_COVERED=$(cat "$OUTPUT_DIR/verdicts"/*-verdict.json 2>/dev/null | \
  jq -s '[.[] | select(.verdict == "PASS") | .expectation_id | split("-")[0]] | unique | length' 2>/dev/null || echo 0)
TOTAL_FEATURES=$(echo "$EXPECTATIONS" | jq '[.expectations[].feature_area] | unique | length')

# Build the full report
ALL_VERDICTS=$(cat "$OUTPUT_DIR/verdicts"/*-verdict.json 2>/dev/null | jq -s '.' || echo "[]")

REPORT=$(jq -n \
  --argjson verdicts "$ALL_VERDICTS" \
  --argjson score "$SCORE" \
  --argjson pass "$PASS_COUNT" \
  --argjson weak "$WEAK_COUNT" \
  --argjson fail "$FAIL_COUNT" \
  --argjson errors "$ERROR_COUNT" \
  --argjson graded "$GRADED" \
  --argjson total_traces "${#TRACE_ZIPS[@]}" \
  --argjson features_covered "$FEATURES_COVERED" \
  --argjson total_features "$TOTAL_FEATURES" \
  --arg generated_at "$(date -Iseconds)" \
  '{
    generated_at: $generated_at,
    score: {
      composite: $score,
      max: 100,
      formula: "PASS=1.0, WEAK=0.4, FAIL=0.0, normalized to 0-100"
    },
    summary: {
      total_traces: $total_traces,
      graded: $graded,
      pass: $pass,
      weak: $weak,
      fail: $fail,
      errors: $errors,
      pass_rate_pct: (if $graded > 0 then (($pass * 100) / $graded | floor) else 0 end),
      features_covered: $features_covered,
      total_features: $total_features
    },
    verdicts: $verdicts,
    concerns: [
      $verdicts[] | select(.verdict == "WEAK" or .verdict == "FAIL") |
      { expectation: .expectation_name, verdict: .verdict, issues: .concerns }
    ]
  }')

REPORT_FILE="$OUTPUT_DIR/eval-report.json"
echo "$REPORT" | jq '.' > "$REPORT_FILE"

# Print summary to stderr
echo "=== Eval Results ===" >&2
echo "Score:    $SCORE / 100" >&2
echo "PASS:     $PASS_COUNT" >&2
echo "WEAK:     $WEAK_COUNT" >&2
echo "FAIL:     $FAIL_COUNT" >&2
echo "Errors:   $ERROR_COUNT" >&2
echo "Coverage: $FEATURES_COVERED / $TOTAL_FEATURES feature areas" >&2
echo "" >&2
echo "Full report: $REPORT_FILE" >&2

# Print concerns
if [[ $(echo "$REPORT" | jq '.concerns | length') -gt 0 ]]; then
  echo "" >&2
  echo "=== Concerns ===" >&2
  echo "$REPORT" | jq -r '.concerns[] | "  [\(.verdict)] \(.expectation): \(.issues[0] // "no details")"' >&2
fi

# Output report path (for piping into other scripts)
echo "$REPORT_FILE"
