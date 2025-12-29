---
name: ef-database-migrations
description: >
  Guidelines for creating and managing Entity Framework Core database migrations
  in this repository. Enforces best practices for migration safety and immutability.
license: MIT
---

# Entity Framework Core Database Migrations Skill

This skill defines the rules and procedures for working with EF Core database migrations in this repository.

## Core Principles

### 1. Migration Immutability
**NEVER modify existing migrations once they have been applied in ANY environment (dev, test, staging, production).**

- Once a migration is committed to version control and potentially applied, it is immutable
- Modifying existing migrations causes inconsistencies between environments
- If a migration is wrong, create a NEW migration to fix it

### 2. Use dotnet ef CLI Tool
**ALWAYS use the `dotnet ef` global tool to create migrations.**

- Never manually create migration files
- Never copy/paste/modify migration code
- Let EF Core generate migrations from model changes

### 3. Test All Migrations
**ALWAYS test migrations with `./build.sh DbMigrationsVerifyAll` before committing.**

- This verifies migrations can be applied cleanly
- Catches migration conflicts early
- Ensures idempotent SQL script is up-to-date

## Prerequisites

### Install dotnet ef Tool

Before creating migrations, ensure `dotnet ef` is installed:

```bash
# Check if installed
dotnet ef --version

# Install if needed (version 9.0.0 or compatible with .NET 9)
dotnet tool install --global dotnet-ef --version 9.0.0

# Update if already installed
dotnet tool update --global dotnet-ef
```

## When to Use This Skill

Use this skill whenever you:

- Need to create a new database migration
- Are modifying entity models or DbContext configuration
- Need to understand migration best practices
- Are troubleshooting migration issues

## Procedure: Creating a New Migration

### Step 1: Make Model Changes

First, make your changes to:
- Entity classes
- DbContext configuration
- Entity type configurations

### Step 2: Navigate to Infrastructure Project

```bash
cd /path/to/App/Server/src/Server.Infrastructure
```

### Step 3: Create Migration Using dotnet ef

For **AppDbContext** migrations:

```bash
dotnet ef migrations add YourMigrationName \
  --context AppDbContext \
  --startup-project ../Server.Web \
  --output-dir Data/Migrations
```

For **TenantStoreDbContext** migrations:

```bash
dotnet ef migrations add YourMigrationName \
  --context TenantStoreDbContext \
  --startup-project ../Server.Web \
  --output-dir Data/Migrations/TenantStore
```

**Migration Naming Convention:**
- Use PascalCase descriptive names
- Be specific about what the migration does
- Examples: `AddArticlePublishedDateColumn`, `RemoveObsoleteUserFields`, `CreateCommentsTable`

### Step 4: Review Generated Migration

Carefully review the generated migration files:
- `<timestamp>_<MigrationName>.cs` - The migration itself
- `<timestamp>_<MigrationName>.Designer.cs` - Migration metadata
- `<DbContext>ModelSnapshot.cs` - Updated model snapshot

Check for:
- Correct column types and constraints
- Proper foreign key relationships
- Data loss warnings (dropping columns, tables)
- Unexpected changes

### Step 5: Regenerate Idempotent SQL Script

After adding a migration, regenerate the idempotent SQL script:

```bash
cd /path/to/App/Server/src/Server.Infrastructure

# For AppDbContext
dotnet ef migrations script \
  --context AppDbContext \
  --startup-project ../Server.Web \
  --idempotent \
  --output Data/Migrations/idempotent.sql
```

### Step 6: Test Migrations

**REQUIRED**: Test migrations before committing:

```bash
cd /path/to/repository/root
./build.sh DbMigrationsVerifyAll
```

This command:
- Verifies all migrations can be applied to a fresh database
- Tests both AppDbContext and TenantStoreDbContext migrations
- Validates the idempotent SQL script
- Reports any errors

### Step 7: Commit Migration Files

If tests pass, commit the migration files:

```bash
git add App/Server/src/Server.Infrastructure/Data/Migrations/
git commit -m "Add migration: YourMigrationName"
```

## Common Scenarios

### Scenario 1: Need to Fix a Wrong Migration

**WRONG Approach:**
```bash
# ❌ NEVER DO THIS
# Edit existing migration file
```

**CORRECT Approach:**
```bash
# ✅ Create a new migration to fix the issue
dotnet ef migrations add FixPreviousMigrationIssue --context AppDbContext --startup-project ../Server.Web
```

### Scenario 2: Migration Conflicts Between Branches

If you encounter migration conflicts when merging/rebasing:

1. **DO NOT** modify existing migration timestamps
2. Create a new merge migration if needed:
   ```bash
   dotnet ef migrations add MergeMigration --context AppDbContext --startup-project ../Server.Web
   ```
3. Run `./build.sh DbMigrationsVerifyAll` to verify

### Scenario 3: Need to Remove a Migration (Not Yet Applied Anywhere)

If migration is ONLY in your local branch and NEVER applied:

```bash
# Remove the last migration
dotnet ef migrations remove --context AppDbContext --startup-project ../Server.Web

# Make model changes if needed
# Create new migration
dotnet ef migrations add CorrectedMigration --context AppDbContext --startup-project ../Server.Web
```

**IMPORTANT**: Only use `migrations remove` if the migration has NEVER been applied in any environment.

### Scenario 4: Separate DbContexts with Different Connection Strings

This repository uses TWO DbContexts:
- **AppDbContext**: Application data (Articles, Comments, Users, Roles)
- **TenantStoreDbContext**: Tenant metadata (TenantInfo) - Finbuckle MultiTenant

**Key Rules:**
- ZERO table overlap between DbContexts
- NO foreign keys between DbContexts (cross-DbContext FKs are not supported)
- TenantStoreDbContext migrations run FIRST on startup, then AppDbContext migrations

## Troubleshooting

### "pending model changes" Error

If you see "The model has changes that are pending migration":

1. Review your model changes carefully
2. Create a new migration to capture the changes:
   ```bash
   dotnet ef migrations add CaptureModelChanges --context AppDbContext --startup-project ../Server.Web
   ```
3. Test with `./build.sh DbMigrationsVerifyAll`

### Migration Fails to Apply

If a migration fails during `DbMigrationsVerifyAll`:

1. Check the error message carefully
2. Review the SQL in the migration's `Up()` method
3. Create a new migration to fix the issue (don't modify the failing one)
4. Consider using raw SQL with `IF EXISTS` checks for defensive migrations

### Idempotent SQL Script Out of Sync

If the idempotent SQL script is out of date:

```bash
cd App/Server/src/Server.Infrastructure
dotnet ef migrations script --context AppDbContext --startup-project ../Server.Web --idempotent --output Data/Migrations/idempotent.sql
```

## Migration Safety Checklist

Before committing a migration:

- [ ] Used `dotnet ef migrations add` (not manual file creation)
- [ ] Reviewed generated migration for correctness
- [ ] Regenerated idempotent SQL script
- [ ] Ran `./build.sh DbMigrationsVerifyAll` successfully
- [ ] Committed migration files together with model changes
- [ ] Did NOT modify any existing migrations

## References

- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [dotnet ef CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- Repository CONTRIBUTING.md (if exists)
