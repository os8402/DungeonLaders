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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210830150419_Init'
)
BEGIN
    CREATE TABLE [ServerInfo] (
        [ServerDbId] int NOT NULL IDENTITY,
        [Name] nvarchar(450) NULL,
        [IpAddress] nvarchar(max) NULL,
        [Port] int NOT NULL,
        [BusyScore] int NOT NULL,
        CONSTRAINT [PK_ServerInfo] PRIMARY KEY ([ServerDbId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210830150419_Init'
)
BEGIN
    CREATE TABLE [Token] (
        [TokenDbId] int NOT NULL IDENTITY,
        [AccountDbId] int NOT NULL,
        [Token] int NOT NULL,
        [Created] datetime2 NOT NULL,
        CONSTRAINT [PK_Token] PRIMARY KEY ([TokenDbId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210830150419_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ServerInfo_Name] ON [ServerInfo] ([Name]) WHERE [Name] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210830150419_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Token_AccountDbId] ON [Token] ([AccountDbId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210830150419_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210830150419_Init', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210830151123_Expired'
)
BEGIN
    EXEC sp_rename N'[Token].[Created]', N'Expired', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210830151123_Expired'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210830151123_Expired', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827164516_ServerHeartbeat'
)
BEGIN
    ALTER TABLE [ServerInfo] ADD [LastPingTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827164516_ServerHeartbeat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260827164516_ServerHeartbeat', N'8.0.10');
END;
GO

COMMIT;
GO

