#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if echo "$COMMAND" | grep -qE 'claude-tests-ran'; then
  echo "BLOCKED: Do not manipulate test markers directly. Run actual tests via ./build.sh." >&2
  exit 2
fi

if echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*dotnet\b'; then
  echo "BLOCKED: Do not run dotnet commands directly. Use ./build.sh <target> instead. Key targets: LintAllVerify, LintAllFix, BuildServer, BuildClient, TestServer, TestClient, TestE2e, RunLocalPublish, DbMigrations*" >&2
  exit 2
fi

if echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*docker(-compose)?\b' || echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*docker\s+compose\b'; then
  echo "BLOCKED: Do not run docker/docker-compose commands directly. Use ./build.sh <target> instead. Key targets: RunLocalPublish, RunLocalDependencies, RunLocalDependenciesDown, DbReset, TestServerPostman*, TestE2e" >&2
  exit 2
fi

exit 0
