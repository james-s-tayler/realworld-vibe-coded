using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations.AppDbContext;

/// <inheritdoc />
public partial class MigrateToApplicationUserOnly : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
        name: "FK_ArticleFavorites_Users_FavoritedById",
        table: "ArticleFavorites");

    migrationBuilder.DropForeignKey(
        name: "FK_Articles_Users_AuthorId",
        table: "Articles");

    migrationBuilder.DropForeignKey(
        name: "FK_Comments_Users_AuthorId",
        table: "Comments");

    migrationBuilder.DropForeignKey(
        name: "FK_UserFollowing_AspNetUsers_ApplicationUserId",
        table: "UserFollowing");

    migrationBuilder.DropForeignKey(
        name: "FK_UserFollowing_AspNetUsers_ApplicationUserId1",
        table: "UserFollowing");

    migrationBuilder.DropForeignKey(
        name: "FK_UserFollowing_Users_FollowedId",
        table: "UserFollowing");

    migrationBuilder.DropForeignKey(
        name: "FK_UserFollowing_Users_FollowerId",
        table: "UserFollowing");

    migrationBuilder.DropTable(
        name: "Users");

    migrationBuilder.DropIndex(
        name: "IX_UserFollowing_ApplicationUserId",
        table: "UserFollowing");

    migrationBuilder.DropIndex(
        name: "IX_UserFollowing_ApplicationUserId1",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "ApplicationUserId",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "ApplicationUserId1",
        table: "UserFollowing");

    migrationBuilder.AddForeignKey(
        name: "FK_ArticleFavorites_AspNetUsers_FavoritedById",
        table: "ArticleFavorites",
        column: "FavoritedById",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);

    migrationBuilder.AddForeignKey(
        name: "FK_Articles_AspNetUsers_AuthorId",
        table: "Articles",
        column: "AuthorId",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_Comments_AspNetUsers_AuthorId",
        table: "Comments",
        column: "AuthorId",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_UserFollowing_AspNetUsers_FollowedId",
        table: "UserFollowing",
        column: "FollowedId",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_UserFollowing_AspNetUsers_FollowerId",
        table: "UserFollowing",
        column: "FollowerId",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
        name: "FK_ArticleFavorites_AspNetUsers_FavoritedById",
        table: "ArticleFavorites");

    migrationBuilder.DropForeignKey(
        name: "FK_Articles_AspNetUsers_AuthorId",
        table: "Articles");

    migrationBuilder.DropForeignKey(
        name: "FK_Comments_AspNetUsers_AuthorId",
        table: "Comments");

    migrationBuilder.DropForeignKey(
        name: "FK_UserFollowing_AspNetUsers_FollowedId",
        table: "UserFollowing");

    migrationBuilder.DropForeignKey(
        name: "FK_UserFollowing_AspNetUsers_FollowerId",
        table: "UserFollowing");

    migrationBuilder.AddColumn<Guid>(
        name: "ApplicationUserId",
        table: "UserFollowing",
        type: "uniqueidentifier",
        nullable: true);

    migrationBuilder.AddColumn<Guid>(
        name: "ApplicationUserId1",
        table: "UserFollowing",
        type: "uniqueidentifier",
        nullable: true);

    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          HashedPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_ApplicationUserId",
        table: "UserFollowing",
        column: "ApplicationUserId");

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_ApplicationUserId1",
        table: "UserFollowing",
        column: "ApplicationUserId1");

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

    migrationBuilder.AddForeignKey(
        name: "FK_ArticleFavorites_Users_FavoritedById",
        table: "ArticleFavorites",
        column: "FavoritedById",
        principalTable: "Users",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);

    migrationBuilder.AddForeignKey(
        name: "FK_Articles_Users_AuthorId",
        table: "Articles",
        column: "AuthorId",
        principalTable: "Users",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_Comments_Users_AuthorId",
        table: "Comments",
        column: "AuthorId",
        principalTable: "Users",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_UserFollowing_AspNetUsers_ApplicationUserId",
        table: "UserFollowing",
        column: "ApplicationUserId",
        principalTable: "AspNetUsers",
        principalColumn: "Id");

    migrationBuilder.AddForeignKey(
        name: "FK_UserFollowing_AspNetUsers_ApplicationUserId1",
        table: "UserFollowing",
        column: "ApplicationUserId1",
        principalTable: "AspNetUsers",
        principalColumn: "Id");

    migrationBuilder.AddForeignKey(
        name: "FK_UserFollowing_Users_FollowedId",
        table: "UserFollowing",
        column: "FollowedId",
        principalTable: "Users",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_UserFollowing_Users_FollowerId",
        table: "UserFollowing",
        column: "FollowerId",
        principalTable: "Users",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);
  }
}
