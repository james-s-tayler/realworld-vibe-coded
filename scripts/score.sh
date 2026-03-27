#!/bin/bash
# score.sh — Orchestrates: run-experiment -> parse-results -> alignment-audit -> compute score
# Usage: ./scripts/score.sh [--keep] [--timeout SECONDS] [--skip-experiment] [--results-file FILE]
#
# Outputs JSON:
# {
#   "score": N,
#   "max": 100,
#   "components": { "tests": N, "alignment": N, "time": N },
#   "test_results": {...},
#   "alignment": {...},
#   "run": {...}
# }
#
# Options:
#   --keep              Don't clean up worktree after completion
#   --timeout SECS      Agent timeout in seconds (default: 5400)
#   --skip-experiment   Skip running experiment, use existing results file
#   --results-file FILE Use this results file instead of running experiment
#   --audit-only        Only run alignment audit against current repo (no experiment)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Defaults
KEEP_FLAG=""
TIMEOUT_FLAG=""
SKIP_EXPERIMENT=false
RESULTS_FILE=""
AUDIT_ONLY=false

# Parse args
while [[ $# -gt 0 ]]; do
  case $1 in
    --keep) KEEP_FLAG="--keep"; shift ;;
    --timeout) TIMEOUT_FLAG="--timeout $2"; shift 2 ;;
    --skip-experiment) SKIP_EXPERIMENT=true; shift ;;
    --results-file) RESULTS_FILE="$2"; shift 2 ;;
    --audit-only) AUDIT_ONLY=true; shift ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

# Audit-only mode: just run alignment audit and output
if [[ "$AUDIT_ONLY" == "true" ]]; then
  ALIGNMENT=$("$SCRIPT_DIR/alignment-audit.sh" "$REPO_ROOT")
  alignment_score=$(echo "$ALIGNMENT" | jq '.score')

  cat <<EOF
{
  "score": $alignment_score,
  "max": 30,
  "mode": "audit-only",
  "alignment": $ALIGNMENT
}
EOF
  exit 0
fi

# Step 1: Run experiment (or use existing results)
if [[ "$SKIP_EXPERIMENT" == "true" && -n "$RESULTS_FILE" ]]; then
  echo "Using existing results: $RESULTS_FILE" >&2
elif [[ "$SKIP_EXPERIMENT" == "false" ]]; then
  echo "Running experiment..." >&2
  EXPERIMENT_OUTPUT=$("$SCRIPT_DIR/run-experiment.sh" $KEEP_FLAG $TIMEOUT_FLAG 2>&1)
  # Last line of output is the results file path
  RESULTS_FILE=$(echo "$EXPERIMENT_OUTPUT" | tail -1)
  echo "Experiment complete. Results: $RESULTS_FILE" >&2
fi

if [[ ! -f "$RESULTS_FILE" ]]; then
  echo "ERROR: Results file not found: $RESULTS_FILE" >&2
  exit 1
fi

# Step 2: Extract test results from experiment
RUN_DATA=$(cat "$RESULTS_FILE")
TEST_RESULTS=$(echo "$RUN_DATA" | jq '.test_results')
ALL_PASSING=$(echo "$TEST_RESULTS" | jq '.summary.all_passing')
TOTAL_TESTS=$(echo "$TEST_RESULTS" | jq '.summary.total')
PASSED_TESTS=$(echo "$TEST_RESULTS" | jq '.summary.passed')
AGENT_DURATION=$(echo "$RUN_DATA" | jq '.agent_duration_seconds')

# Step 3: Compute test score
if [[ "$ALL_PASSING" == "true" ]]; then
  test_score=50
else
  # Partial: floor(pass/total * 45), max 45
  if [[ "$TOTAL_TESTS" -gt 0 ]]; then
    test_score=$(( (PASSED_TESTS * 45) / TOTAL_TESTS ))
  else
    test_score=0
  fi
fi

# Step 4: Run alignment audit against the source repo (not the worktree)
echo "Running alignment audit..." >&2
ALIGNMENT=$("$SCRIPT_DIR/alignment-audit.sh" "$REPO_ROOT")
alignment_score=$(echo "$ALIGNMENT" | jq '.score')

# Step 5: Compute time bonus (only if all tests pass)
# max(0, floor(20 * (90 - min) / 60)). 30min=20, 60min=10, 90min=0
time_score=0
if [[ "$ALL_PASSING" == "true" ]]; then
  agent_minutes=$(( AGENT_DURATION / 60 ))
  if [[ "$agent_minutes" -lt 90 ]]; then
    time_score=$(( 20 * (90 - agent_minutes) / 60 ))
    if [[ "$time_score" -gt 20 ]]; then
      time_score=20
    fi
    if [[ "$time_score" -lt 0 ]]; then
      time_score=0
    fi
  fi
fi

# Step 6: Compute total score
total_score=$((test_score + alignment_score + time_score))

# Step 7: Output
cat <<EOF
{
  "score": $total_score,
  "max": 100,
  "components": {
    "tests": $test_score,
    "alignment": $alignment_score,
    "time": $time_score
  },
  "test_results": $TEST_RESULTS,
  "alignment": $ALIGNMENT,
  "run": $RUN_DATA
}
EOF

# Step 8: Append to iterations.jsonl
ITERATION_LINE=$(cat <<EOF2
{"timestamp":"$(date -u +%Y-%m-%dT%H:%M:%SZ)","score":$total_score,"max":100,"tests":$test_score,"alignment":$alignment_score,"time":$time_score,"agent_duration":$AGENT_DURATION,"total_tests":$TOTAL_TESTS,"passed_tests":$PASSED_TESTS,"all_passing":$ALL_PASSING,"run_id":"$(echo "$RUN_DATA" | jq -r '.run_id')"}
EOF2
)
echo "$ITERATION_LINE" >> "$REPO_ROOT/iterations.jsonl"
echo "Iteration recorded in iterations.jsonl" >&2
