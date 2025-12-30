#!/bin/bash
set -e

echo "Generating idempotent SQL script..."
dotnet ef migrations script --idempotent \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --no-build \
  --output /output/idempotent.sql

echo "✓ Idempotent SQL script generated successfully!"
