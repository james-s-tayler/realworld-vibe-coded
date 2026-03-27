#!/bin/bash
INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name // empty')
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if [ "$TOOL_NAME" != "Bash" ]; then
  exit 0
fi

if ! echo "$COMMAND" | grep -qP '^\./build\.sh\s+\S'; then
  exit 0
fi

TARGET=$(echo "$COMMAND" | grep -oP '(?<=^\./build\.sh\s)\S+')
TIMESTAMP=$(date -u +%Y-%m-%dT%H:%M:%SZ)
STATS_FILE="Reports/agent-stats.csv"

if [ ! -f "$STATS_FILE" ]; then
  mkdir -p Reports
  echo "timestamp,target,result" > "$STATS_FILE"
fi

echo "${TIMESTAMP},${TARGET},fail" >> "$STATS_FILE"

# Emit the on-nuke-test-fail behavior for Test* targets
if echo "$COMMAND" | grep -qP '^\./build\.sh\s+Test'; then
  jq -n '{
    hookSpecificOutput: {
      hookEventName: "PostToolUseFailure",
      additionalContext: "The failed test command output contains specific instructions on where to find reports and logs. Read those instructions carefully and follow them to investigate the failure before proceeding."
    }
  }'
fi

exit 0
