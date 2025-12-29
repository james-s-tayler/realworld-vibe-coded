using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Contributors",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
          Status = table.Column<int>(type: "int", nullable: false),
          PhoneNumber_CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
          PhoneNumber_Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
          PhoneNumber_Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Contributors", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Tags",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Tags", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
          HashedPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
          Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Articles",
        columns: table => new
        {
          Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
          Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
          Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
          Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          AuthorId = table.Column<int>(type: "int", nullable: false),
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
          FollowerId = table.Column<int>(type: "int", nullable: false),
          FollowedId = table.Column<int>(type: "int", nullable: false),
          Id = table.Column<int>(type: "int", nullable: false),
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
          ArticleId = table.Column<int>(type: "int", nullable: false),
          FavoritedById = table.Column<int>(type: "int", nullable: false),
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
          ArticlesId = table.Column<int>(type: "int", nullable: false),
          TagsId = table.Column<int>(type: "int", nullable: false),
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
          Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
          Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          AuthorId = table.Column<int>(type: "int", nullable: false),
          ArticleId = table.Column<int>(type: "int", nullable: false),
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
        name: "Contributors");

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
