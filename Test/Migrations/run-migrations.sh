#!/bin/bash
set -e

echo "Waiting for SQL Server to be ready..."
sleep 5

echo "Running EF Core migrations for IdentityDbContext..."
dotnet ef database update \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --context IdentityDbContext \
  --connection "$ConnectionStrings__DefaultConnection"

echo "Running EF Core migrations for DomainDbContext..."
dotnet ef database update \
  --project /app/App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj \
  --startup-project /app/App/Server/src/Server.Web/Server.Web.csproj \
  --context DomainDbContext \
  --connection "$ConnectionStrings__DefaultConnection"

echo "✓ Migrations applied successfully!"
