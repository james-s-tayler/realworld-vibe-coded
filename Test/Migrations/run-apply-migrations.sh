#!/bin/bash
set -e

echo "Waiting for SQL Server to be ready..."
sleep 5

echo "Running EF Core migrations for AppDbContext..."
dotnet ef database update \
  --context AppDbContext \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --no-build \
  --connection "$ConnectionStrings__DefaultConnection"

echo "✓ AppDbContext migrations applied successfully!"

echo "Running EF Core migrations for TenantStoreDbContext..."
dotnet ef database update \
  --context TenantStoreDbContext \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --no-build \
  --connection "${ConnectionStrings__TenantStoreConnection:-$ConnectionStrings__DefaultConnection}"

echo "✓ TenantStoreDbContext migrations applied successfully!"

echo "✓ All migrations applied successfully!"
