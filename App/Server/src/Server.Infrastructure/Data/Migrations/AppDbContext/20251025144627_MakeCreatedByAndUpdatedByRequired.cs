using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Infrastructure.Data.Migrations.AppDbContext;

/// <inheritdoc />
public partial class MakeCreatedByAndUpdatedByRequired : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    // Update existing null values to "SYSTEM" before making columns non-nullable
    migrationBuilder.Sql(@"
            UPDATE Users SET CreatedBy = 'SYSTEM' WHERE CreatedBy IS NULL;
            UPDATE Users SET UpdatedBy = 'SYSTEM' WHERE UpdatedBy IS NULL;
            UPDATE UserFollowing SET CreatedBy = 'SYSTEM' WHERE CreatedBy IS NULL;
            UPDATE UserFollowing SET UpdatedBy = 'SYSTEM' WHERE UpdatedBy IS NULL;
            UPDATE Tags SET CreatedBy = 'SYSTEM' WHERE CreatedBy IS NULL;
            UPDATE Tags SET UpdatedBy = 'SYSTEM' WHERE UpdatedBy IS NULL;
            UPDATE Comments SET CreatedBy = 'SYSTEM' WHERE CreatedBy IS NULL;
            UPDATE Comments SET UpdatedBy = 'SYSTEM' WHERE UpdatedBy IS NULL;
            UPDATE Articles SET CreatedBy = 'SYSTEM' WHERE CreatedBy IS NULL;
            UPDATE Articles SET UpdatedBy = 'SYSTEM' WHERE UpdatedBy IS NULL;
        ");

    // Make columns non-nullable
    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Users",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Users",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "UserFollowing",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "UserFollowing",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Tags",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Tags",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Comments",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Comments",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Articles",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Articles",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: false,
        defaultValue: string.Empty,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Users",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Users",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "UserFollowing",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "UserFollowing",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Tags",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Tags",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Comments",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Comments",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "UpdatedBy",
        table: "Articles",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);

    migrationBuilder.AlterColumn<string>(
        name: "CreatedBy",
        table: "Articles",
        type: "nvarchar(256)",
        maxLength: 256,
        nullable: true,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256);
  }
}
