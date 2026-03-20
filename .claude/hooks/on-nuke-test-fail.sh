#!/bin/bash
INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name // empty')
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Only handle ./build.sh Test* failures
if [ "$TOOL_NAME" != "Bash" ]; then
  exit 0
fi

if ! echo "$COMMAND" | grep -qP '^\./build\.sh\s+Test'; then
  exit 0
fi

jq -n '{
  hookSpecificOutput: {
    hookEventName: "PostToolUseFailure",
    additionalContext: "The failed test command output contains specific instructions on where to find reports and logs. Read those instructions carefully and follow them to investigate the failure before proceeding."
  }
}'
exit 0
