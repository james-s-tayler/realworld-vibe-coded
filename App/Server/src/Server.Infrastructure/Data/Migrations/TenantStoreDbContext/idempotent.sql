IF OBJECT_ID(N'[__EFMigrationsHistory_TenantStore]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory_TenantStore] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory_TenantStore] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_TenantStore]
    WHERE [MigrationId] = N'20251229125736_InitialTenantStore'
)
BEGIN
    CREATE TABLE [TenantInfo] (
        [Id] nvarchar(450) NOT NULL,
        [Identifier] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NULL,
        CONSTRAINT [PK_TenantInfo] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_TenantStore]
    WHERE [MigrationId] = N'20251229125736_InitialTenantStore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TenantInfo_Identifier] ON [TenantInfo] ([Identifier]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory_TenantStore]
    WHERE [MigrationId] = N'20251229125736_InitialTenantStore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory_TenantStore] ([MigrationId], [ProductVersion])
    VALUES (N'20251229125736_InitialTenantStore', N'10.0.1');
END;

COMMIT;
GO

