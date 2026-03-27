#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Track gate steps in /tmp/claude-gate/ directory
if echo "$COMMAND" | grep -qP '^\./build\.sh\s+(LintAllVerify|Build(Server|Client)|Test)'; then
  TARGET=$(echo "$COMMAND" | grep -oP '(?<=^\./build\.sh\s)\S+')
  mkdir -p /tmp/claude-gate
  echo "$COMMAND" > "/tmp/claude-gate/$TARGET"
fi

# Clear Kiota dirty marker when API client is verified or regenerated
if echo "$COMMAND" | grep -qP '^\./build\.sh\s+(LintApiClientVerify|BuildGenerateApiClient)'; then
  rm -f /tmp/claude-kiota-dirty
fi

exit 0
