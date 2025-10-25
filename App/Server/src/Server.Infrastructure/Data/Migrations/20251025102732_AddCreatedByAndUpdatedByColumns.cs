using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddCreatedByAndUpdatedByColumns : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
        name: "CreatedBy",
        table: "Users",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "UpdatedBy",
        table: "Users",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "CreatedBy",
        table: "UserFollowing",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "UpdatedBy",
        table: "UserFollowing",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "CreatedBy",
        table: "Tags",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "UpdatedBy",
        table: "Tags",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "CreatedBy",
        table: "Comments",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "UpdatedBy",
        table: "Comments",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "CreatedBy",
        table: "Articles",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "UpdatedBy",
        table: "Articles",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "CreatedBy",
        table: "Users");

    migrationBuilder.DropColumn(
        name: "UpdatedBy",
        table: "Users");

    migrationBuilder.DropColumn(
        name: "CreatedBy",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "UpdatedBy",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "CreatedBy",
        table: "Tags");

    migrationBuilder.DropColumn(
        name: "UpdatedBy",
        table: "Tags");

    migrationBuilder.DropColumn(
        name: "CreatedBy",
        table: "Comments");

    migrationBuilder.DropColumn(
        name: "UpdatedBy",
        table: "Comments");

    migrationBuilder.DropColumn(
        name: "CreatedBy",
        table: "Articles");

    migrationBuilder.DropColumn(
        name: "UpdatedBy",
        table: "Articles");
  }
}
