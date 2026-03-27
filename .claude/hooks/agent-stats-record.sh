#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if ! echo "$COMMAND" | grep -qP '^\./build\.sh\s+\S'; then
  exit 0
fi

TARGET=$(echo "$COMMAND" | grep -oP '(?<=^\./build\.sh\s)\S+')
TIMESTAMP=$(date -u +%Y-%m-%dT%H:%M:%SZ)
STATS_FILE="Reports/agent-stats.csv"

# Create header if file doesn't exist
if [ ! -f "$STATS_FILE" ]; then
  mkdir -p Reports
  echo "timestamp,target,result" > "$STATS_FILE"
fi

echo "${TIMESTAMP},${TARGET},pass" >> "$STATS_FILE"
exit 0
