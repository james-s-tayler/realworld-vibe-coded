using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class ChangeEntityIdsToGuid : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // WARNING: This migration will delete all existing data
    // Drop all foreign key constraints first
    migrationBuilder.DropForeignKey(name: "FK_Articles_Users_AuthorId", table: "Articles");
    migrationBuilder.DropForeignKey(name: "FK_ArticleFavorites_Articles_ArticleId", table: "ArticleFavorites");
    migrationBuilder.DropForeignKey(name: "FK_ArticleFavorites_Users_FavoritedById", table: "ArticleFavorites");
    migrationBuilder.DropForeignKey(name: "FK_ArticleTags_Articles_ArticlesId", table: "ArticleTags");
    migrationBuilder.DropForeignKey(name: "FK_ArticleTags_Tags_TagsId", table: "ArticleTags");
    migrationBuilder.DropForeignKey(name: "FK_Comments_Articles_ArticleId", table: "Comments");
    migrationBuilder.DropForeignKey(name: "FK_Comments_Users_AuthorId", table: "Comments");
    migrationBuilder.DropForeignKey(name: "FK_UserFollowing_Users_FollowedId", table: "UserFollowing");
    migrationBuilder.DropForeignKey(name: "FK_UserFollowing_Users_FollowerId", table: "UserFollowing");

    // Drop all tables
    migrationBuilder.DropTable(name: "ArticleFavorites");
    migrationBuilder.DropTable(name: "ArticleTags");
    migrationBuilder.DropTable(name: "Comments");
    migrationBuilder.DropTable(name: "UserFollowing");
    migrationBuilder.DropTable(name: "Articles");
    migrationBuilder.DropTable(name: "Tags");
    migrationBuilder.DropTable(name: "Users");

    // Recreate Users table with Guid ID
    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
          HashedPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
          Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });

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

    // Recreate Tags table with Guid ID
    migrationBuilder.CreateTable(
        name: "Tags",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Tags", x => x.Id);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Tags_Name",
        table: "Tags",
        column: "Name",
        unique: true);

    // Recreate Articles table with Guid ID
    migrationBuilder.CreateTable(
        name: "Articles",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
          Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
          Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
          Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
          AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
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

    migrationBuilder.CreateIndex(
        name: "IX_Articles_AuthorId",
        table: "Articles",
        column: "AuthorId");

    migrationBuilder.CreateIndex(
        name: "IX_Articles_Slug",
        table: "Articles",
        column: "Slug",
        unique: true);

    // Recreate Comments table with Guid ID
    migrationBuilder.CreateTable(
        name: "Comments",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
          AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
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
        name: "IX_Comments_ArticleId",
        table: "Comments",
        column: "ArticleId");

    migrationBuilder.CreateIndex(
        name: "IX_Comments_AuthorId",
        table: "Comments",
        column: "AuthorId");

    // Recreate UserFollowing table with Guid foreign keys
    migrationBuilder.CreateTable(
        name: "UserFollowing",
        columns: table => new
        {
          FollowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          FollowedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
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

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_FollowedId",
        table: "UserFollowing",
        column: "FollowedId");

    // Recreate ArticleFavorites junction table
    migrationBuilder.CreateTable(
        name: "ArticleFavorites",
        columns: table => new
        {
          ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          FavoritedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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

    migrationBuilder.CreateIndex(
        name: "IX_ArticleFavorites_FavoritedById",
        table: "ArticleFavorites",
        column: "FavoritedById");

    // Recreate ArticleTags junction table
    migrationBuilder.CreateTable(
        name: "ArticleTags",
        columns: table => new
        {
          ArticlesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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

    migrationBuilder.CreateIndex(
        name: "IX_ArticleTags_TagsId",
        table: "ArticleTags",
        column: "TagsId");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    // WARNING: Down migration will also delete all data
    // Drop all tables
    migrationBuilder.DropTable(name: "ArticleFavorites");
    migrationBuilder.DropTable(name: "ArticleTags");
    migrationBuilder.DropTable(name: "Comments");
    migrationBuilder.DropTable(name: "UserFollowing");
    migrationBuilder.DropTable(name: "Articles");
    migrationBuilder.DropTable(name: "Tags");
    migrationBuilder.DropTable(name: "Users");

    // Recreate the original schema with int IDs
    // (This would require replicating the exact original schema from previous migrations)
    throw new NotSupportedException("Down migration not supported for this breaking change. " +
        "If you need to rollback, restore from a backup or apply previous migrations to a new database.");
  }
}
