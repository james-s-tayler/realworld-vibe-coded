#!/bin/bash
# run-experiment.sh — Worktree setup, claude -p invocation, test execution
# Usage: ./scripts/run-experiment.sh [--keep] [--timeout SECONDS] [--worktree-dir DIR]
#
# Creates a git worktree, runs Claude Code with the starter prompt,
# then runs all test suites. Captures wall time, test results, and token usage.
#
# Options:
#   --keep            Don't clean up worktree after completion
#   --timeout SECS    Agent timeout in seconds (default: 5400 = 90 min)
#   --worktree-dir    Base directory for worktrees (default: /tmp/harness-runs)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Defaults
KEEP=false
TIMEOUT=5400
WORKTREE_BASE="/tmp/harness-runs"

# Parse args
while [[ $# -gt 0 ]]; do
  case $1 in
    --keep) KEEP=true; shift ;;
    --timeout) TIMEOUT="$2"; shift 2 ;;
    --worktree-dir) WORKTREE_BASE="$2"; shift 2 ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

# Generate unique run ID
RUN_ID="run-$(date +%Y%m%d-%H%M%S)-$$"
WORKTREE_DIR="$WORKTREE_BASE/$RUN_ID"
RESULTS_FILE="$WORKTREE_BASE/$RUN_ID-results.json"

echo "=== Experiment: $RUN_ID ==="
echo "Worktree: $WORKTREE_DIR"
echo "Timeout: ${TIMEOUT}s"

# Create worktree
mkdir -p "$WORKTREE_BASE"
BRANCH_NAME="experiment/$RUN_ID"
git -C "$REPO_ROOT" worktree add -b "$BRANCH_NAME" "$WORKTREE_DIR" HEAD

echo "Worktree created at $WORKTREE_DIR"

# Copy settings.json but rewrite absolute paths to point to worktree
mkdir -p "$WORKTREE_DIR/.claude"
if [[ -f "$REPO_ROOT/.claude/settings.json" ]]; then
  sed "s|$REPO_ROOT|$WORKTREE_DIR|g" "$REPO_ROOT/.claude/settings.json" > "$WORKTREE_DIR/.claude/settings.json"
fi

# Record start time
START_TIME=$(date +%s)

# Run Claude Code agent
AGENT_OUTPUT_FILE="$WORKTREE_BASE/$RUN_ID-agent-output.json"
AGENT_EXIT_CODE=0
echo "Starting Claude Code agent (timeout: ${TIMEOUT}s)..."

# Unset CLAUDECODE to allow nested invocation from within a Claude Code session
env -u CLAUDECODE timeout "$TIMEOUT" claude -p "$(cat "$WORKTREE_DIR/scripts/starter-prompt.md")" \
  --dangerously-skip-permissions \
  --output-format json \
  --cwd "$WORKTREE_DIR" \
  > "$AGENT_OUTPUT_FILE" 2>&1 || AGENT_EXIT_CODE=$?

AGENT_END_TIME=$(date +%s)
AGENT_DURATION=$((AGENT_END_TIME - START_TIME))

echo "Agent completed in ${AGENT_DURATION}s (exit code: $AGENT_EXIT_CODE)"

# Extract token usage from agent output if available
TOTAL_INPUT_TOKENS=0
TOTAL_OUTPUT_TOKENS=0
if [[ -f "$AGENT_OUTPUT_FILE" && -s "$AGENT_OUTPUT_FILE" ]]; then
  # Try to parse JSON output for token usage
  TOTAL_INPUT_TOKENS=$(jq -r '.usage.input_tokens // 0' "$AGENT_OUTPUT_FILE" 2>/dev/null || echo "0")
  TOTAL_OUTPUT_TOKENS=$(jq -r '.usage.output_tokens // 0' "$AGENT_OUTPUT_FILE" 2>/dev/null || echo "0")
fi

# Run all test suites in the worktree
echo "Running test suites..."
TEST_EXIT_CODES=()
SUITES=("TestServerPostmanAuth" "TestServerPostmanProfiles" "TestServerPostmanArticlesEmpty" "TestServerPostmanArticle" "TestServerPostmanFeedAndArticles" "TestE2e")

for suite in "${SUITES[@]}"; do
  echo "  Running $suite..."
  suite_exit=0
  (cd "$WORKTREE_DIR" && ./build.sh "$suite" 2>&1) || suite_exit=$?
  TEST_EXIT_CODES+=("$suite:$suite_exit")
  echo "  $suite: exit $suite_exit"
done

TEST_END_TIME=$(date +%s)
TOTAL_DURATION=$((TEST_END_TIME - START_TIME))

# Parse test results
echo "Parsing results..."
TEST_RESULTS=$("$WORKTREE_DIR/scripts/parse-results.sh" "$WORKTREE_DIR")

# Build suite exit codes JSON
suite_exits_json="{"
first=true
for entry in "${TEST_EXIT_CODES[@]}"; do
  suite_name="${entry%%:*}"
  exit_code="${entry##*:}"
  if [[ "$first" == "true" ]]; then
    first=false
  else
    suite_exits_json+=","
  fi
  suite_exits_json+="\"$suite_name\":$exit_code"
done
suite_exits_json+="}"

# Write results
cat > "$RESULTS_FILE" <<EOF
{
  "run_id": "$RUN_ID",
  "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "agent_duration_seconds": $AGENT_DURATION,
  "total_duration_seconds": $TOTAL_DURATION,
  "agent_exit_code": $AGENT_EXIT_CODE,
  "agent_timed_out": $([ "$AGENT_EXIT_CODE" -eq 124 ] && echo "true" || echo "false"),
  "tokens": {
    "input": $TOTAL_INPUT_TOKENS,
    "output": $TOTAL_OUTPUT_TOKENS
  },
  "suite_exit_codes": $suite_exits_json,
  "test_results": $TEST_RESULTS,
  "worktree": "$WORKTREE_DIR",
  "branch": "$BRANCH_NAME"
}
EOF

echo "Results written to $RESULTS_FILE"

# Cleanup
if [[ "$KEEP" == "false" ]]; then
  echo "Cleaning up worktree..."
  git -C "$REPO_ROOT" worktree remove --force "$WORKTREE_DIR" 2>/dev/null || true
  git -C "$REPO_ROOT" branch -D "$BRANCH_NAME" 2>/dev/null || true
  echo "Worktree cleaned up"
else
  echo "Worktree preserved at $WORKTREE_DIR (branch: $BRANCH_NAME)"
fi

echo "=== Experiment $RUN_ID complete ==="
echo "Duration: agent=${AGENT_DURATION}s total=${TOTAL_DURATION}s"
echo "Results: $RESULTS_FILE"

# Output the results file path for piping
echo "$RESULTS_FILE"
