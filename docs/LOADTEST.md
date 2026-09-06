# 부하 테스트 기록 (2026-09-05 ~ 06)

[ROADMAP.md](ROADMAP.md) 3-4 "부하 도구는 있는데 측정한 숫자가 없다"에 대한 실측 기록이다.

두 번에 걸쳐 돌렸다.

| 회차 | 날짜 | DB | 잰 것 |
|---|---|---|---|
| 1차 | 09-05 | **없음** (LocalDB 미설치) | 게임 서버 유휴 CPU · 메모리, 230 세션 **TCP 접속만** — 로그인은 전부 DB 예외 |
| 2차 | 09-06 | LocalDB 설치 후 3개 DB 생성 | **230 세션이 로그인 → 캐릭터 생성 → 게임 입장까지 들어간 상태**의 CPU · 메모리 · 왕복 시간, 가짜 토큰 거부, 좀비 세션 재현·수정 확인 |

아래 숫자는 전부 실제 실행에서 나온 것이다. 재지 않은 항목은 "측정 못 함"으로 적었고 추정치로 채우지 않았다.
1차에서 "측정 못 함"이었던 항목은 2차 숫자로 바꿨고, 1차에서만 볼 수 있었던 관찰(DB 없는 상태의 동작)은 남겼다.

---

## 1. 환경

| 항목 | 값 |
|---|---|
| CPU | AMD Ryzen 5 5600X (6코어 / 12스레드) |
| RAM | 32 GB (31.9 GB 인식) |
| OS | Windows 10 Pro 10.0.19045 |
| .NET | SDK 9.0.308 / 런타임 Microsoft.NETCore.App 8.0.22 (서버는 `net8.0` 타깃) |
| **DB (2차)** | **SQL Server 2022 Express LocalDB 16.0.1000.6** (`SqlLocalDB.msi` 로 이번에 설치) + go-sqlcmd v1.10.0 (`winget install Microsoft.Sqlcmd`) |
| 빌드 | `dotnet build Server/Server.sln -c Debug` — Debug 빌드 (Release 아님). 2차 게임 서버에는 5-3 의 좀비 세션 수정이 들어가 있다 |
| 서버 ↔ 클라이언트 | 같은 PC, 루프백이 아닌 실제 LAN 주소(`192.168.75.16`)로 접속. AccountServer 는 `https://localhost:5001` |
| 부하 도구 | `DummyClient` — 코드에 박힌 대로 `6666:90 / 7777:100 / 8888:40` = **230 세션**. 접속 후 아무 입력도 보내지 않는다 |

### 측정 도구

- 프로세스 CPU / 메모리: PowerShell `Get-Process` 를 2초 간격으로 샘플링, `TotalProcessorTime` 차분으로 CPU% 계산
  (`CPU% = ΔTotalProcessorTime / Δ시간 / 논리 프로세서 12개 × 100`, 즉 **PC 전체 대비** 비율). 스크립트는 3절.
- 접속 수: `Get-NetTCPConnection -State Established` 를 포트별로 10초마다 집계 + DummyClient / GameServer 콘솔의 `Connected (N) Players`
- 왕복 시간: DummyClient 에 넣은 계측 (6-3절). `[stats]` 한 줄을 10초마다 출력 — avg / p50 / p95 / max (nearest-rank)

### 2차에서 DB 를 넣으며 부딪힌 것

- `winget` 에는 LocalDB 패키지가 없다 (`Microsoft.SQLServer.2022.LocalDB` 같은 ID 는 존재하지 않음). SQL Server 2022 Express 미디어의 `SqlLocalDB.msi` 를 직접 받아 `msiexec /i SqlLocalDB.msi IACCEPTSQLLOCALDBLICENSETERMS=YES /qn` 으로 넣었다 (관리자 승인 1회).
- `winget install Microsoft.Sqlcmd` 로 들어오는 것은 **go-sqlcmd** 인데, `-S "(localdb)\MSSQLLocalDB"` 를 풀지 못한다
  (`no named pipe instance matching 'MSSQLLOCALDB' returned from host '(localdb)'`).
  `sqllocaldb info MSSQLLocalDB` 가 보여주는 파이프 이름(`np:\\.\pipe\LOCALDB#xxxx\tsql\query`)을 `-S` 에 그대로 주면 된다. README 의 4줄은 ODBC sqlcmd 기준이다.
- `-I -b` 를 붙여 4개 스크립트 전부 종료 코드 0, 테이블 수 SharedDB 3 / DungeonLadersDB 4 / AccountDB 2 확인.

---

## 2. 1차 — DB 없이 돌렸을 때 (09-05)

이 PC 에 LocalDB · sqlcmd · Docker · WSL 이 전부 없었고 관리자 권한도 없어 그날은 DB 를 만들지 못했다.
DB 가 없으면 서버는 이렇게 동작한다 (전부 실제로 관찰):

- `GameServer.exe` 는 정상 기동해 `Listening...` 까지 간다. `SharedDB` 갱신은 10초마다 실패 로그를 남긴다
  (`[ServerInfo] SharedDB 갱신 실패 : ... error: 52 - Unable to locate a Local Database Runtime installation`).
- 클라이언트가 `C_Login` 을 보내면 `HandleLogin` 의 `AppDbContext` 조회에서 `SqlException` 이 난다.
  → 세션마다 `OnRecvCompleted Failed` 1건씩 (90 / 100 / 40건), **로그인 성공 0**, 그리고 **230 세션이 전부 연결된 채 수신 불능**으로 남았다 (관찰 3).

1차 숫자 (게임 서버 3개, 수정 전 DummyClient, 70초):

| 프로세스 | CPU 평균 | CPU 최대 | Private 평균 | Private 최대 | WS 최대 | 스레드 최대 |
|---|---|---|---|---|---|---|
| GameServer (Perion, 90 세션) | 24.8 % | 26.0 % | 58.8 MB | 90.9 MB | 153.6 MB | 39 |
| GameServer (Henesis, 100 세션) | 24.9 % | 26.4 % | 53.1 MB | 99.1 MB | 162.6 MB | 39 |
| GameServer (Kunning, 40 세션) | 24.9 % | 27.1 % | 61.6 MB | 90.1 MB | 153.8 MB | 38 |

접속 자체는 230/230 성공, `OnConnectCompleted Fail` 0건이었다. 접속만 하고 로그인이 예외로 끝난 상태의 세션당 메모리는 피크 기준 약 0.6 MB, GC 후 약 0.26 MB.

---

## 3. 2차 — 실행한 명령 (09-06)

```powershell
# 0) DB 3개 (README 순서, -S 는 sqllocaldb info 의 파이프 이름)
sqlcmd -S $pipe -E -I -b -i 00_CreateDatabases.sql   # 이하 01/02/03 도 -d 붙여 동일

# 1) 서버 빌드 (에러 0)
dotnet build Server/Server.sln -c Debug

# 2) AccountServer + DummyClient 가 붙는 3개 채널 기동 (포트 6666 / 7777 / 8888)
#    작업 디렉터리를 Build 폴더로 잡아야 ../config.json 과 맵 데이터를 찾는다
Server\AccountServer\bin\Debug\net8.0\AccountServer.exe          # ASPNETCORE_URLS=https://localhost:5001
foreach ($inst in "Perion","Henesis","Kunning") {
  Start-Process "Server\GameServer\bin\Debug\$inst\Build\GameServer.exe" -WorkingDirectory "Server\GameServer\bin\Debug\$inst\Build"
}

# 3) 유휴 20초 샘플링 → DummyClient 기동 → 120초 샘플링 (sample.ps1 은 한 번에 140초)
.\sample.ps1 -Seconds 140 -Out normal.csv      # 백그라운드
Start-Sleep 20
Server\DummyClient\bin\Debug\net8.0\DummyClient.exe          # dummy0001~0230 을 AccountServer 에 만들고 로그인 → 토큰으로 접속

# 4) 가짜 토큰 (전부 거부돼야 정상)
Server\DummyClient\bin\Debug\net8.0\DummyClient.exe --bogus

# 5) 좀비 세션 재현 — 게임 DB 만 내리고 정상 토큰으로 접속 (토큰 검증은 통과, 계정 조회에서 예외)
sqlcmd -S $pipe -E -Q "ALTER DATABASE DungeonLadersDB SET OFFLINE WITH ROLLBACK IMMEDIATE"
Server\DummyClient\bin\Debug\net8.0\DummyClient.exe
sqlcmd -S $pipe -E -Q "ALTER DATABASE DungeonLadersDB SET ONLINE"
```

`sample.ps1` (측정에 쓴 스크립트 전문 — 저장소에 넣지 않았다):

```powershell
param([int]$Seconds = 60, [int]$Interval = 2, [string]$Out = "sample.csv",
      [string[]]$Names = @("GameServer","DummyClient","AccountServer"))
$cpuCount = (Get-CimInstance Win32_ComputerSystem).NumberOfLogicalProcessors
$prev = @{}; $rows = @(); $t0 = Get-Date
for ($i = 0; $i -le [math]::Ceiling($Seconds / $Interval); $i++) {
    $now = Get-Date
    foreach ($p in (Get-Process -Name $Names -ErrorAction SilentlyContinue)) {
        $key = "$($p.Name)-$($p.Id)"; $cpuSec = $p.TotalProcessorTime.TotalSeconds; $pct = $null
        if ($prev.ContainsKey($key)) {
            $dt = ($now - $prev[$key].Time).TotalSeconds
            if ($dt -gt 0) { $pct = [math]::Round(($cpuSec - $prev[$key].Cpu) / $dt / $cpuCount * 100, 1) }
        }
        $prev[$key] = @{ Time = $now; Cpu = $cpuSec }
        $rows += [pscustomobject]@{ T = [math]::Round(($now - $t0).TotalSeconds); Proc = $p.Name; Pid = $p.Id
            CpuPct = $pct; PrivMB = [math]::Round($p.PrivateMemorySize64/1MB,1)
            WSMB = [math]::Round($p.WorkingSet64/1MB,1); Threads = $p.Threads.Count }
    }
    Start-Sleep -Seconds $Interval
}
$rows | Export-Csv $Out -NoTypeInformation
```

2차는 같은 조건으로 두 번 돌렸다. 첫 번째는 더미 계정 230개와 게임 DB 계정·기본 캐릭터(계정당 3개 → `Player` 690행)를 **처음 만들면서** 들어간 회차이고,
두 번째는 게임 서버 3개를 새로 띄우고(유휴 기준선을 깨끗하게 잡으려고) 이미 있는 계정으로 들어간 회차다. **아래 4절 숫자는 두 번째 회차**이고, 첫 번째 회차와 다른 점은 관찰 6 · 7 에 적었다.

---

## 4. 2차 결과

### 4-1. 유휴 상태 — 게임 서버 3개(방금 기동), 접속자 0명, 20초 (프로세스당 10 샘플)

| 프로세스 | CPU 평균 | CPU 최대 | Private Bytes | Working Set | 스레드 |
|---|---|---|---|---|---|
| GameServer (Perion 6666) | **21.9 %** | 23.4 % | 38.3 MB | 98.7 MB | 18 |
| GameServer (Henesis 7777) | **22.1 %** | 23.2 % | 37.6 MB | 96.9 MB | 18 |
| GameServer (Kunning 8888) | **21.7 %** | 23.6 % | 36.8 MB | 94.9 MB | 18 |
| AccountServer | 0.0 % | 0.0 % | 154.8 MB* | 211.9 MB | 26 |

\* AccountServer 는 재기동하지 않아 직전 회차(230 계정 생성·로그인)를 치른 상태다. 처음 떴을 때는 38.2 MB 였다 (관찰 8).

### 4-2. DummyClient 230 세션 — 로그인 → 게임 입장 → 120초 유지 (프로세스당 61 샘플)

| 프로세스 | CPU 평균 | CPU 최대 | Private 평균 | Private 최대 | WS 최대 | 스레드 최대 |
|---|---|---|---|---|---|---|
| GameServer (Perion, 90 세션) | 20.3 % | 22.2 % | 131.0 MB | **178.7 MB** | 250.0 MB | 29 |
| GameServer (Henesis, 100 세션) | 20.4 % | 23.2 % | 176.5 MB | **213.9 MB** | 284.3 MB | 28 |
| GameServer (Kunning, 40 세션) | 20.1 % | 23.0 % | 78.5 MB | **86.5 MB** | 157.5 MB | 28 |
| AccountServer (230 계정 로그인) | 0.2 % | 11.6 % | 240.1 MB | 245.9 MB | 305.7 MB | 37 |
| DummyClient (230 세션) | 0.1 % | 1.2 % | 56.4 MB | 60.5 MB | 91.2 MB | 42 |

| 접속 · 로그인 결과 (DummyClient `[stats]` 최종) | 값 |
|---|---|
| `connected` (DummyClient 세션 수) | **230** |
| `loginOk` / `loginFail` | **230 / 0** |
| `inGame` (첫 `S_EnterGame` 받은 세션) | **230** |
| `disconnected` (서버가 끊은 세션) | **0** |
| `respawn` (두 번째 이후 `S_EnterGame`, 120초 누적) | **2,631** — 관찰 6 |
| `Established` 연결 수 (포트별, 120초 내내) | 6666: **90** / 7777: **100** / 8888: **40** |
| GameServer 콘솔 `OnRecvCompleted Failed` | **0** |

| 왕복 시간 (230 샘플, ms) | avg | **p50** | **p95** | max |
|---|---|---|---|---|
| `loginRtt` — `C_Login` 송신 → `S_Login` 수신 | 146 | **109** | **328** | 438 |
| `enterRtt` — `C_EnterGame` 송신 → `S_EnterGame` 수신 | 110 | **109** | **110** | 219 |

접속 시점 전후 시계열 (Henesis, 100 세션 — 다른 두 인스턴스도 같은 모양):

| 경과 | CPU | Private | WS | 스레드 | 비고 |
|---|---|---|---|---|---|
| 10 s | 21.8 % | 36.4 MB | 94.2 MB | 17 | 유휴 |
| 20 s | 20.6 % | 36.9 MB | 95.1 MB | 17 | DummyClient 기동 (AccountServer 로그인 230건에 약 5초) |
| 30 s | 21.4 % | **213.3 MB** | **283.3 MB** | **28** | 100 세션 로그인 + 게임 입장 완료 |
| 40 s | 21.4 % | 209.0 MB | 279.6 MB | 28 | |
| 51 s | 20.7 % | 179.2 MB | 248.7 MB | 15 | GC + 스레드풀 IOCP 스레드 회수 |
| 61 ~ 111 s | 19.5 ~ 21.5 % | 179.2 ~ 180.0 MB | 249 MB | 14 | 안정. 100명이 죽고 살아나는 중인데 메모리가 늘지 않는다 |
| 141 s | 20.6 % | 174.6 MB | 245.7 MB | 13 | |

세션당 메모리 (안정 구간 − 유휴): Perion (124.7 − 38.9) / 90 ≈ **0.95 MB**, Henesis (179.2 − 36.9) / 100 ≈ **1.4 MB**, Kunning (81.1 − 36.8) / 40 ≈ **1.1 MB**.
1차의 "접속만" 0.26 MB 와 비교하면 게임 입장(플레이어 객체 · 인벤토리 · `VisionCube` · 로비 캐릭터 목록) 몫이 세션당 약 1 MB 다.

### 4-3. 가짜 토큰 (`--bogus`, AccountDbId 900000~, Token 1) — 230 세션

| 항목 | 값 |
|---|---|
| DummyClient `[stats]` (첫 10초 시점부터 끝까지 동일) | `connected=0 loginOk=0 loginFail=230 inGame=0 disconnected=230` |
| GameServer `[Login] 토큰 검증 실패 ... → 접속 종료` | 90 / 100 / 40 건 |
| GameServer `OnRecvCompleted Failed` | 0 건 |
| `Established` 연결 (10초 뒤부터) | 0 |

### 4-4. 좀비 세션 재현 — 게임 DB 만 OFFLINE, 정상 토큰 230 세션

토큰 검증(SharedDB)은 통과하고, 바로 다음의 `AppDbContext` 계정 조회가 Recv 스레드에서 예외를 던지는 경로다. 수정 전이라면 1차처럼 230 세션이 붙은 채 남아야 한다.

| 항목 | 값 |
|---|---|
| `Established` 연결 | 접속 직후 90 / 100 / 40 → **10초 뒤 0** |
| DummyClient `[stats]` | `connected=0 loginOk=0 loginFail=0 disconnected=230` |
| GameServer `OnRecvCompleted Failed System.InvalidOperationException: ... transient failure ...` | 90 / 100 / 40 건 (예외는 그대로 난다) |
| GameServer `Connected (N)` 최종 | **0** |

→ 5-3 의 수정이 의도대로 동작한다. 예외를 없앤 것이 아니라 **예외가 난 세션을 끊어서** 좀비로 남지 않게 한 것이다.

### 4-5. 아직 측정 못 한 것

| 항목 | 이유 |
|---|---|
| `GameLogic.Update()` tick 소요 시간 | 서버에 tick 시간 계측 코드가 없다. CPU% 로는 보이지 않는다 (관찰 1) |
| 세션 수를 늘려가며 무너지는 지점 | 이번엔 230 고정. `connectList` 를 500 / 1000 으로 올리는 것은 다음 |
| 이동 · AOI 갱신 부하 | 더미가 `C_Move` 를 보내지 않는다. 다만 몬스터가 더미를 죽이고 리스폰시키는 트래픽은 실제로 발생했다 (관찰 6) |
| Release 빌드 | 전부 Debug (관찰 5) |

---

## 5. 관찰

### 관찰 1 — 유휴 CPU 20~25% 는 "부하"가 아니라 스핀 루프다

접속자 0명인데 인스턴스마다 **21.7 ~ 22.1 %** (1차 24.9 %) 다. 230 세션이 들어와 계속 죽고 살아나도 20.1 ~ 20.4 % 로 오히려 조금 낮다.
`Program.cs` 의 세 스레드가 전부 `while(true) { ...; Thread.Sleep(0); }` 로 돌기 때문이다.

```
GameLogic 스레드  ─┐
DB       스레드  ─┼─ 각각 논리 코어 1개를 거의 100% 점유 → 3 / 12 코어 ≈ 25 %
NetworkSend 스레드 ┘
```

즉 **이 서버에서 프로세스 CPU% 는 부하 지표로 쓸 수 없다.** 부하가 0이든 230이든 같은 값이다.
실제 부하를 보려면 `GameLogic.Update()` 한 바퀴의 소요 시간(tick time)을 서버 안에서 재야 한다.
(`Thread.Sleep(0)` → `Thread.Sleep(1)` 또는 잡 큐 이벤트 대기로 바꾸면 유휴 CPU 는 거의 0 이 된다 — 4채널이면 유휴에 12코어 중 12개를 태우는 셈이라 실 배포에선 반드시 손봐야 한다.)

### 관찰 2 — 230 세션 동시 접속 · 로그인 · 입장 자체는 문제없다

`Connector` 가 10 ms 간격으로 소켓을 여는 구조라 230개가 약 2.3초에 걸쳐 붙었고, 접속 실패 0 · 로그인 실패 0 · 입장 실패 0 이다.
피크 메모리는 Henesis 기준 213.9 MB 로, 32 GB PC 에서 의미 있는 숫자가 아니다. 120초 동안 메모리가 늘지 않았다 (179 → 175 MB).

### 관찰 3 — DB 예외가 나면 세션이 "좀비"가 된다 (서버 버그 → **수정함**, 5-3)

`HandleLogin` 은 Recv 스레드에서 동기로 DB 를 읽는다. 예외가 나면 `Session.OnRecvCompleted` 의 `catch` 가 잡아서
프로세스는 죽지 않지만, **`RegisterRecv()` 가 다시 걸리지 않는다.**

```csharp
// ServerCore/Session.cs (수정 전)
try {
    int processLen = OnRecv(...);   // ← 여기서 SqlException
    ...
    RegisterRecv();                 // ← 실행되지 않음
}
catch (Exception e) { Console.WriteLine($"OnRecvCompleted Failed {e}"); }   // 세션은 그대로 남는다
```

결과: 소켓은 열려 있고 `SessionManager` 에도 남아 `BusyScore` 에 포함되지만 **아무것도 수신하지 못한다.**
1차에서 230개 전부 이 상태가 됐고, 서버 콘솔의 `Connected (N)` 은 끝까지 줄지 않았다.
DB 가 잠깐 죽었다 살아나는 상황에서 "접속은 되는데 로그인이 안 된다"는 증상으로 나타났을 것이다.

### 관찰 4 — 접속 폭주 시 스레드가 17 → 28 로 늘었다가 돌아온다

Recv 완료 콜백(IOCP)에서 DB 조회를 동기로 하니, 100 세션이 2초 안에 로그인하면 스레드풀이 그만큼 스레드를 늘린다 (1차 38, 2차 28~29).
DB 가 정상이어도 로그인 폭주 시 같은 일이 벌어진다 — DB 지연이 곧 스레드풀 팽창으로 이어지는 구조다.
`loginRtt` p95 가 p50 의 3배(328 vs 109 ms)인 것도 같은 원인이다 (관찰 7).
로그인/DB 작업을 별도 잡 큐(이미 있는 `DbTransaction` 스레드)로 넘기는 것이 다음 개선점이다.

### 관찰 5 — Release 가 아니라 Debug 빌드다

위 숫자는 전부 Debug 빌드다. 게임 로직 부하를 잴 때는 `-c Release` 로 다시 해야 한다. (관찰 1 의 스핀 루프는 빌드 구성과 무관하다.)

### 관찰 6 — 가만히 서 있는 더미는 몬스터에게 10초마다 죽는다 → `S_EnterGame` 이 반복된다

첫 회차에서 `inGame` 이 230 을 넘어 10초마다 약 220씩 계속 늘고 `enterRtt` 도 끝없이 커졌다.
서버 로그를 보니 `해골 병사_N -> MyMage_dummyN Kill` 이 120초 동안 Perion 1,066 / Henesis 990 / Kunning 636 건 — 더미가 입력을 안 보내니 스폰 위치에서 몬스터에게 맞아 죽고,
서버가 리스폰하면서 `GameRoom.EnterGame` → **`S_EnterGame` 을 다시 보낸다.** 더미는 그걸 "또 입장했다"로 셌던 것이다.

- DummyClient 계측을 고쳤다: 첫 `S_EnterGame` 만 `enterRtt` 로 재고, 이후는 `respawn` 으로 센다.
- 덕분에 "접속만 하고 가만히 있는" 시나리오가 아니었다. 230명이 **1초에 약 22번 죽고 살아나며** 공격 · 데미지 · 사망 · 리스폰 · 시야 내 `S_Spawn`/`S_Despawn` 이 계속 나가는 상태의 숫자다. 그 상태에서도 메모리는 늘지 않았고 끊긴 세션은 0 이다.
- 반대로 말하면 "가만히 있는 유휴 230명"의 숫자는 아니다. 그걸 재려면 더미를 안전 지대에 스폰시키거나 몬스터를 끄고 다시 돌려야 한다.

### 관찰 7 — 왕복 시간의 바닥은 100 ms 송신 주기다

`enterRtt` 가 p50 = p95 = 109~110 ms 로 거의 상수다. 서버 `Send` 가 큐에 넣기만 하고 `NetworkTask` 스레드가 **100 ms 주기로 flush** 하기 때문에, 응답 왕복은 처리 시간과 무관하게 0 ~ 100 ms 를 기다린다.
`loginRtt` 도 p50 은 같은 109 ms 인데 p95 가 328 ms, max 438 ms 다 — 230건이 2.3초 안에 몰리면서 SharedDB 토큰 조회 + 게임 DB 계정 조회가 Recv 스레드에서 서로 기다린 시간이다.
계정을 처음 만들며 들어간 회차(게임 계정 + 캐릭터 3개 INSERT 포함)도 avg 150 / p50 109 / p95 329 / max 532 ms 로 크게 다르지 않았다.

→ 이 숫자로 "몇 명까지 버티나"에 답하려면 왕복 시간이 아니라 tick 시간을 재야 한다. 왕복 시간은 flush 주기에 가려서 1,000명이 돼도 109 ms 로 보일 수 있다.

### 관찰 8 — AccountServer 메모리가 회차마다 늘었다 (원인 미확인)

38.2 MB (기동) → 160.5 MB (1회차 230 로그인 후) → 245.9 MB (2회차 후). 회차당 약 90~100 MB 다.
Kestrel/EF 의 워밍업일 수도 있고 뭔가 쌓이는 것일 수도 있다. 3회차 이상 돌려보지 않아 판단하지 않는다. 다음에 볼 것.

---

## 5-3. 이번에 고친 서버 코드 — 좀비 세션 (`ServerCore/Session.cs`, 4줄)

```csharp
catch (Exception e)
{
    Console.WriteLine($"OnRecvCompleted Failed {e}");
    Disconnect();          // ← 추가. 컨텐츠 핸들러가 예외를 던진 세션은 끊어서 SessionManager 에서도 빠지게 한다
}
```

"예외가 나도 살려두자"는 선택지도 있었지만, 이 서버 구조에서는 살려둬도 다시 수신할 수 없어 아무 의미가 없다.
끊으면 클라이언트는 재접속으로 복구할 수 있고, `BusyScore` 도 거짓 인원을 세지 않는다. 4-4 에서 재현 → 확인했다.
(인증 수정([AUTH_FIX.md](AUTH_FIX.md))의 토큰 조회 쪽은 이것과 별개로 `try/catch` + 거부 응답(fail-closed)으로 처리한다 — 클라이언트에게 "거부됐다"를 알려줄 수 있는 경로라서다.)

---

## 6. 다음에 볼 것

### 6-1. 이 PC 준비 (끝남)

LocalDB 2022 + go-sqlcmd 가 들어갔고 DB 3개가 만들어져 있다. 인스턴스는 `sqllocaldb start MSSQLLocalDB` 로 올린다 (유휴 시 자동 종료됨).
go-sqlcmd 는 `-S` 에 `sqllocaldb info MSSQLLocalDB` 의 파이프 이름을 줘야 한다 (1절).

### 6-2. 실행 순서

```powershell
cd Server/AccountServer ; dotnet run            # https://localhost:5001
# 게임 서버 3개 (3절)
cd Server/DummyClient/bin/Debug/net8.0 ; .\DummyClient.exe        # 또는 --bogus
#   → 10초마다 [stats] 한 줄: connected / loginOk / loginFail / inGame / respawn / disconnected / loginRtt / enterRtt (avg/p50/p95/max ms)
# 동시에 sample.ps1 -Seconds 140
```

### 6-3. DummyClient 에 추가한 계측 (서버 무변경)

| 출력 | 의미 | 경로 |
|---|---|---|
| `loginRtt` | `C_Login` 송신 → `S_Login` 수신 | Recv 스레드에서 SharedDB 토큰 조회 + 게임 DB 계정 조회 |
| `enterRtt` | `C_EnterGame` 송신 → **첫** `S_EnterGame` 수신 | Recv → GameLogic 잡 큐 → 100 ms 주기 Send flush (**하한 ~100 ms**) |
| `respawn` | 두 번째 이후 `S_EnterGame` | 몬스터에게 죽고 리스폰된 횟수 (관찰 6) |
| `disconnected` | 서버가 끊은 세션 수 | 원래 DummyClient 는 끊겨도 카운트가 줄지 않았다 — `OnDisconnected` 에서 제거하도록 고침 |

### 6-4. 그 다음에 볼 것

1. `GameLogic.Update()` 소요 시간을 서버에서 재서 로그로 남기기 — 관찰 1 · 7 모두 "CPU% 와 왕복 시간으로는 안 보인다"로 끝난다
2. `connectList` 를 230 → 500 → 1000 으로 올리며 tick 시간과 `loginRtt` p95 가 꺾이는 지점 찾기
3. 더미가 주기적으로 `C_Move` 를 보내게 해서 AOI(`VisionCube`) 비용을 의도적으로 발생시키기 (지금은 몬스터가 만들어주는 트래픽뿐이다)
4. 로그인 DB 조회를 `DbTransaction` 큐로 옮기고 관찰 4 의 스레드 팽창과 p95 가 줄어드는지 보기
5. AccountServer 메모리 (관찰 8) 3회차 이상 돌려서 계속 느는지 확인
6. Release 빌드로 1~3 반복
