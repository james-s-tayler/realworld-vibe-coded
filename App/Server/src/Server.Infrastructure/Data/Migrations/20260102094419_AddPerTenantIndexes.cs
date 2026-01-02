using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddPerTenantIndexes : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropIndex(
        name: "IX_UserFollowing_FollowedId",
        table: "UserFollowing");

    migrationBuilder.DropIndex(
        name: "IX_Tags_Name",
        table: "Tags");

    migrationBuilder.DropIndex(
        name: "IX_Comments_ArticleId",
        table: "Comments");

    migrationBuilder.DropIndex(
        name: "IX_Comments_AuthorId",
        table: "Comments");

    migrationBuilder.DropIndex(
        name: "IX_Articles_AuthorId",
        table: "Articles");

    migrationBuilder.AddColumn<string>(
        name: "TenantId",
        table: "UserFollowing",
        type: "nvarchar(450)",
        nullable: false,
        defaultValue: string.Empty);

    migrationBuilder.AddColumn<string>(
        name: "TenantId",
        table: "Tags",
        type: "nvarchar(450)",
        nullable: false,
        defaultValue: string.Empty);

    migrationBuilder.AddColumn<string>(
        name: "TenantId",
        table: "Comments",
        type: "nvarchar(450)",
        nullable: false,
        defaultValue: string.Empty);

    migrationBuilder.AddColumn<string>(
        name: "TenantId",
        table: "Articles",
        type: "nvarchar(450)",
        nullable: false,
        defaultValue: string.Empty);

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_FollowedId",
        table: "UserFollowing",
        columns: new[] { "FollowedId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_Tags_Name",
        table: "Tags",
        columns: new[] { "Name", "TenantId" },
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Comments_ArticleId",
        table: "Comments",
        columns: new[] { "ArticleId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_Comments_AuthorId",
        table: "Comments",
        columns: new[] { "AuthorId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_Articles_AuthorId",
        table: "Articles",
        columns: new[] { "AuthorId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_Articles_Slug",
        table: "Articles",
        columns: new[] { "Slug", "TenantId" },
        unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropIndex(
        name: "IX_UserFollowing_FollowedId",
        table: "UserFollowing");

    migrationBuilder.DropIndex(
        name: "IX_Tags_Name",
        table: "Tags");

    migrationBuilder.DropIndex(
        name: "IX_Comments_ArticleId",
        table: "Comments");

    migrationBuilder.DropIndex(
        name: "IX_Comments_AuthorId",
        table: "Comments");

    migrationBuilder.DropIndex(
        name: "IX_Articles_AuthorId",
        table: "Articles");

    migrationBuilder.DropIndex(
        name: "IX_Articles_Slug",
        table: "Articles");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Tags");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Comments");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Articles");

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_FollowedId",
        table: "UserFollowing",
        column: "FollowedId");

    migrationBuilder.CreateIndex(
        name: "IX_Tags_Name",
        table: "Tags",
        column: "Name",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Comments_ArticleId",
        table: "Comments",
        column: "ArticleId");

    migrationBuilder.CreateIndex(
        name: "IX_Comments_AuthorId",
        table: "Comments",
        column: "AuthorId");

    migrationBuilder.CreateIndex(
        name: "IX_Articles_AuthorId",
        table: "Articles",
        column: "AuthorId");
  }
}
