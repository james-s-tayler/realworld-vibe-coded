using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class RemoveTenantInfoFromAppDbContext : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
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
