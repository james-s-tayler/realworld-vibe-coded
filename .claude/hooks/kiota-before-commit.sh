#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if ! echo "$COMMAND" | grep -qP '^git\s+commit\b'; then
  exit 0
fi

if [ -f /tmp/claude-kiota-dirty ]; then
  echo "BLOCKED: Server.Web endpoint files were modified. Run './build.sh BuildGenerateApiClient' then './build.sh LintApiClientVerify' before committing." >&2
  exit 2
fi
exit 0
