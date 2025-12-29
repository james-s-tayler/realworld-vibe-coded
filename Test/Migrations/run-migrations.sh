#!/bin/bash
set -e

echo "Waiting for SQL Server to be ready..."
sleep 5

echo "Running EF Core migrations for TenantStoreDbContext..."
dotnet ef database update \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --context TenantStoreDbContext \
  --connection "$ConnectionStrings__DefaultConnection"

echo "✓ TenantStoreDbContext migrations applied successfully!"

echo "Running EF Core migrations for AppDbContext..."
dotnet ef database update \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --context AppDbContext \
  --connection "$ConnectionStrings__DefaultConnection"

echo "✓ AppDbContext migrations applied successfully!"
echo "✓ All migrations applied successfully!"
