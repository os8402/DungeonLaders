# DB 구축

DungeonLaders 는 **데이터베이스 3개**를 쓴다.

| DB | 내용 | 소유 프로젝트 |
|---|---|---|
| `DungeonLadersDB` | 계정 · 플레이어 · 아이템 (게임 데이터) | `GameServer` |
| `SharedDB` | 서버 목록 · 혼잡도 · 로그인 토큰 | `SharedDB` |
| `AccountDB` | 계정 인증 | `AccountServer` |

스키마의 원본은 **EF Core 마이그레이션**(`*/Migrations/`) 이다.
이 폴더의 `.sql` 은 거기서 생성한 것이므로, 스키마를 바꿀 때는 마이그레이션을 고치고 스크립트를 다시 뽑아야 한다.

---

## 방법 A — SQL 스크립트 (EF 도구 없이)

`.NET SDK` 나 `dotnet-ef` 없이 SQL 클라이언트만 있으면 된다.

```bash
cd Server/Sql

sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -i 00_CreateDatabases.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d DungeonLadersDB -i 01_DungeonLadersDB.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d SharedDB        -i 02_SharedDB.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d AccountDB       -i 03_AccountDB.sql
```

### ⚠️ `-I` 를 빼면 안 된다

`sqlcmd` 는 기본적으로 **`QUOTED_IDENTIFIER OFF`** 로 접속한다.
그런데 EF Core 가 만드는 유니크 인덱스는 `QUOTED_IDENTIFIER ON` 을 요구한다.
`-I` 없이 실행하면 이렇게 실패한다.

```
Msg 1934: CREATE INDEX failed because the following SET options have
          incorrect settings: 'QUOTED_IDENTIFIER'.
```

**더 나쁜 건, 실패해도 sqlcmd 가 종료 코드 0 을 돌려준다는 점이다.**
테이블이 절반만 만들어진 채로 "성공"처럼 보인다.
`-b` 를 함께 주면 오류 시 종료 코드가 0 이 아니게 되므로, 스크립트에서 돌릴 때는 `-I -b` 를 같이 쓰는 게 안전하다.

`01`~`03` 은 **멱등(idempotent)** 스크립트라 여러 번 실행해도 안전하고, 이미 적용된 마이그레이션은 건너뛴다.
다만 위처럼 반쯤 적용되어 깨진 상태라면 멱등성이 도움이 되지 않으므로, DB 를 지우고 다시 만드는 게 빠르다.

```sql
ALTER DATABASE [DungeonLadersDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [DungeonLadersDB];
```

> SSMS / Azure Data Studio 는 `QUOTED_IDENTIFIER ON` 이 기본이라 그냥 열어서 실행하면 된다.
> 단 `01`~`03` 은 **대상 DB 를 선택한 상태**에서 실행해야 한다.

## 방법 B — EF Core 도구

```bash
dotnet tool install -g dotnet-ef        # 최초 1회

cd Server/GameServer    && dotnet ef database update
cd Server/SharedDB      && dotnet ef database update
cd Server/AccountServer && dotnet ef database update --context AccountServer.DB.AppDbContext
```

⚠️ **`AccountServer` 는 `--context` 가 필수다.** 이 프로젝트는 `SharedDB` 를 참조하고 있어
`AccountServer.DB.AppDbContext` 와 `SharedDB.SharedDbContext` 둘 다 발견되고,
지정하지 않으면 다음 오류가 난다.

```
More than one DbContext was found. Specify which one to use.
```

---

## LocalDB 가 없다면 — Docker

macOS · Linux 이거나 LocalDB 를 쓰고 싶지 않다면 SQL Server 컨테이너를 띄운다.

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=DungeonL@ders1" \
  -p 1433:1433 --name dungeonladers-db -d \
  mcr.microsoft.com/mssql/server:2022-latest
```

그 다음 접속 문자열을 바꾼다.

| 파일 | 대상 |
|---|---|
| `Server/Configs/*/config.json` | `connectionString` (게임 서버 4채널) |
| `Server/AccountServer/appsettings.json` | `DefaultConnection` · `SharedConnection` |
| `Server/SharedDB/SharedDbContext.cs` | `ConnectionString` 기본값 (하드코딩) |

```
Server=localhost,1433;Database=DungeonLadersDB;User Id=sa;Password=DungeonL@ders1;Encrypt=False;TrustServerCertificate=True;
```

---

## ⚠️ `Encrypt` 는 반드시 명시할 것

`Microsoft.Data.SqlClient` 4.0 부터 `Encrypt` **기본값이 `true`** 로 바뀌었다 (EF Core 8 이 이 버전대를 쓴다).
접속 문자열에 `Encrypt` 가 없으면 LocalDB / 자체서명 인증서 환경에서 **인증서 오류로 접속이 전부 실패**한다.

서버는 정상적으로 뜨고 `Listening...` 까지 찍히는데 DB만 안 붙으므로 원인을 찾기 어렵다.
로컬 개발용 접속 문자열에는 항상 다음을 넣는다.

```
Encrypt=False;TrustServerCertificate=True;
```

자세한 내용은 [`docs/MIGRATION.md`](../../docs/MIGRATION.md) 3-3 절 참고.

---

## 스키마를 바꿨을 때 — 스크립트 재생성

```bash
cd Server/GameServer && dotnet ef migrations script --idempotent -o ../Sql/01_DungeonLadersDB.sql
cd Server/SharedDB   && dotnet ef migrations script --idempotent -o ../Sql/02_SharedDB.sql
cd Server/AccountServer && dotnet ef migrations script --idempotent \
    --context AccountServer.DB.AppDbContext -o ../Sql/03_AccountDB.sql
```

`migrations script` 는 **DB 에 접속하지 않으므로** 서버가 꺼져 있어도 생성된다.
