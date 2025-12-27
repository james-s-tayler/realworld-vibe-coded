using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddTenantIdToEntities : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<Guid>(
        name: "TenantId",
        table: "UserFollowing",
        type: "uniqueidentifier",
        nullable: true);

    migrationBuilder.AddColumn<Guid>(
        name: "TenantId",
        table: "Tags",
        type: "uniqueidentifier",
        nullable: true);

    migrationBuilder.AddColumn<Guid>(
        name: "TenantId",
        table: "Organizations",
        type: "uniqueidentifier",
        nullable: true);

    migrationBuilder.AddColumn<Guid>(
        name: "TenantId",
        table: "Comments",
        type: "uniqueidentifier",
        nullable: true);

    migrationBuilder.AddColumn<Guid>(
        name: "TenantId",
        table: "Articles",
        type: "uniqueidentifier",
        nullable: true);

    migrationBuilder.CreateIndex(
        name: "IX_UserFollowing_TenantId",
        table: "UserFollowing",
        column: "TenantId");

    migrationBuilder.CreateIndex(
        name: "IX_Tags_TenantId",
        table: "Tags",
        column: "TenantId");

    migrationBuilder.CreateIndex(
        name: "IX_Organizations_TenantId",
        table: "Organizations",
        column: "TenantId");

    migrationBuilder.CreateIndex(
        name: "IX_Comments_TenantId",
        table: "Comments",
        column: "TenantId");

    migrationBuilder.CreateIndex(
        name: "IX_Articles_TenantId",
        table: "Articles",
        column: "TenantId");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropIndex(
        name: "IX_UserFollowing_TenantId",
        table: "UserFollowing");

    migrationBuilder.DropIndex(
        name: "IX_Tags_TenantId",
        table: "Tags");

    migrationBuilder.DropIndex(
        name: "IX_Organizations_TenantId",
        table: "Organizations");

    migrationBuilder.DropIndex(
        name: "IX_Comments_TenantId",
        table: "Comments");

    migrationBuilder.DropIndex(
        name: "IX_Articles_TenantId",
        table: "Articles");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "UserFollowing");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Tags");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Organizations");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Comments");

    migrationBuilder.DropColumn(
        name: "TenantId",
        table: "Articles");
  }
}
