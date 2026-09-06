# 인증 우회 구멍 메우기 (2026-09-05)

[ROADMAP.md](ROADMAP.md) 3-1 "인증이 통째로 우회 가능하다"에 대한 수정 기록이다.
목표는 **가장 작은 변경으로 "게임 서버가 AccountServer 가 발급한 토큰을 실제로 검사하게"** 만드는 것.
비밀번호 해싱(3-2)은 이번 범위에 넣지 않았다.

---

## 1. 구멍 — 정확히 무엇이 뚫려 있었나

### 어디

`Server/GameServer/Session/ClientSession_PreGame.cs` 의 `HandleLogin` (`C_Login` 패킷 핸들러).

```csharp
// 수정 전
public void HandleLogin(C_Login loginPacket)
{
    //보안 체크~                                   ← 주석만 있고 검증이 없다
    if (ServerState != PlayerServerState.ServerStateLogin) return;

    AccountDb findAccount = db.Accounts
        .Where(a => a.AccountName == loginPacket.UniqueId)   // 클라가 보낸 문자열을 그대로 계정 이름으로
        .FirstOrDefault();
    ...
    else
    {
        AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };  // 없으면 새로 만든다
```

클라이언트는 `UniqueId` 에 **설치 경로의 해시**를 넣어 보냈다 (`Application.dataPath.GetHashCode()`).

### 클라이언트(공격자)가 할 수 있었던 것

1. AccountServer 를 **거치지 않고** 게임 서버 포트(9999 등)에 TCP 로 직접 붙는다.
2. `C_Login { uniqueId = "아무 문자열" }` 을 보낸다.
3. 그 문자열과 같은 `AccountName` 을 가진 게임 계정이 있으면 **비밀번호 없이 그 계정으로 로비 진입**, 없으면 **계정이 새로 생성**된다.

즉 AccountServer 의 로그인·토큰 발급은 게임 진입에 아무 영향도 없었다.
`SharedDB.Token` 테이블은 쓰기만 되고 읽는 곳이 없었다 (`GameServer` 전체에서 `Token` 미참조).

곁들여 `AccountServer/Controllers/AccountController.cs:98` 의 만료 시각 계산이 잘못돼 있었다.

```csharp
DateTime expired = DateTime.UtcNow;
expired.AddSeconds(600);        // DateTime 은 immutable — 반환값을 버려서 항상 "지금"이 저장됨
```

검사하는 곳이 없어서 드러나지 않았을 뿐, 검사를 넣는 순간 **모든 토큰이 발급 즉시 만료**되는 버그였다.

---

## 2. 수정

### 2-1. 설계 — 정식 경로를 그대로 쓴다

새 인프라를 만들지 않았다. 이미 있던 것들을 잇기만 했다.

```
로그인(HTTPS)          토큰 기록                    토큰 검증 (신규)
클라 ─────────▶ AccountServer ────▶ SharedDB.Token ◀──── GameServer.HandleLogin
      ◀── {AccountDbId, Token}                                ▲
                                                              │ C_Login { accountDbId, token }  (필드 추가)
클라 ─────────────────────────────────────────────────────────┘
```

| 파일 | 변경 |
|---|---|
| `Common/protoc-3.17.3-win64/bin/Protocol.proto` | `C_Login` 에 `int32 accountDbId = 2; int32 token = 3;` 추가 (`uniqueId` 는 호환용으로 남김) |
| `Common/.../Protocol.cs` · `Server/GameServer/Packet/Protocol.cs` · `Server/DummyClient/Packet/Protocol.cs` · `Client/Assets/Scripts/Packet/Protocol.cs` | `protoc 3.17.3` 으로 재생성 (4개 파일 동일, MD5 일치). 메시지 추가는 없어서 `*PacketManager.cs` 는 그대로 |
| `Server/GameServer/Session/ClientSession_PreGame.cs` | **`VerifyToken` 추가 + `HandleLogin` 에서 호출**, 계정 키를 `UniqueId` → `AccountDbId` 로 |
| `Server/AccountServer/Controllers/AccountController.cs` | 만료 시각 버그 수정 (`TokenLifetimeSeconds = 600`) |
| `Client/Assets/Scripts/Packet/PacketHandler.cs` | `C_Login` 에 `Managers.Network.AccountId / Token` 을 실어 보냄, `LoginOk == 0` 이면 중단 |
| `Server/DummyClient/*` | AccountServer 에 더미 계정 생성·로그인 후 토큰으로 접속. `--bogus` 옵션(가짜 토큰) 추가. 왕복 시간 계측 추가 ([LOADTEST.md](LOADTEST.md) 6-3) |

### 2-2. 핵심 코드 — `ClientSession_PreGame.cs`

```csharp
bool VerifyToken(int accountDbId, int token)
{
    if (accountDbId <= 0)
        return false;
    try
    {
        using (SharedDbContext shared = new SharedDbContext())
        {
            TokenDb tokenDb = shared.Tokens.AsNoTracking()
                .Where(t => t.AccountDbId == accountDbId).FirstOrDefault();

            if (tokenDb == null || tokenDb.Token != token) return false;
            if (tokenDb.Expired < DateTime.UtcNow)          return false;
            return true;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"[Login] SharedDB 토큰 조회 실패 : {e.Message}");
        return false;                                   // DB 를 못 읽어도 거부 (fail-closed)
    }
}

public void HandleLogin(C_Login loginPacket)
{
    if (ServerState != PlayerServerState.ServerStateLogin) return;

    if (VerifyToken(loginPacket.AccountDbId, loginPacket.Token) == false)
    {
        Send(new S_Login() { LoginOk = 0 });
        GameLogic.Instance.PushAfter(1000, Disconnect);  // Send 는 예약만 하므로 flush 뒤에 끊는다
        return;
    }

    string accountName = loginPacket.AccountDbId.ToString();   // 게임 DB 계정은 AccountServer 의 ID 로 찾는다
    ...기존 로직 (UniqueId 대신 accountName)...
}
```

두 가지 선택에 이유가 있다.

- **왜 바로 `Disconnect()` 하지 않고 `PushAfter(1000, Disconnect)` 인가** —
  이 서버의 `ClientSession.Send` 는 큐에 넣기만 하고, 실제 송신은 `NetworkTask` 스레드가 최대 100 ms 뒤에 flush 한다.
  즉시 끊으면 `LoginOk = 0` 이 클라이언트에 도달하지 못한다. 기존 `JobTimer` 를 그대로 썼다.
- **왜 DB 예외도 "실패"로 처리하는가 (fail-closed)** —
  `HandleLogin` 은 Recv 스레드에서 실행된다. 예외가 새어 나가면 `Session.OnRecvCompleted` 의 `catch` 가 잡지만
  `RegisterRecv()` 가 다시 걸리지 않아 **세션이 연결된 채로 수신 불능**이 된다 ([LOADTEST.md](LOADTEST.md) 관찰 3 에서 실측).
  거부하고 끊는 편이 안전하다.

### 2-3. 클라이언트 — `PacketHandler.cs`

```csharp
// 수정 전                                                  // 수정 후
loginPacket.UniqueId = path.GetHashCode().ToString();      loginPacket.AccountDbId = Managers.Network.AccountId;
                                                            loginPacket.Token       = Managers.Network.Token;
```

`Managers.Network.AccountId / Token` 은 원래부터 `UI_LoginScene` 이 로그인 응답에서 채워두던 값이다. 쓰는 곳이 없었을 뿐이다.

---

## 3. 검증

### 3-1. 한 것

| 검증 | 방법 | 결과 |
|---|---|---|
| 서버 빌드 | `dotnet build Server/Server.sln -c Debug` | **에러 0** / 경고 7 (전부 기존 경고) |
| Unity 클라이언트 컴파일 | `Unity.exe -batchmode -quit -nographics -projectPath Client` (6000.2.10f1) | `CompileScripts` 성공, **컴파일 에러 0**, 종료 코드 0 |
| 생성 코드 일관성 | 수정 전 proto 로 `protoc` 재생성 → 기존 `Protocol.cs` 와 내용 동일(줄바꿈만 차이) 확인 후 필드 추가 | 4개 복사본 MD5 동일 |
| **가짜 토큰 거부** | 게임 서버 3개 기동 → `DummyClient.exe --bogus` (AccountDbId 900000~, Token 1 로 230 세션) | 아래 |

`--bogus` 실행 결과 (09-05, 이 PC 에 SharedDB 가 없던 상태 — 토큰 조회가 실패 → fail-closed 경로. DB 있는 상태의 결과는 3-2):

```
DummyClient : Connected (230) → "로그인 거부 (LoginOk=0)" 230줄 → Connected (0)   ← 전부 거부되고 끊김
GameServer  : [Login] SharedDB 토큰 조회 실패 : ... error: 52 ...   90 / 100 / 40 건
              [Login] 토큰 검증 실패 AccountDbId=9000xx → 접속 종료   90 / 100 / 40 건
              OnRecvCompleted Failed                                   0 건   ← 수정 전엔 세션당 1건 (좀비 세션)
              Connected (0) Players                                    ← 25초 뒤 Established 연결 0개
```

수정 전 같은 조건에서는 230 세션이 **전부 연결된 채 수신 불능 상태로 남았다** ([LOADTEST.md](LOADTEST.md) 4-2).

### 3-2. DB 있는 상태에서의 확인 (2026-09-06, LocalDB 설치 후 실행)

작성 당일에는 LocalDB 가 없어 빌드만 확인했고, 다음 날 LocalDB 2022 를 설치해 아래 1)~3) 을 실제로 돌렸다. 결과:

| 단계 | 결과 |
|---|---|
| 1) `test` 계정 로그인 → 토큰 | `{"LoginOk":true,"AccountDbId":1,"Token":-1891285517,"ServerList":[3개]}` · SharedDB `Expired` = 발급 시각 **+ 10분** (22:51:47 → 23:01:47 UTC). 수정 전엔 발급 시각과 같았다 |
| 2) DummyClient 정상 모드 | `connected=230 loginOk=230 loginFail=0 inGame=230 disconnected=0`, `loginRtt` p50 109 / p95 328 ms — 230 세션 전부 게임 입장 |
| 3) `--bogus` | `loginFail=230 disconnected=230 connected=0` (10초 안에), 서버 `토큰 검증 실패` 90 / 100 / 40 건, `OnRecvCompleted Failed` 0 건 |
| 추가) 게임 DB 만 OFFLINE + 정상 토큰 | 토큰은 통과, 계정 조회 예외 → 230 세션 전부 **끊김** (좀비 세션 수정 확인, [LOADTEST.md](LOADTEST.md) 4-4) |
| 4) 만료 토큰으로 Unity 접속 · 5) Unity 정상 플레이 | **아직 안 함** — Unity 클라이언트는 컴파일만 확인했다 |

숫자와 절차 전문은 [LOADTEST.md](LOADTEST.md). 아래는 원래 적어둔 확인 순서다 (4·5 는 남아 있다).

```powershell
# 0) DB 3개 + AccountServer + 게임 서버 (README 순서)
# 1) 정상 경로 — 토큰 발급 → 게임 서버 진입
curl -k -X POST https://localhost:5001/api/account/login -H "Content-Type: application/json" `
     -d '{"AccountName":"test","Password":"test"}'
#    → {"LoginOk":true,"AccountDbId":1,"Token":-123456789,...}
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -d SharedDB -Q "SELECT AccountDbId, Token, Expired FROM Token"
#    → Expired 가 "지금 + 10분" 이어야 한다 (수정 전엔 발급 시각과 같았다)

# 2) DummyClient 정상 모드 — AccountServer 로 dummy0001~0230 로그인 후 접속
.\DummyClient.exe
#    기대: [stats] connected=230 loginOk=230 loginFail=0 inGame=230 disconnected=0 loginRtt=... enterRtt=...

# 3) DummyClient 가짜 토큰 — 전부 거부
.\DummyClient.exe --bogus
#    기대: loginFail=230, 약 1초 뒤 connected=0, 서버 로그에 "토큰 검증 실패" 230건

# 4) 만료 — SharedDB 에서 Expired 를 과거로 바꾼 뒤 Unity 클라로 접속
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -d SharedDB -Q "UPDATE Token SET Expired = '2000-01-01' WHERE AccountDbId = 1"
#    기대: Unity 콘솔 "LoginOk_0" + "게임 서버가 로그인을 거부했습니다" → OnDisconnected

# 5) Unity 클라 정상 로그인 → 서버 선택 → 로비 → 게임 입장이 예전과 동일하게 되는지
```

⚠️ **기존 게임 계정은 이어지지 않는다.** 게임 DB 의 계정 키가 "설치 경로 해시" → "AccountDbId 문자열" 로 바뀌었으므로,
예전에 만든 캐릭터는 새 로그인에서 보이지 않는다 (새 계정으로 다시 생성된다). 로컬 개발 DB 라 마이그레이션은 하지 않았다.

---

## 4. 남은 것 (이번에 의도적으로 하지 않은 것)

| 항목 | 상태 | 비고 |
|---|---|---|
| 비밀번호 평문 저장 (ROADMAP 3-2) | 그대로 | `PasswordHasher<T>` 도입은 별도 작업 |
| 토큰 1회용 처리 | 안 함 | 600초 안에는 같은 토큰으로 재접속 가능. 채널 이동 시 재로그인 없이 쓰는 흐름을 깨지 않기 위해 유지 |
| `DungeonLadersDB.Account` 에 FK 컬럼 | 안 함 | 스키마 변경(마이그레이션 + `Sql/*.sql` 재생성)을 피하려고 기존 `AccountName` 에 ID 문자열을 넣었다 |
| `C_Login.uniqueId` 필드 | 남김 | 서버는 더 이상 읽지 않는다. 다음 proto 정리 때 제거 |
| 로그인 DB 조회를 Recv 스레드에서 수행 | 그대로 | 로그인 폭주 시 스레드풀 팽창 ([LOADTEST.md](LOADTEST.md) 관찰 4). `DbTransaction` 큐로 옮기는 것이 다음 단계 |
| 거부 시 클라 UI | 로그만 | `S_LoginHandler` 에서 경고 로그 후 중단. 로그인 씬 자동 복귀는 넣지 않았다 |

---

## 5. 면접 설명용 요약

1. 계정 서버가 토큰을 발급하는데 **게임 서버가 그 토큰을 한 번도 읽지 않는** 것을 코드 전체 검색으로 확인했다 — 게임 서버 포트에 직접 붙어 아무 문자열이나 보내면 비밀번호 없이 들어가고, 없는 계정은 자동 생성됐다.
2. 고치는 김에 만료 시각 계산 버그(`expired.AddSeconds(600)` 반환값 폐기)도 찾았다 — 검사하는 곳이 없어서 5년간 아무도 몰랐고, 검사를 넣는 순간 모든 토큰이 즉시 만료되는 버그였다.
3. 새 인프라 없이 **이미 있던 `SharedDB.Token` 테이블**을 게임 서버가 읽게만 했다 — `C_Login` 에 `accountDbId`·`token` 두 필드를 추가하고 `HandleLogin` 첫 줄에서 대조한다.
4. 거부할 때 바로 끊지 않고 **`JobTimer` 로 1초 뒤에 끊는다** — 이 서버의 `Send` 는 100 ms 주기 배치 송신이라 즉시 끊으면 거부 응답이 안 나가기 때문이다. 기존 스레드 모델을 이해해야 나오는 선택이다.
5. DB 예외는 **fail-closed** 로 처리했다 — 실측에서 Recv 스레드의 예외가 세션을 "연결된 채 수신 불능"으로 만드는 걸 봤기 때문이다. 검증은 `DummyClient --bogus` 로 230 세션이 전부 `LoginOk=0` 을 받고 끊기는 것으로 확인했다.
