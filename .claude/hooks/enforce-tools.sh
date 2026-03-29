#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Block find/grep/rg as the leading command (not when piped from another command)
# Matches: "find .", "find /path", "&&  find", ";find" but NOT "cmd | grep"
if echo "$COMMAND" | grep -qE '(^|&&|;)\s*find\b'; then
  echo "BLOCKED: Do not use 'find' in Bash. Use the Glob tool instead (e.g., Glob **/*.cs)." >&2
  exit 2
fi

if echo "$COMMAND" | grep -qE '(^|&&|;)\s*(grep|rg)\b'; then
  echo "BLOCKED: Do not use 'grep' or 'rg' in Bash. Use the Grep tool instead." >&2
  exit 2
fi

exit 0
