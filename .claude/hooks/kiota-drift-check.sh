#!/bin/bash
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

if ! echo "$FILE_PATH" | grep -q 'Server.Web/'; then
  exit 0
fi

touch /tmp/claude-kiota-dirty
exit 0
