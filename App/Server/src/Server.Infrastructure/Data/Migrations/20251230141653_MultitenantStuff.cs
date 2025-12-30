using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class MultitenantStuff : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropIndex(
        name: "IX_AspNetUsers_TenantId",
        table: "AspNetUsers");

    migrationBuilder.AlterColumn<string>(
        name: "TenantId",
        table: "AspNetUsers",
        type: "nvarchar(450)",
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(450)",
        oldNullable: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AlterColumn<string>(
        name: "TenantId",
        table: "AspNetUsers",
        type: "nvarchar(450)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(450)");

    migrationBuilder.CreateIndex(
        name: "IX_AspNetUsers_TenantId",
        table: "AspNetUsers",
        column: "TenantId");
  }
}
