using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class MoveFollowingAndFavoritesToAuthor : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
        name: "FK_ArticleFavorites_AspNetUsers_FavoritedById",
        table: "ArticleFavorites");

    migrationBuilder.CreateTable(
        name: "AuthorFollowing",
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
          table.PrimaryKey("PK_AuthorFollowing", x => x.Id);
          table.ForeignKey(
                    name: "FK_AuthorFollowing_Authors_FollowedId",
                    column: x => x.FollowedId,
                    principalTable: "Authors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
          table.ForeignKey(
                    name: "FK_AuthorFollowing_Authors_FollowerId",
                    column: x => x.FollowerId,
                    principalTable: "Authors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateIndex(
        name: "IX_AuthorFollowing_FollowedId",
        table: "AuthorFollowing",
        columns: new[] { "FollowedId", "TenantId" });

    migrationBuilder.CreateIndex(
        name: "IX_AuthorFollowing_FollowerId_FollowedId",
        table: "AuthorFollowing",
        columns: new[] { "FollowerId", "FollowedId", "TenantId" },
        unique: true);

    migrationBuilder.AddForeignKey(
        name: "FK_ArticleFavorites_Authors_FavoritedById",
        table: "ArticleFavorites",
        column: "FavoritedById",
        principalTable: "Authors",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
        name: "FK_ArticleFavorites_Authors_FavoritedById",
        table: "ArticleFavorites");

    migrationBuilder.DropTable(
        name: "AuthorFollowing");

    migrationBuilder.AddForeignKey(
        name: "FK_ArticleFavorites_AspNetUsers_FavoritedById",
        table: "ArticleFavorites",
        column: "FavoritedById",
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
  }
}
