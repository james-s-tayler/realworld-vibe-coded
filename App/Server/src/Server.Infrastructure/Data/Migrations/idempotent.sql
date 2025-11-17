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
    VALUES (N'20251006155541_InitialCreate', N'9.0.6');
END;

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
    VALUES (N'20251014115046_RemoveContributorsTable', N'9.0.6');
END;

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
    VALUES (N'20251024025739_AddAuditColumnsToEntityBase', N'9.0.6');
END;

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
    VALUES (N'20251025102732_AddCreatedByAndUpdatedByColumns', N'9.0.6');
END;

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
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'UpdatedBy');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var + '];');
    EXEC(N'UPDATE [Users] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Users] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Users] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'CreatedBy');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
    EXEC(N'UPDATE [Users] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [Users] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Users] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFollowing]') AND [c].[name] = N'UpdatedBy');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [UserFollowing] DROP CONSTRAINT [' + @var2 + '];');
    EXEC(N'UPDATE [UserFollowing] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [UserFollowing] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [UserFollowing] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFollowing]') AND [c].[name] = N'CreatedBy');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [UserFollowing] DROP CONSTRAINT [' + @var3 + '];');
    EXEC(N'UPDATE [UserFollowing] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [UserFollowing] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [UserFollowing] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tags]') AND [c].[name] = N'UpdatedBy');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Tags] DROP CONSTRAINT [' + @var4 + '];');
    EXEC(N'UPDATE [Tags] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Tags] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Tags] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tags]') AND [c].[name] = N'CreatedBy');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Tags] DROP CONSTRAINT [' + @var5 + '];');
    EXEC(N'UPDATE [Tags] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [Tags] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Tags] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Comments]') AND [c].[name] = N'UpdatedBy');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Comments] DROP CONSTRAINT [' + @var6 + '];');
    EXEC(N'UPDATE [Comments] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Comments] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Comments] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Comments]') AND [c].[name] = N'CreatedBy');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Comments] DROP CONSTRAINT [' + @var7 + '];');
    EXEC(N'UPDATE [Comments] SET [CreatedBy] = N'''' WHERE [CreatedBy] IS NULL');
    ALTER TABLE [Comments] ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Comments] ADD DEFAULT N'' FOR [CreatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Articles]') AND [c].[name] = N'UpdatedBy');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Articles] DROP CONSTRAINT [' + @var8 + '];');
    EXEC(N'UPDATE [Articles] SET [UpdatedBy] = N'''' WHERE [UpdatedBy] IS NULL');
    ALTER TABLE [Articles] ALTER COLUMN [UpdatedBy] nvarchar(256) NOT NULL;
    ALTER TABLE [Articles] ADD DEFAULT N'' FOR [UpdatedBy];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025144627_MakeCreatedByAndUpdatedByRequired'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Articles]') AND [c].[name] = N'CreatedBy');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Articles] DROP CONSTRAINT [' + @var9 + '];');
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
    VALUES (N'20251025144627_MakeCreatedByAndUpdatedByRequired', N'9.0.6');
END;

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
    VALUES (N'20251025184325_ChangeEntityIdsToGuid', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117155657_AddTestPropertyToUser'
)
BEGIN
    ALTER TABLE [Users] ADD [Test] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117155657_AddTestPropertyToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251117155657_AddTestPropertyToUser', N'9.0.6');
END;

COMMIT;
GO

