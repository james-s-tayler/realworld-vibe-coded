#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if echo "$COMMAND" | grep -qE '^\s*dotnet\s+(build|test|run|publish|ef|format)'; then
  echo "BLOCKED: Do not run dotnet commands directly. Use ./build.sh <target> instead. Key targets: LintAllVerify, LintAllFix, BuildServer, BuildClient, TestServer, TestClient, TestE2e, RunLocalHotReload, RunLocalPublish, DbMigrations*" >&2
  exit 2
fi
exit 0
