#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Mark when ./build.sh BuildServer, BuildClient, or Test* succeeds
if echo "$COMMAND" | grep -qP '^\./build\.sh\s+(Build(Server|Client)|Test)'; then
  # Record which command was used as the gate
  echo "$COMMAND" > /tmp/claude-tests-ran
fi

exit 0
