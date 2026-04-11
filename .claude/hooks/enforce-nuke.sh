#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*dotnet\b'; then
  echo "BLOCKED: Do not run dotnet commands directly. Use ./build.sh <target> instead. Key targets: LintAllVerify, LintAllFix, BuildServer, BuildClient, TestServer, TestClient, TestE2e, RunLocalPublish, DbMigrations*" >&2
  exit 2
fi

if echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*docker-compose\b' || echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*docker\s+compose\b'; then
  echo "BLOCKED: Do not run docker compose commands directly. Use ./build.sh <target> instead. Key targets: RunLocalPublish, RunLocalDependencies, RunLocalDependenciesDown, DbReset, TestE2e" >&2
  exit 2
fi

if echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*npm\s+(run|test)\b'; then
  echo "BLOCKED: Do not run npm scripts directly. Use ./build.sh <target> instead." >&2
  exit 2
fi

if echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*npx\s+(vitest|eslint|stylelint|tsc)\b'; then
  echo "BLOCKED: Do not run frontend tools directly. Use ./build.sh <target> instead. Key targets: TestClient, LintClientVerify, BuildClient" >&2
  exit 2
fi

if echo "$COMMAND" | grep -qE '(^|\||&&|;)\s*node_modules/\.bin/'; then
  echo "BLOCKED: Do not run node_modules/.bin/* directly. Use ./build.sh <target> instead." >&2
  exit 2
fi

exit 0
