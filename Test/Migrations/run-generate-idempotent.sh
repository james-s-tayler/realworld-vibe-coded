#!/bin/bash
set -e

CONTEXT="${DBCONTEXT:-AppDbContext}"

echo "Generating idempotent SQL script for ${CONTEXT}..."
dotnet ef migrations script --idempotent \
  --context "${CONTEXT}" \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --no-build \
  --output /output/idempotent.sql

echo "✓ Idempotent SQL script generated successfully for ${CONTEXT}!"
