#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Only intercept git commit commands
if ! echo "$COMMAND" | grep -qP '^git\s+commit\b'; then
  exit 0
fi

TEST_MARKER="/tmp/claude-tests-ran"
CIRCUIT_BREAKER_MARKER="/tmp/claude-circuit-breaker-skip"

# Allow commits when circuit breaker is active (agent is skipping a stuck feature)
if [ -f "$CIRCUIT_BREAKER_MARKER" ]; then
  rm -f "$CIRCUIT_BREAKER_MARKER"
  exit 0
fi

# Check if tests were run since last commit
if [ -f "$TEST_MARKER" ]; then
  rm -f "$TEST_MARKER"
  exit 0
fi

echo "BLOCKED: You must run tests before committing. Run the relevant test command from IMPLEMENTATION-PLAN.md (e.g., ./build.sh TestServerPostmanAuth). If you are using the circuit breaker to skip a stuck feature, create the marker file first: touch /tmp/claude-circuit-breaker-skip" >&2
exit 2
