#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Only mark when a ./build.sh Test* command succeeds
if echo "$COMMAND" | grep -qP '^\./build\.sh\s+Test'; then
  touch /tmp/claude-tests-ran
fi

exit 0
