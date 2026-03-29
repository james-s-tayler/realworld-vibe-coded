#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Only check ./build.sh commands
if ! echo "$COMMAND" | grep -qP '^\./build\.sh\s'; then
  exit 0
fi

# Require --agent flag
if ! echo "$COMMAND" | grep -q -- '--agent'; then
  echo "BLOCKED: Always pass --agent flag when running Nuke targets (e.g., ./build.sh TestServerPostmanAuth --agent). This suppresses verbose Docker output for context efficiency." >&2
  exit 2
fi

exit 0
