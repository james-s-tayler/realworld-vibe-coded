using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddAuthorEntity : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
        name: "FK_Articles_AspNetUsers_AuthorId",
        table: "Articles");

    migrationBuilder.DropForeignKey(
        name: "FK_Comments_AspNetUsers_AuthorId",
        table: "Comments");

    migrationBuilder.CreateTable(
        name: "Authors",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
          Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
          Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
          TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Authors", x => x.Id);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Authors_Username",
        table: "Authors",
        columns: new[] { "Username", "TenantId" },
        unique: true);

    // Populate Authors table from AspNetUsers for existing users who have created articles or comments
    migrationBuilder.Sql(@"
      INSERT INTO Authors (Id, Username, Bio, Image, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
      SELECT 
        u.Id,
        u.UserName,
        ISNULL(u.Bio, ''),
        u.Image,
        u.TenantId,
        GETUTCDATE(),
        GETUTCDATE(),
        'SYSTEM_MIGRATION',
        'SYSTEM_MIGRATION'
      FROM AspNetUsers u
      WHERE EXISTS (
        SELECT 1 FROM Articles a WHERE a.AuthorId = u.Id
        UNION
        SELECT 1 FROM Comments c WHERE c.AuthorId = u.Id
      )
    ");

    migrationBuilder.AddForeignKey(
        name: "FK_Articles_Authors_AuthorId",
        table: "Articles",
        column: "AuthorId",
        principalTable: "Authors",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_Comments_Authors_AuthorId",
        table: "Comments",
        column: "AuthorId",
        principalTable: "Authors",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
        name: "FK_Articles_Authors_AuthorId",
        table: "Articles");

    migrationBuilder.DropForeignKey(
        name: "FK_Comments_Authors_AuthorId",
        table: "Comments");

    migrationBuilder.DropTable(
        name: "Authors");

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
  }
}
