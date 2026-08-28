-- DungeonLaders — 데이터베이스 생성
--
-- 01~03 스크립트는 "이미 존재하는 DB 안에" 테이블을 만든다.
-- 이 스크립트를 먼저 실행해 빈 DB 3개를 만들어야 한다.
--
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i 00_CreateDatabases.sql
--
-- 이미 있으면 아무것도 하지 않는다 (재실행 안전).

IF DB_ID(N'DungeonLadersDB') IS NULL
BEGIN
    CREATE DATABASE [DungeonLadersDB];
    PRINT 'created: DungeonLadersDB';
END
ELSE
    PRINT 'already exists: DungeonLadersDB';
GO

IF DB_ID(N'SharedDB') IS NULL
BEGIN
    CREATE DATABASE [SharedDB];
    PRINT 'created: SharedDB';
END
ELSE
    PRINT 'already exists: SharedDB';
GO

IF DB_ID(N'AccountDB') IS NULL
BEGIN
    CREATE DATABASE [AccountDB];
    PRINT 'created: AccountDB';
END
ELSE
    PRINT 'already exists: AccountDB';
GO
