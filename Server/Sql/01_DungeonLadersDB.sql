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
    WHERE [MigrationId] = N'20210816074653_Init'
)
BEGIN
    CREATE TABLE [Account] (
        [AccountDbId] int NOT NULL IDENTITY,
        [AccountName] nvarchar(450) NULL,
        CONSTRAINT [PK_Account] PRIMARY KEY ([AccountDbId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816074653_Init'
)
BEGIN
    CREATE TABLE [Player] (
        [PlayerDbId] int NOT NULL IDENTITY,
        [PlayerName] nvarchar(450) NULL,
        [AccountDbId] int NULL,
        CONSTRAINT [PK_Player] PRIMARY KEY ([PlayerDbId]),
        CONSTRAINT [FK_Player_Account_AccountDbId] FOREIGN KEY ([AccountDbId]) REFERENCES [Account] ([AccountDbId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816074653_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Account_AccountName] ON [Account] ([AccountName]) WHERE [AccountName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816074653_Init'
)
BEGIN
    CREATE INDEX [IX_Player_AccountDbId] ON [Player] ([AccountDbId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816074653_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Player_PlayerName] ON [Player] ([PlayerName]) WHERE [PlayerName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816074653_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210816074653_Init', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] DROP CONSTRAINT [FK_Player_Account_AccountDbId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    DROP INDEX [IX_Player_AccountDbId] ON [Player];
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Player]') AND [c].[name] = N'AccountDbId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Player] DROP CONSTRAINT [' + @var0 + '];');
    EXEC(N'UPDATE [Player] SET [AccountDbId] = 0 WHERE [AccountDbId] IS NULL');
    ALTER TABLE [Player] ALTER COLUMN [AccountDbId] int NOT NULL;
    ALTER TABLE [Player] ADD DEFAULT 0 FOR [AccountDbId];
    CREATE INDEX [IX_Player_AccountDbId] ON [Player] ([AccountDbId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] ADD [Attack] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] ADD [Hp] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] ADD [Level] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] ADD [MaxHp] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] ADD [Speed] real NOT NULL DEFAULT CAST(0 AS real);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] ADD [TotalExp] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    ALTER TABLE [Player] ADD CONSTRAINT [FK_Player_Account_AccountDbId] FOREIGN KEY ([AccountDbId]) REFERENCES [Account] ([AccountDbId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816083156_PlayerStat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210816083156_PlayerStat', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816104957_Item'
)
BEGIN
    CREATE TABLE [Item] (
        [ItemDbId] int NOT NULL IDENTITY,
        [TemplateId] int NOT NULL,
        [Count] int NOT NULL,
        [Slot] int NOT NULL,
        [OwnerDbId] int NULL,
        CONSTRAINT [PK_Item] PRIMARY KEY ([ItemDbId]),
        CONSTRAINT [FK_Item_Player_OwnerDbId] FOREIGN KEY ([OwnerDbId]) REFERENCES [Player] ([PlayerDbId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816104957_Item'
)
BEGIN
    CREATE INDEX [IX_Item_OwnerDbId] ON [Item] ([OwnerDbId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210816104957_Item'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210816104957_Item', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210817115108_Equipped'
)
BEGIN
    ALTER TABLE [Item] ADD [Equipped] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210817115108_Equipped'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210817115108_Equipped', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210821103005_Mp'
)
BEGIN
    ALTER TABLE [Player] ADD [CurExp] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210821103005_Mp'
)
BEGIN
    ALTER TABLE [Player] ADD [MaxMp] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210821103005_Mp'
)
BEGIN
    ALTER TABLE [Player] ADD [Mp] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210821103005_Mp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210821103005_Mp', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210821103018_MaxMp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210821103018_MaxMp', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210821103045_CurExp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210821103045_CurExp', N'8.0.10');
END;
GO

COMMIT;
GO

