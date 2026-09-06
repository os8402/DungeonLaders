# 포트폴리오로서의 현재 위치와 다음 할 일

2026-08-28 작성. Unity 6 / .NET 8 현대화를 끝내고 실제로 플레이까지 확인한 뒤,
**"이 프로젝트가 기술적으로 강점이 있나?"** 라는 질문을 스스로 던져 점검한 기록이다.

작업 자체의 기록은 [MIGRATION.md](MIGRATION.md) 에 있다. 이 문서는 **앞으로 뭘 할지**에 대한 것이다.

---

## 1. 결론부터

**기술이 빈약한 게 아니라, 알아보기 쉬운 게 문제다.**

`ServerCore` / `JobSerializer` / `GameRoom` / `Zone` / `VisionCube` 라는 구성과
"N일차" 커밋 메시지는, 국내 MMO 서버 강의를 아는 사람이면 바로 알아본다.
그러면 "완주했구나"까지는 읽히지만 **"설계했구나"로는 읽히지 않는다.**
같은 구조의 코드를 낸 지원자가 여럿이기 때문이다.

내용 자체는 신입 포트폴리오에서 흔치 않다 — 관심영역 동기화, 단일 스레드 게임 로직,
scatter-gather 송신 배칭, 서버 권위 판정. 문제는 그게 **내 것으로 보이지 않는다**는 것.

→ 그래서 방향은 "새 기능 추가"가 아니라
**"강의가 남긴 것을 내가 찾아내고 메웠다"를 만들 수 있는가**로 잡는다.

---

## 2. 이미 차별화되어 있는 것 (지키기)

현대화 과정에서 나온 것들은 **어떤 강의에도 없다.** 스스로 판단해야 나오는 것들이다.

- `Microsoft.Data.SqlClient` 4.0 의 `Encrypt` 기본값 변경 → 컴파일도 되고 서버도 뜨는데 DB 만 안 붙음
- `Dns.GetHostEntry().AddressList[1]` 이 Hyper-V 가상 어댑터를 집는 것 → 실측으로 확인
- `sqlcmd` 가 인덱스 생성 실패에도 종료 코드 0 을 반환 → 반쯤 깨진 DB 가 "성공"으로 보임
- `System.Timers.Timer` 가 예외를 삼켜서, DB 가 없어도 서버가 멀쩡해 보이던 것

[MIGRATION.md](MIGRATION.md) 4장(증상 → 원인 추적)이 지금 이 저장소에서 **가장 값어치 있는 부분**이다.
면접에서 이 얘기를 하는 지원자와 "AOI 구현했습니다"라고 하는 지원자는 다르게 읽힌다.

---

## 3. 확인된 약점 (= 기회)

전부 코드를 직접 열어 확인한 것이다. 추정이 아니다.

### 3-1. ✅ 인증이 통째로 우회 가능하다 — 가장 큰 건 (2026-09-05 해결)

> **해결됨** — 게임 서버가 `SharedDB.Token` 을 실제로 대조하고, 만료 계산 버그도 고쳤다.
> 무엇이 뚫려 있었고 어떻게 막았는지, 230 세션으로 어떻게 검증했는지는 👉 **[AUTH_FIX.md](AUTH_FIX.md)**
> (비밀번호 해싱 3-2 는 아직이다.) 아래는 수정 전 상태를 기록으로 남긴 것이다.

**계정 시스템이 둘인데 서로 연결되어 있지 않다.**

| | 테이블 | 식별자 | 검증 |
|---|---|---|---|
| AccountServer | `AccountDB.Account` | 이름 + 비밀번호 | 있음 |
| GameServer | `DungeonLadersDB.Account` | **설치 경로 해시** | **없음** |

클라이언트는 게임 서버에 이렇게 자기를 알린다.

```csharp
// Client/Assets/Scripts/Packet/PacketHandler.cs:151
loginPacket.UniqueId = Application.dataPath.GetHashCode().ToString();
```

게임 서버는 그 문자열을 그대로 계정 이름으로 쓴다.

```csharp
// Server/GameServer/Session/ClientSession_PreGame.cs:21
//보안 체크~                          ← 주석만 있고 실제 검증이 없다
if (ServerState != PlayerServerState.ServerStateLogin) return;

// :31
.Where(a => a.AccountName == loginPacket.UniqueId)
// :91  없으면 그냥 새로 만든다
AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
```

**결과**

- AccountServer 로그인 / 토큰 발급은 **게임 진입에 아무 영향이 없다.** 완전히 장식이다.
- 9999 포트에 직접 붙어 아무 `UniqueId` 나 보내면 그 계정으로 접속된다. 비밀번호가 필요 없다.
- 없는 ID 를 보내면 계정이 자동 생성된다.
- `Token` 은 `GameServer` 전체에서 **단 한 번도 사용되지 않는다** (전체 검색으로 확인).

곁들여, 토큰 만료는 애초에 동작하지 않는다.

```csharp
// Server/AccountServer/Controllers/AccountController.cs:98
DateTime expired = DateTime.UtcNow;
expired.AddSeconds(600);   // DateTime 은 immutable — 반환값을 버려서 아무 일도 안 일어난다
```

`Expired` 에는 항상 "지금"이 들어간다. 어차피 아무도 검사하지 않지만.

### 3-2. 🔴 비밀번호가 평문이다

DB 를 열면 그대로 보인다.

```
3 | tester01 | 313150
```

### 3-3. 🟡 테스트가 0개다

서버 솔루션에 테스트 프로젝트가 없고, Unity 쪽에도 테스트가 없다.
(`com.unity.test-framework` 는 매니페스트에 있지만 쓰이지 않는다)

### 3-4. ✅ 부하 도구는 있는데 측정한 숫자가 없다 (2026-09-06 측정)

> **측정함** — 230 세션이 실제로 로그인·게임 입장까지 들어간 상태의 CPU · 메모리 · 왕복 시간을 기록했다 👉 **[LOADTEST.md](LOADTEST.md)**
> 유휴 CPU 25% 가 스핀 루프라 CPU% 는 부하 지표로 못 쓴다는 것, 접속 폭주 시 세션이 좀비가 되는 서버 버그(수정함)도 거기서 나왔다.
> tick 시간 계측과 세션 수를 늘려가며 꺾이는 지점 찾기는 아직이다.

`DummyClient` 는 3개 채널에 **230 세션**(90/100/40)을 붙이도록 이미 만들어져 있다.
그런데 돌려본 결과가 어디에도 기록되어 있지 않았다. **도구는 있고 데이터가 없었다.**

### 3-5. 🟢 컴파일되지 않는 죽은 폴더

`Server/Common/Packet/` 은 `Server.sln` 에 없어서 빌드되지 않는다.
Protobuf 이전 세대의 수동 직렬화 코드이고, 정의조차 없는 `SendBufferHelper` 를 참조한다.
동작에는 영향이 없지만 리뷰어가 혼동한다.

---

## 4. 우선순위

효과 ÷ 비용 순서다. 위에서부터 하면 된다.

### 1순위 — 인증 구멍 메우기 (반나절) — ✅ 완료, [AUTH_FIX.md](AUTH_FIX.md)

가장 크다. **보안 문제를 스스로 찾아 메운 서사**가 생기고, 그건 강의에 없다.

- `C_Login` 패킷에 `AccountDbId` + `Token` 을 싣는다 (지금은 `UniqueId` 문자열 하나)
- `GameServer.HandleLogin` 이 `SharedDB.Token` 을 조회해 검증한다 — 테이블은 **이미 있다**
- 만료 검사도 넣는다 (`expired.AddSeconds(600)` 버그부터 수정: `expired = expired.AddSeconds(600)`)
- `DungeonLadersDB.Account` 를 설치 경로가 아니라 `AccountDbId` 로 연결한다
- 비밀번호는 해싱해서 저장한다 (`ASP.NET Core Identity` 의 `PasswordHasher` 또는 BCrypt)

> ⚠️ `.proto` 를 고치면 `PacketGenerator` 로 재생성해야 하고,
> 클라이언트 `Protocol.cs` 도 같이 갱신해야 한다. 양쪽 버전이 어긋나면 조용히 깨진다.

### 2순위 — 부하 측정하고 숫자 남기기 (반나절) — ✅ 1차 완료, [LOADTEST.md](LOADTEST.md) (tick 계측 · 한계점 탐색은 남음)

도구가 이미 있으니 **돌리고 기록만 하면 된다.** 숫자 하나가 서술 열 줄을 이긴다.

- 230 세션 접속 시 GameLogic tick 소요 시간, 메모리, CPU
- 세션 수를 늘려가며 tick 이 무너지는 지점 찾기
- README 에 표로: "N 세션에서 tick 평균 X ms"

면접에서 "몇 명까지 버티나요?"는 거의 반드시 나온다. 지금은 답할 근거가 없다.

### 3순위 — 플레이 GIF (1시간)

서류 심사에서 **가장 먼저 보는 것.** 리뷰어가 30초 안에 "돌아가는 게임이구나"를 아는 것과
모르는 것의 차이가 크다. README 최상단에 넣는다.

### 4순위 — 핵심 경로 테스트 (하루)

전부 할 필요 없다. 설명하기 좋은 것만.

- `RecvBuffer` — TCP 분할 수신 / 병합 수신 경계 처리
- `Map.FindPath` — A* 가 막힌 길에서 제대로 실패하는지
- AOI 진입/이탈 시 `S_SPAWN` / `S_DESPAWN` 이 정확히 한 번씩 나가는지

서버 포지션에서 **테스트가 있는 포트폴리오는 눈에 띈다.**

### 5순위 — CI / Docker (반나절씩)

- GitHub Actions 로 `dotnet build` + `CIBuild.BuildWindows` → "빌드되는 프로젝트"가 배지로 증명됨
- Docker Compose 로 SQL Server + 서버 일괄 기동 → 리뷰어가 5분 안에 실행 가능

---

## 5. 짚고 갈 것

**포트폴리오 하나로 서류 탈락이 풀리지는 않는다.**

100% 탈락이라면 이력서 본문, 지원 직무 범위, 지원 회사 티어 쪽 문제일 가능성이 더 크다.
이 저장소를 고치는 것은 그중 하나의 변수일 뿐이다.
포트폴리오만 계속 다듬으면서 다른 변수를 점검하지 않으면 결과가 안 바뀔 수 있다.

---

## 부록 — 바로 찾아갈 위치

| 할 일 | 파일 |
|---|---|
| 토큰 검증 | `Server/GameServer/Session/ClientSession_PreGame.cs:19` |
| 토큰 발급/만료 버그 | `Server/AccountServer/Controllers/AccountController.cs:98` |
| 클라 로그인 패킷 | `Client/Assets/Scripts/Packet/PacketHandler.cs:150` |
| 비밀번호 저장 | `Server/AccountServer/Controllers/AccountController.cs` (create/login) |
| 부하 테스트 설정 | `Server/DummyClient/Program.cs` (portList / connectList) |
| 패킷 재생성 | `Common/protoc-3.17.3-win64/bin/` + `Server/PacketGenerator/` |
