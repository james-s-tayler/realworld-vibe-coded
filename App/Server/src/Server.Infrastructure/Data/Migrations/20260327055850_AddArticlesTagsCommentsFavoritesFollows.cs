using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddArticlesTagsCommentsFavoritesFollows : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Article",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Slug = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
          Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
          Body = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: false),
          AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Article", x => x.Id);
          table.ForeignKey(
                    name: "FK_Article_AspNetUsers_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateTable(
        name: "Tag",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
          TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Tag", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "UserFollowing",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          FollowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          FollowedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UserFollowing", x => x.Id);
          table.ForeignKey(
                    name: "FK_UserFollowing_AspNetUsers_FollowedId",
                    column: x => x.FollowedId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
          table.ForeignKey(
                    name: "FK_UserFollowing_AspNetUsers_FollowerId",
                    column: x => x.FollowerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "ArticleFavorite",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ArticleFavorite", x => x.Id);
          table.ForeignKey(
                    name: "FK_ArticleFavorite_Article_ArticleId",
                    column: x => x.ArticleId,
                    principalTable: "Article",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_ArticleFavorite_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "Comment",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Body = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: false),
          ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Comment", x => x.Id);
          table.ForeignKey(
                    name: "FK_Comment_Article_ArticleId",
                    column: x => x.ArticleId,
                    principalTable: "Article",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_Comment_AspNetUsers_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateTable(
        name: "ArticleTag",
        columns: table => new
        {
          ArticlesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ArticleTag", x => new { x.ArticlesId, x.TagsId });
          table.ForeignKey(
                    name: "FK_ArticleTag_Article_ArticlesId",
                    column: x => x.ArticlesId,
                    principalTable: "Article",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_ArticleTag_Tag_TagsId",
                    column: x => x.TagsId,
                    principalTable: "Tag",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Article_AuthorId",
        table: "Article",
        columns: new[] { "AuthorId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_Article_Slug",
        table: "Article",
        columns: new[] { "Slug", "TenantId" },
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_ArticleFavorite_ArticleId",
        table: "ArticleFavorite",
        columns: new[] { "ArticleId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_ArticleFavorite_UserId_ArticleId",
        table: "ArticleFavorite",
        columns: new[] { "UserId", "ArticleId", "TenantId" },
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_ArticleTag_TagsId",
        table: "ArticleTag",
        column: "TagsId");

    migrationBuilder.CreateIndex(
        name: "IX_Comment_ArticleId",
        table: "Comment",
        columns: new[] { "ArticleId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_Comment_AuthorId",
        table: "Comment",
        columns: new[] { "AuthorId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_Tag_Name",
        table: "Tag",
        columns: new[] { "Name", "TenantId" },
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_FollowedId",
        table: "UserFollowing",
        columns: new[] { "FollowedId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_FollowerId_FollowedId",
        table: "UserFollowing",
        columns: new[] { "FollowerId", "FollowedId", "TenantId" },
        unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "ArticleFavorite");

    migrationBuilder.DropTable(
        name: "ArticleTag");

    migrationBuilder.DropTable(
        name: "Comment");

    migrationBuilder.DropTable(
        name: "UserFollowing");

    migrationBuilder.DropTable(
        name: "Tag");

    migrationBuilder.DropTable(
        name: "Article");
  }
}
