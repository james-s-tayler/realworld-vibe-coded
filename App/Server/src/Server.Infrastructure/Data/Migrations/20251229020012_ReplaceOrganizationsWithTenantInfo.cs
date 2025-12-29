using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class ReplaceOrganizationsWithTenantInfo : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // Drop the foreign key constraint - this is the one causing the error
    migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_Organizations_TenantId')
            BEGIN
                ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Organizations_TenantId];
            END
        ");

    migrationBuilder.DropTable(
        name: "Organizations");

    migrationBuilder.AlterColumn<string>(
        name: "TenantId",
        table: "AspNetUsers",
        type: "nvarchar(450)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(50)",
        oldNullable: true);

    migrationBuilder.CreateTable(
        name: "TenantInfo",
        columns: table => new
        {
          Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
          Identifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
          Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TenantInfo", x => x.Id);
        });
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "TenantInfo");

    migrationBuilder.AlterColumn<string>(
        name: "TenantId",
        table: "AspNetUsers",
        type: "nvarchar(50)",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(450)",
        oldNullable: true);

    migrationBuilder.CreateTable(
        name: "Organizations",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
          ChangeCheck = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
          Identifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
          Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
          TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
          UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
          UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Organizations", x => x.Id);
          table.UniqueConstraint("AK_Organizations_Identifier", x => x.Identifier);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Organizations_Identifier",
        table: "Organizations",
        column: "Identifier",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_Organizations_TenantId",
        table: "Organizations",
        column: "TenantId");

    migrationBuilder.AddForeignKey(
        name: "FK_AspNetUsers_Organizations_TenantId",
        table: "AspNetUsers",
        column: "TenantId",
        principalTable: "Organizations",
        principalColumn: "Identifier",
        onDelete: ReferentialAction.Restrict);
  }
}
