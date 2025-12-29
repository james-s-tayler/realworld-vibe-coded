using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class RemoveTenantInfoFromAppDbContext : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // Drop foreign key constraint if it exists (from old migrations)
    migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_TenantInfo_TenantId')
            BEGIN
                ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_TenantInfo_TenantId];
            END
        ");

    // Drop TenantInfo table if it exists (should only be in TenantStoreDbContext)
    migrationBuilder.DropTable(
        name: "TenantInfo");
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
