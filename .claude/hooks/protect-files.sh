#!/bin/bash
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

PROTECTED_PATTERNS=(
  "App/Server/analyzers/"
  ".editorconfig"
  ".husky/"
  ".nuke/"
  ".claude/"
  "Nuke.Tests/"
)

for pattern in "${PROTECTED_PATTERNS[@]}"; do
  if [[ "$FILE_PATH" == *"$pattern"* ]]; then
    echo "{
      \"hookSpecificOutput\": {
        \"hookEventName\": \"PreToolUse\",
        \"permissionDecision\": \"ask\",
        \"permissionDecisionReason\": \"'$FILE_PATH' is a protected file (matches '$pattern'). Only modify if explicitly instructed by the user.\"
      }
    }"
    exit 0
  fi
done
exit 0
