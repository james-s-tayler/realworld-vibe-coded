using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddAuditColumnsToEntityBase : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<DateTime>(
        name: "CreatedAt",
        table: "Users",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

    migrationBuilder.AddColumn<DateTime>(
        name: "UpdatedAt",
        table: "Users",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

    migrationBuilder.AddColumn<DateTime>(
        name: "CreatedAt",
        table: "UserFollowing",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

    migrationBuilder.AddColumn<DateTime>(
        name: "UpdatedAt",
        table: "UserFollowing",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

    migrationBuilder.AddColumn<DateTime>(
        name: "CreatedAt",
        table: "Tags",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

    migrationBuilder.AddColumn<DateTime>(
        name: "UpdatedAt",
        table: "Tags",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "CreatedAt",
        table: "Users");

    migrationBuilder.DropColumn(
        name: "UpdatedAt",
        table: "Users");

    migrationBuilder.DropColumn(
        name: "CreatedAt",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "UpdatedAt",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "CreatedAt",
        table: "Tags");

    migrationBuilder.DropColumn(
        name: "UpdatedAt",
        table: "Tags");
  }
}
