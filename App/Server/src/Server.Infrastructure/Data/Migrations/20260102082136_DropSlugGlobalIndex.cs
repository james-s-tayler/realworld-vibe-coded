using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class DropSlugGlobalIndex : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropIndex(
        name: "IX_Articles_Slug_TenantId",
        table: "Articles");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Articles");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
        name: "TenantId",
        table: "Articles",
        type: "nvarchar(64)",
        maxLength: 64,
        nullable: false,
        defaultValue: string.Empty);

    migrationBuilder.CreateIndex(
        name: "IX_Articles_Slug_TenantId",
        table: "Articles",
        columns: new[] { "Slug", "TenantId" },
        unique: true);
  }
}
