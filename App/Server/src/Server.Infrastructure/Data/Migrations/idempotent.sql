IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [Contributors] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Status] int NOT NULL,
        [PhoneNumber_CountryCode] nvarchar(max) NULL,
        [PhoneNumber_Number] nvarchar(max) NULL,
        [PhoneNumber_Extension] nvarchar(max) NULL,
        CONSTRAINT [PK_Contributors] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [Tags] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(255) NOT NULL,
        [Username] nvarchar(100) NOT NULL,
        [HashedPassword] nvarchar(255) NOT NULL,
        [Bio] nvarchar(1000) NOT NULL,
        [Image] nvarchar(500) NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [Articles] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [Slug] nvarchar(250) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [AuthorId] int NOT NULL,
        CONSTRAINT [PK_Articles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Articles_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [UserFollowing] (
        [FollowerId] int NOT NULL,
        [FollowedId] int NOT NULL,
        [Id] int NOT NULL,
        CONSTRAINT [PK_UserFollowing] PRIMARY KEY ([FollowerId], [FollowedId]),
        CONSTRAINT [FK_UserFollowing_Users_FollowedId] FOREIGN KEY ([FollowedId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserFollowing_Users_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [ArticleFavorites] (
        [ArticleId] int NOT NULL,
        [FavoritedById] int NOT NULL,
        CONSTRAINT [PK_ArticleFavorites] PRIMARY KEY ([ArticleId], [FavoritedById]),
        CONSTRAINT [FK_ArticleFavorites_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleFavorites_Users_FavoritedById] FOREIGN KEY ([FavoritedById]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [ArticleTags] (
        [ArticlesId] int NOT NULL,
        [TagsId] int NOT NULL,
        CONSTRAINT [PK_ArticleTags] PRIMARY KEY ([ArticlesId], [TagsId]),
        CONSTRAINT [FK_ArticleTags_Articles_ArticlesId] FOREIGN KEY ([ArticlesId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleTags_Tags_TagsId] FOREIGN KEY ([TagsId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE TABLE [Comments] (
        [Id] int NOT NULL IDENTITY,
        [Body] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [AuthorId] int NOT NULL,
        [ArticleId] int NOT NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comments_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ArticleFavorites_FavoritedById] ON [ArticleFavorites] ([FavoritedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Articles_AuthorId] ON [Articles] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Articles_Slug] ON [Articles] ([Slug]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ArticleTags_TagsId] ON [ArticleTags] ([TagsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_ArticleId] ON [Comments] ([ArticleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_AuthorId] ON [Comments] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tags_Name] ON [Tags] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserFollowing_FollowedId] ON [UserFollowing] ([FollowedId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006155541_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251006155541_InitialCreate', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014115046_RemoveContributorsTable'
)
BEGIN
    DROP TABLE [Contributors];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014115046_RemoveContributorsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251014115046_RemoveContributorsTable', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024025739_AddAuditColumnsToEntityBase'
)
BEGIN
    ALTER TABLE [Users] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024025739_AddAuditColumnsToEntityBase'
)
BEGIN
    ALTER TABLE [Users] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024025739_AddAuditColumnsToEntityBase'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024025739_AddAuditColumnsToEntityBase'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024025739_AddAuditColumnsToEntityBase'
)
BEGIN
    ALTER TABLE [Tags] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024025739_AddAuditColumnsToEntityBase'
)
BEGIN
    ALTER TABLE [Tags] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024025739_AddAuditColumnsToEntityBase'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251024025739_AddAuditColumnsToEntityBase', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [CreatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [UpdatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD [CreatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD [UpdatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Tags] ADD [CreatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Tags] ADD [UpdatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Comments] ADD [CreatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Comments] ADD [UpdatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Articles] ADD [CreatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    ALTER TABLE [Articles] ADD [UpdatedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025102732_AddCreatedByAndUpdatedByColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251025102732_AddCreatedByAndUpdatedByColumns', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN

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
            
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'UpdatedBy');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var + ';');
    EXEC(N'UPDATE [Users] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Users] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Users] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'CreatedBy');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var1 + ';');
    EXEC(N'UPDATE [Users] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [Users] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Users] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFollowing]') AND [c].[name] = N'UpdatedBy');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [UserFollowing] DROP CONSTRAINT ' + @var2 + ';');
    EXEC(N'UPDATE [UserFollowing] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [UserFollowing] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [UserFollowing] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFollowing]') AND [c].[name] = N'CreatedBy');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [UserFollowing] DROP CONSTRAINT ' + @var3 + ';');
    EXEC(N'UPDATE [UserFollowing] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [UserFollowing] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [UserFollowing] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tags]') AND [c].[name] = N'UpdatedBy');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Tags] DROP CONSTRAINT ' + @var4 + ';');
    EXEC(N'UPDATE [Tags] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Tags] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Tags] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tags]') AND [c].[name] = N'CreatedBy');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Tags] DROP CONSTRAINT ' + @var5 + ';');
    EXEC(N'UPDATE [Tags] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [Tags] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Tags] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Comments]') AND [c].[name] = N'UpdatedBy');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Comments] DROP CONSTRAINT ' + @var6 + ';');
    EXEC(N'UPDATE [Comments] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Comments] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Comments] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Comments]') AND [c].[name] = N'CreatedBy');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Comments] DROP CONSTRAINT ' + @var7 + ';');
    EXEC(N'UPDATE [Comments] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [Comments] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Comments] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Articles]') AND [c].[name] = N'UpdatedBy');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Articles] DROP CONSTRAINT ' + @var8 + ';');
    EXEC(N'UPDATE [Articles] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Articles] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Articles] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Articles]') AND [c].[name] = N'CreatedBy');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Articles] DROP CONSTRAINT ' + @var9 + ';');
    EXEC(N'UPDATE [Articles] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [Articles] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Articles] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251025144627_MakeCreatedByAndUpdatedByRequired', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [Articles] DROP CONSTRAINT [FK_Articles_Users_AuthorId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [ArticleFavorites] DROP CONSTRAINT [FK_ArticleFavorites_Articles_ArticleId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [ArticleFavorites] DROP CONSTRAINT [FK_ArticleFavorites_Users_FavoritedById];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [ArticleTags] DROP CONSTRAINT [FK_ArticleTags_Articles_ArticlesId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [ArticleTags] DROP CONSTRAINT [FK_ArticleTags_Tags_TagsId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [Comments] DROP CONSTRAINT [FK_Comments_Articles_ArticleId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [Comments] DROP CONSTRAINT [FK_Comments_Users_AuthorId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [UserFollowing] DROP CONSTRAINT [FK_UserFollowing_Users_FollowedId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    ALTER TABLE [UserFollowing] DROP CONSTRAINT [FK_UserFollowing_Users_FollowerId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    DROP TABLE [ArticleFavorites];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    DROP TABLE [ArticleTags];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    DROP TABLE [Comments];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    DROP TABLE [UserFollowing];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    DROP TABLE [Articles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    DROP TABLE [Tags];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    DROP TABLE [Users];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [Username] nvarchar(100) NOT NULL,
        [HashedPassword] nvarchar(255) NOT NULL,
        [Bio] nvarchar(1000) NOT NULL,
        [Image] nvarchar(500) NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE TABLE [Tags] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tags_Name] ON [Tags] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE TABLE [Articles] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [Slug] nvarchar(250) NOT NULL,
        [AuthorId] uniqueidentifier NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Articles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Articles_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE INDEX [IX_Articles_AuthorId] ON [Articles] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Articles_Slug] ON [Articles] ([Slug]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE TABLE [Comments] (
        [Id] uniqueidentifier NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [AuthorId] uniqueidentifier NOT NULL,
        [ArticleId] uniqueidentifier NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comments_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE INDEX [IX_Comments_ArticleId] ON [Comments] ([ArticleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE INDEX [IX_Comments_AuthorId] ON [Comments] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE TABLE [UserFollowing] (
        [FollowerId] uniqueidentifier NOT NULL,
        [FollowedId] uniqueidentifier NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_UserFollowing] PRIMARY KEY ([FollowerId], [FollowedId]),
        CONSTRAINT [FK_UserFollowing_Users_FollowedId] FOREIGN KEY ([FollowedId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserFollowing_Users_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE INDEX [IX_UserFollowing_FollowedId] ON [UserFollowing] ([FollowedId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE TABLE [ArticleFavorites] (
        [ArticleId] uniqueidentifier NOT NULL,
        [FavoritedById] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ArticleFavorites] PRIMARY KEY ([ArticleId], [FavoritedById]),
        CONSTRAINT [FK_ArticleFavorites_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleFavorites_Users_FavoritedById] FOREIGN KEY ([FavoritedById]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE INDEX [IX_ArticleFavorites_FavoritedById] ON [ArticleFavorites] ([FavoritedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE TABLE [ArticleTags] (
        [ArticlesId] uniqueidentifier NOT NULL,
        [TagsId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ArticleTags] PRIMARY KEY ([ArticlesId], [TagsId]),
        CONSTRAINT [FK_ArticleTags_Articles_ArticlesId] FOREIGN KEY ([ArticlesId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleTags_Tags_TagsId] FOREIGN KEY ([TagsId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    CREATE INDEX [IX_ArticleTags_TagsId] ON [ArticleTags] ([TagsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025184325_ChangeEntityIdsToGuid'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251025184325_ChangeEntityIdsToGuid', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD [ApplicationUserId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD [ApplicationUserId1] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [Bio] nvarchar(1000) NOT NULL,
        [Image] nvarchar(500) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_UserFollowing_ApplicationUserId] ON [UserFollowing] ([ApplicationUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_UserFollowing_ApplicationUserId1] ON [UserFollowing] ([ApplicationUserId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD CONSTRAINT [FK_UserFollowing_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD CONSTRAINT [FK_UserFollowing_AspNetUsers_ApplicationUserId1] FOREIGN KEY ([ApplicationUserId1]) REFERENCES [AspNetUsers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211185450_AddIdentityTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251211185450_AddIdentityTables', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [ArticleFavorites] DROP CONSTRAINT [FK_ArticleFavorites_Users_FavoritedById];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [Articles] DROP CONSTRAINT [FK_Articles_Users_AuthorId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [Comments] DROP CONSTRAINT [FK_Comments_Users_AuthorId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [UserFollowing] DROP CONSTRAINT [FK_UserFollowing_AspNetUsers_ApplicationUserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [UserFollowing] DROP CONSTRAINT [FK_UserFollowing_AspNetUsers_ApplicationUserId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [UserFollowing] DROP CONSTRAINT [FK_UserFollowing_Users_FollowedId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [UserFollowing] DROP CONSTRAINT [FK_UserFollowing_Users_FollowerId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    DROP TABLE [Users];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    DROP INDEX [IX_UserFollowing_ApplicationUserId] ON [UserFollowing];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    DROP INDEX [IX_UserFollowing_ApplicationUserId1] ON [UserFollowing];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFollowing]') AND [c].[name] = N'ApplicationUserId');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [UserFollowing] DROP CONSTRAINT ' + @var10 + ';');
    ALTER TABLE [UserFollowing] DROP COLUMN [ApplicationUserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFollowing]') AND [c].[name] = N'ApplicationUserId1');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [UserFollowing] DROP CONSTRAINT ' + @var11 + ';');
    ALTER TABLE [UserFollowing] DROP COLUMN [ApplicationUserId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [ArticleFavorites] ADD CONSTRAINT [FK_ArticleFavorites_AspNetUsers_FavoritedById] FOREIGN KEY ([FavoritedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [Articles] ADD CONSTRAINT [FK_Articles_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [Comments] ADD CONSTRAINT [FK_Comments_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD CONSTRAINT [FK_UserFollowing_AspNetUsers_FollowedId] FOREIGN KEY ([FollowedId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD CONSTRAINT [FK_UserFollowing_AspNetUsers_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251214103130_MigrateToApplicationUserOnly'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251214103130_MigrateToApplicationUserOnly', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    DROP INDEX [UserNameIndex] ON [AspNetUsers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    DROP INDEX [RoleNameIndex] ON [AspNetRoles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetUserTokens] ADD [TenantId] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [TenantId] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] ADD [TenantId] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetUserLogins] ADD [TenantId] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetUserClaims] ADD [TenantId] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetRoles] ADD [TenantId] nvarchar(450) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetRoleClaims] ADD [TenantId] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    CREATE TABLE [Organizations] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Identifier] nvarchar(50) NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Organizations] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Organizations_Identifier] UNIQUE ([Identifier])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    INSERT INTO Organizations (Id, Name, Identifier, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) 
                      VALUES ('00000000-0000-0000-0000-000000000001', 'Default', '', '2025-01-01', '2025-01-01', 'System', 'System')
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_TenantId] ON [AspNetUsers] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName], [TenantId]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName], [TenantId]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Organizations_Identifier] ON [Organizations] ([Identifier]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Organizations_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Organizations] ([Identifier]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227061254_AddOrganizationAndNullableTenantId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251227061254_AddOrganizationAndNullableTenantId', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    ALTER TABLE [UserFollowing] ADD [TenantId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    ALTER TABLE [Tags] ADD [TenantId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    ALTER TABLE [Organizations] ADD [TenantId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    ALTER TABLE [Comments] ADD [TenantId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    ALTER TABLE [Articles] ADD [TenantId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    CREATE INDEX [IX_UserFollowing_TenantId] ON [UserFollowing] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    CREATE INDEX [IX_Tags_TenantId] ON [Tags] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    CREATE INDEX [IX_Organizations_TenantId] ON [Organizations] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    CREATE INDEX [IX_Comments_TenantId] ON [Comments] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    CREATE INDEX [IX_Articles_TenantId] ON [Articles] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227084843_AddTenantIdToEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251227084843_AddTenantIdToEntities', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229020012_ReplaceOrganizationsWithTenantInfo'
)
BEGIN

                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_Organizations_TenantId')
                BEGIN
                    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Organizations_TenantId];
                END
            
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229020012_ReplaceOrganizationsWithTenantInfo'
)
BEGIN
    DROP TABLE [Organizations];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229020012_ReplaceOrganizationsWithTenantInfo'
)
BEGIN
    DROP INDEX [IX_AspNetUsers_TenantId] ON [AspNetUsers];
    DROP INDEX [UserNameIndex] ON [AspNetUsers];
    DECLARE @var12 nvarchar(max);
    SELECT @var12 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'TenantId');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT ' + @var12 + ';');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [TenantId] nvarchar(450) NULL;
    CREATE INDEX [IX_AspNetUsers_TenantId] ON [AspNetUsers] ([TenantId]);
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName], [TenantId]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229020012_ReplaceOrganizationsWithTenantInfo'
)
BEGIN
    CREATE TABLE [TenantInfo] (
        [Id] nvarchar(450) NOT NULL,
        [Identifier] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NULL,
        CONSTRAINT [PK_TenantInfo] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229020012_ReplaceOrganizationsWithTenantInfo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251229020012_ReplaceOrganizationsWithTenantInfo', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229080618_RemoveTenantInfoFromAppDbContext'
)
BEGIN

                IF EXISTS (SELECT * FROM sys.objects WHERE name = 'TenantInfo' AND type = 'U')
                BEGIN
                    DROP TABLE [TenantInfo];
                END
            
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229080618_RemoveTenantInfoFromAppDbContext'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251229080618_RemoveTenantInfoFromAppDbContext', N'10.0.1');
END;

COMMIT;
GO

