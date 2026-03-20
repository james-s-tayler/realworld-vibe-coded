#!/bin/bash
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

PROTECTED_PATTERNS=(
  "App/Server/analyzers/"
  ".editorconfig"
  ".husky/"
  ".nuke/"
)

for pattern in "${PROTECTED_PATTERNS[@]}"; do
  if [[ "$FILE_PATH" == *"$pattern"* ]]; then
    echo "BLOCKED: '$FILE_PATH' is a protected file (matches '$pattern'). Only modify if explicitly instructed by the user." >&2
    exit 2
  fi
done
exit 0
