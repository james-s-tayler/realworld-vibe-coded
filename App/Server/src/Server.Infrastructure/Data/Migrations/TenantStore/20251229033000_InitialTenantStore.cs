using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations.TenantStore;

/// <inheritdoc />
public partial class InitialTenantStore : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "TenantInfo",
      columns: table => new
      {
        Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
        Identifier = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
        Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_TenantInfo", x => x.Id);
      });

    migrationBuilder.CreateIndex(
      name: "IX_TenantInfo_Identifier",
      table: "TenantInfo",
      column: "Identifier",
      unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "TenantInfo");
  }
}
