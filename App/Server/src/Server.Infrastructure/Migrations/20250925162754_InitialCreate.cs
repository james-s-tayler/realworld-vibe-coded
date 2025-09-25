using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Tags",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
          Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
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

    migrationBuilder.CreateTable(
        name: "Articles",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
          Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
          Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
          Body = table.Column<string>(type: "TEXT", nullable: false),
          Slug = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
          AuthorId = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Articles", x => x.Id);
          table.ForeignKey(
                    name: "FK_Articles_Users_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateTable(
        name: "UserFollowing",
        columns: table => new
        {
          FollowerId = table.Column<int>(type: "INTEGER", nullable: false),
          FollowedId = table.Column<int>(type: "INTEGER", nullable: false),
          Id = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UserFollowing", x => new { x.FollowerId, x.FollowedId });
          table.ForeignKey(
                    name: "FK_UserFollowing_Users_FollowedId",
                    column: x => x.FollowedId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
          table.ForeignKey(
                    name: "FK_UserFollowing_Users_FollowerId",
                    column: x => x.FollowerId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateTable(
        name: "ArticleFavorites",
        columns: table => new
        {
          ArticleId = table.Column<int>(type: "INTEGER", nullable: false),
          FavoritedById = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ArticleFavorites", x => new { x.ArticleId, x.FavoritedById });
          table.ForeignKey(
                    name: "FK_ArticleFavorites_Articles_ArticleId",
                    column: x => x.ArticleId,
                    principalTable: "Articles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_ArticleFavorites_Users_FavoritedById",
                    column: x => x.FavoritedById,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "ArticleTags",
        columns: table => new
        {
          ArticlesId = table.Column<int>(type: "INTEGER", nullable: false),
          TagsId = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ArticleTags", x => new { x.ArticlesId, x.TagsId });
          table.ForeignKey(
                    name: "FK_ArticleTags_Articles_ArticlesId",
                    column: x => x.ArticlesId,
                    principalTable: "Articles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_ArticleTags_Tags_TagsId",
                    column: x => x.TagsId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "Comments",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
          Body = table.Column<string>(type: "TEXT", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
          AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
          ArticleId = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Comments", x => x.Id);
          table.ForeignKey(
                    name: "FK_Comments_Articles_ArticleId",
                    column: x => x.ArticleId,
                    principalTable: "Articles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_Comments_Users_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateIndex(
        name: "IX_ArticleFavorites_FavoritedById",
        table: "ArticleFavorites",
        column: "FavoritedById");

    migrationBuilder.CreateIndex(
        name: "IX_Articles_AuthorId",
        table: "Articles",
        column: "AuthorId");

    migrationBuilder.CreateIndex(
        name: "IX_Articles_Slug",
        table: "Articles",
        column: "Slug",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_ArticleTags_TagsId",
        table: "ArticleTags",
        column: "TagsId");

    migrationBuilder.CreateIndex(
        name: "IX_Comments_ArticleId",
        table: "Comments",
        column: "ArticleId");

    migrationBuilder.CreateIndex(
        name: "IX_Comments_AuthorId",
        table: "Comments",
        column: "AuthorId");

    migrationBuilder.CreateIndex(
        name: "IX_Tags_Name",
        table: "Tags",
        column: "Name",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_FollowedId",
        table: "UserFollowing",
        column: "FollowedId");

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
        name: "ArticleFavorites");

    migrationBuilder.DropTable(
        name: "ArticleTags");

    migrationBuilder.DropTable(
        name: "Comments");

    migrationBuilder.DropTable(
        name: "UserFollowing");

    migrationBuilder.DropTable(
        name: "Tags");

    migrationBuilder.DropTable(
        name: "Articles");

    migrationBuilder.DropTable(
        name: "Users");
  }
}
