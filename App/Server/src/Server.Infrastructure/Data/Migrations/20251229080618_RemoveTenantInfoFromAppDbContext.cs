using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class RemoveTenantInfoFromAppDbContext : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // Drop the FK constraint that's preventing us from dropping TenantInfo table
    // This handles the case where an earlier version of migration 20251229020012 ran without dropping the FK
    migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_Organizations_TenantId')
            BEGIN
                ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Organizations_TenantId];
            END
        ");

    // Drop TenantInfo table if it exists (should only be in TenantStoreDbContext)
    migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.objects WHERE name = 'TenantInfo' AND type = 'U')
            BEGIN
                DROP TABLE [TenantInfo];
            END
        ");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "TenantInfo",
        columns: table => new
        {
          Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
          Identifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
          Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TenantInfo", x => x.Id);
        });
  }
}
