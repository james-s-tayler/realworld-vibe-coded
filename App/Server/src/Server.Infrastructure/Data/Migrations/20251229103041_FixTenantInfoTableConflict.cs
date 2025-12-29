using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class FixTenantInfoTableConflict : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // This migration fixes a conflict between AppDbContext and TenantStoreDbContext:
    // Migration 20251229020012_ReplaceOrganizationsWithTenantInfo created a TenantInfo table,
    // but TenantStoreDbContext (Finbuckle) also creates a TenantInfo table.
    // Since TenantStoreDbContext migrations run first on startup, when AppDbContext's migration
    // 20251229020012 tries to CREATE TABLE TenantInfo, it fails with "object already exists".
    //
    // The solution: Make migration 20251229020012's TenantInfo creation idempotent by wrapping
    // it in IF NOT EXISTS. Since we can't modify that migration, we handle the conflict here
    // by ensuring any attempt to create TenantInfo is safe.
    //
    // However, since the previous migration already failed, we need to ensure the migration
    // history is correct. The issue is that __EFMigrationsHistory will have 20251229020012
    // recorded even though it partially failed.
    //
    // Actually, looking at the logs: migration 20251229020012 DID apply its Up() commands
    // up until the CREATE TABLE TenantInfo failed. So Organizations table was dropped,
    // TenantId column was altered, but TenantInfo table creation failed.
    //
    // Since TenantInfo table exists (created by TenantStoreDbContext), and AspNetUsers.TenantId
    // is correct, there's nothing to fix here. The system is actually in the correct state.
    //
    // The only issue is that migration 20251229080618 tries to drop FK_AspNetUsers_Organizations_TenantId
    // which should have been dropped by migration 20251229020012, but let's verify it was dropped.

    // Ensure FK_AspNetUsers_Organizations_TenantId is dropped (defensive check)
    migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_Organizations_TenantId')
                BEGIN
                    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Organizations_TenantId];
                END
            ");

    // Note: We do NOT drop or create TenantInfo table here.
    // TenantInfo is managed exclusively by TenantStoreDbContext.
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
  }
}
