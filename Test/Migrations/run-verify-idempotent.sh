#!/bin/bash
set -e

echo "Verifying idempotent SQL script..."

if [ ! -f /committed/idempotent.sql ]; then
  echo "Error: Committed idempotent SQL script not found"
  exit 1
fi

dotnet ef migrations script --idempotent \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --no-build \
  --output /tmp/generated-idempotent.sql

if ! diff -q /committed/idempotent.sql /tmp/generated-idempotent.sql > /dev/null; then
  echo "Error: Idempotent SQL script is out of date"
  echo "Run 'nuke DbMigrationsGenerateIdempotentScript' to regenerate it"
  exit 1
fi

echo "✓ Idempotent SQL script is up to date"
