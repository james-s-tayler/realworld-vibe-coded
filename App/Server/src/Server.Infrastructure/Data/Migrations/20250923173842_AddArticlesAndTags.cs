using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddArticlesAndTags : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Articles",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
          Slug = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
          Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
          Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
          Body = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
          AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
          FavoritesCount = table.Column<int>(type: "INTEGER", nullable: false),
          TagList = table.Column<string>(type: "TEXT", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Articles", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Tags",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
          Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Tags", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
          Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
          Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
          HashedPassword = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
          Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
          Image = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Articles_AuthorId",
        table: "Articles",
        column: "AuthorId");

    migrationBuilder.CreateIndex(
        name: "IX_Articles_CreatedAt",
        table: "Articles",
        column: "CreatedAt");

    migrationBuilder.CreateIndex(
        name: "IX_Articles_Slug",
        table: "Articles",
        column: "Slug",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Tags_Name",
        table: "Tags",
        column: "Name",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Users_Email",
        table: "Users",
        column: "Email",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Users_Username",
        table: "Users",
        column: "Username",
        unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Articles");

    migrationBuilder.DropTable(
        name: "Tags");

    migrationBuilder.DropTable(
        name: "Users");
  }
}
