using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddChangeCheckToEntityBase : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<byte[]>(
        name: "ChangeCheck",
        table: "Users",
        type: "rowversion",
        rowVersion: true,
        nullable: false,
        defaultValue: new byte[0]);

    migrationBuilder.AddColumn<byte[]>(
        name: "ChangeCheck",
        table: "UserFollowing",
        type: "rowversion",
        rowVersion: true,
        nullable: false,
        defaultValue: new byte[0]);

    migrationBuilder.AddColumn<byte[]>(
        name: "ChangeCheck",
        table: "Tags",
        type: "rowversion",
        rowVersion: true,
        nullable: false,
        defaultValue: new byte[0]);

    migrationBuilder.AddColumn<byte[]>(
        name: "ChangeCheck",
        table: "Comments",
        type: "rowversion",
        rowVersion: true,
        nullable: false,
        defaultValue: new byte[0]);

    migrationBuilder.AddColumn<byte[]>(
        name: "ChangeCheck",
        table: "Articles",
        type: "rowversion",
        rowVersion: true,
        nullable: false,
        defaultValue: new byte[0]);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "ChangeCheck",
        table: "Users");

    migrationBuilder.DropColumn(
        name: "ChangeCheck",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "ChangeCheck",
        table: "Tags");

    migrationBuilder.DropColumn(
        name: "ChangeCheck",
        table: "Comments");

    migrationBuilder.DropColumn(
        name: "ChangeCheck",
        table: "Articles");
  }
}
