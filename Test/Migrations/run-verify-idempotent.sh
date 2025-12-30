#!/bin/bash
set -e

CONTEXT="${DBCONTEXT:-AppDbContext}"

echo "Verifying idempotent SQL script for ${CONTEXT}..."

if [ ! -f /committed/idempotent.sql ]; then
  echo "Error: Committed idempotent SQL script not found for ${CONTEXT}"
  exit 1
fi

dotnet ef migrations script --idempotent \
  --context "${CONTEXT}" \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --no-build \
  --output /tmp/generated-idempotent.sql

if ! diff -q /committed/idempotent.sql /tmp/generated-idempotent.sql > /dev/null; then
  echo "Error: Idempotent SQL script is out of date for ${CONTEXT}"
  echo "Run 'nuke DbMigrationsGenerateIdempotentScript --db-context ${CONTEXT}' to regenerate it"
  exit 1
fi

echo "✓ Idempotent SQL script is up to date for ${CONTEXT}"
