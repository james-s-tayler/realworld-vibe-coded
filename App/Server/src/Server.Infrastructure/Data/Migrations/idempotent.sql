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
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [TenantId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [Bio] nvarchar(1000) NOT NULL,
        [Image] nvarchar(500) NULL,
        [TenantId] nvarchar(450) NOT NULL,
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
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        [TenantId] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        [TenantId] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TenantId] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [TenantId] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        [TenantId] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName], [TenantId]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName], [TenantId]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260322043003_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260322043003_InitialCreate', N'10.0.1');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE TABLE [Article] (
        [Id] uniqueidentifier NOT NULL,
        [Slug] nvarchar(300) NOT NULL,
        [Title] nvarchar(255) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [AuthorId] uniqueidentifier NOT NULL,
        [TenantId] nvarchar(450) NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Article] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Article_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE TABLE [Tag] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [TenantId] nvarchar(450) NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Tag] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE TABLE [UserFollowing] (
        [Id] uniqueidentifier NOT NULL,
        [FollowerId] uniqueidentifier NOT NULL,
        [FollowedId] uniqueidentifier NOT NULL,
        [TenantId] nvarchar(450) NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_UserFollowing] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserFollowing_AspNetUsers_FollowedId] FOREIGN KEY ([FollowedId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserFollowing_AspNetUsers_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE TABLE [ArticleFavorite] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ArticleId] uniqueidentifier NOT NULL,
        [TenantId] nvarchar(450) NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_ArticleFavorite] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ArticleFavorite_Article_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Article] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleFavorite_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE TABLE [Comment] (
        [Id] uniqueidentifier NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [ArticleId] uniqueidentifier NOT NULL,
        [AuthorId] uniqueidentifier NOT NULL,
        [TenantId] nvarchar(450) NOT NULL,
        [ChangeCheck] rowversion NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Comment] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comment_Article_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Article] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comment_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE TABLE [ArticleTag] (
        [ArticlesId] uniqueidentifier NOT NULL,
        [TagsId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ArticleTag] PRIMARY KEY ([ArticlesId], [TagsId]),
        CONSTRAINT [FK_ArticleTag_Article_ArticlesId] FOREIGN KEY ([ArticlesId]) REFERENCES [Article] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleTag_Tag_TagsId] FOREIGN KEY ([TagsId]) REFERENCES [Tag] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE INDEX [IX_Article_AuthorId] ON [Article] ([AuthorId], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Article_Slug] ON [Article] ([Slug], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE INDEX [IX_ArticleFavorite_ArticleId] ON [ArticleFavorite] ([ArticleId], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ArticleFavorite_UserId_ArticleId] ON [ArticleFavorite] ([UserId], [ArticleId], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE INDEX [IX_ArticleTag_TagsId] ON [ArticleTag] ([TagsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE INDEX [IX_Comment_ArticleId] ON [Comment] ([ArticleId], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE INDEX [IX_Comment_AuthorId] ON [Comment] ([AuthorId], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tag_Name] ON [Tag] ([Name], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE INDEX [IX_UserFollowing_FollowedId] ON [UserFollowing] ([FollowedId], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserFollowing_FollowerId_FollowedId] ON [UserFollowing] ([FollowerId], [FollowedId], [TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260327055850_AddArticlesTagsCommentsFavoritesFollows'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260327055850_AddArticlesTagsCommentsFavoritesFollows', N'10.0.1');
END;

COMMIT;
GO

