using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddLanguageToApplicationUser : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
        name: "Language",
        table: "AspNetUsers",
        type: "nvarchar(10)",
        maxLength: 10,
        nullable: false,
        defaultValue: "en");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "Language",
        table: "AspNetUsers");
  }
}
