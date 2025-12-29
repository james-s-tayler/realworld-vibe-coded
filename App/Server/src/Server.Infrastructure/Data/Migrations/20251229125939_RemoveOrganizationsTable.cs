using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class RemoveOrganizationsTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
        name: "FK_AspNetUsers_Organizations_TenantId",
        table: "AspNetUsers");

    migrationBuilder.Sql("DELETE FROM Organizations WHERE Id = '00000000-0000-0000-0000-000000000001'");

    migrationBuilder.DropTable(
        name: "Organizations");

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
    throw new NotSupportedException("This migration cannot be reversed as it removes the Organizations table which has been replaced by TenantInfo in TenantStoreDbContext.");
  }
}
