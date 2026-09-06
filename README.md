# DungeonLaders

2D 탑다운 MMORPG — **Unity 클라이언트 + C# 전용 게임 서버**를 직접 구현한 개인 프로젝트.

상용 네트워크 엔진(Mirror, Photon, Netcode for GameObjects)을 쓰지 않고
**소켓 계층부터 패킷 직렬화, 관심영역 동기화, DB 연동까지 전부 직접 작성**했다.

> **2026-08 현대화 완료** — 2021년 Unity 2019.4 / .NET Core 3.1 로 만든 프로젝트를
> **Unity 6.2 / .NET 8 LTS** 로 올려 현재 툴체인에서 그대로 빌드·실행된다.
> 무엇이 왜 깨졌고 어떻게 고쳤는지는 👉 **[docs/MIGRATION.md](docs/MIGRATION.md)**

---

## 빌드 상태

| 대상 | 버전 | 상태 |
|---|---|---|
| Unity 클라이언트 | Unity 6000.2.10f1 | ✅ 플레이어 빌드 성공 (에러 0 / 경고 0) |
| 게임 서버 | .NET 8 LTS | ✅ 빌드 성공 · 정상 기동 |
| DB | SQL Server LocalDB + EF Core 8 | ✅ |

---

## 구현 범위

### 네트워크 — 직접 만든 부분

| 계층 | 내용 |
|---|---|
| **소켓** | `SocketAsyncEventArgs` 기반 비동기 I/O. 세션당 send/recv args 를 재사용해 이벤트 객체 할당 제거 |
| **패킷 조립** | 세션당 64KB `RecvBuffer` 를 재사용하며 TCP 스트림 경계 문제(분할·병합 수신)를 처리 |
| **송신 배칭** | 대기 중인 패킷들을 `_pendingList` 에 모아 `SocketAsyncEventArgs.BufferList`(scatter-gather)로 **한 번의 `SendAsync`** 에 전송 → syscall 횟수 감소 |
| **직렬화** | Protocol Buffers (34종 메시지) |
| **코드 생성** | `PacketGenerator` 가 `.proto` → 패킷 ID enum · 핸들러 디스패치 테이블 자동 생성 |

### 서버 아키텍처

```
                    ┌──────────────┐
   로그인 ─────────▶ │ AccountServer│ 계정 인증 · 토큰 발급 (ASP.NET Core Web API)
                    └──────┬───────┘
                           │  SharedDB (서버 목록 · 혼잡도 · 토큰)
                    ┌──────┴───────┐
   서버 선택 ───────▶│  GameServer  │ × 4 인스턴스
                    └──────────────┘   엘리니아 · 헤네시스 · 페리온 · 커닝시티
```

**스레드 모델** — 게임 로직은 **단일 스레드**로 유지하고, 나머지를 분리해 락 경합을 없앴다.

| 스레드 | 역할 |
|---|---|
| Recv (N) | 소켓 수신 → 잡(Job) 으로 변환해 큐잉 |
| **GameLogic (1)** | 모든 게임 상태 변경을 여기서만 수행 → **동기화 코드 불필요** |
| Send (1) | 세션별 송신 버퍼 flush |
| DbTask (1) | DB 쓰기를 비동기 큐로 분리 → 게임 루프가 DB I/O 에 막히지 않음 |

`JobSerializer` 가 커맨드 패턴으로 작업을 직렬화하고, `JobTimer` 가 지연 실행(`PushAfter`)을 담당한다.

### 관심영역(AOI) 동기화

전체 브로드캐스트 대신 **Zone 격자 + VisionCube** 로 시야 내 오브젝트만 동기화한다.

- 맵을 Zone 격자로 분할, 각 Zone 이 `Player` / `Monster` / `Projectile` 집합을 보유
- `VisionCube` 가 100ms 주기로 인접 Zone 을 스캔해 **이전 시야와 차집합**을 계산
- 새로 보이는 것만 `S_SPAWN`, 사라진 것만 `S_DESPAWN` 전송

→ 플레이어 수가 늘어도 **1인당 트래픽이 시야 크기에 비례**하고 전체 인원에 비례하지 않는다.

### 게임 콘텐츠

- **직업 3종** — 전사 / 궁수 / 마법사, 무기 4종(검·활·스태프·창)별 공격 로직
- **몬스터 AI** — A* 길찾기(`Map.FindPath`) 기반 추적. 타일맵 충돌 데이터를 서버가 직접 로드
- **아이템** — 인벤토리, 장착/해제, 사용, 버리기 (모두 서버 권위)
- **성장** — 경험치 · 레벨업 · 스탯
- **투사체** — 화살 등 발사체를 서버에서 시뮬레이션
- **채팅**, **핑/퐁 기반 연결 유지**

> 위치·데미지·아이템 판정은 전부 **서버 권위(server-authoritative)** 로 처리한다.
> 클라이언트는 입력을 보내고 결과를 렌더링만 한다.

### 맵 데이터 파이프라인

Unity 타일맵에서 충돌 정보를 추출해 서버가 읽는 텍스트 포맷으로 내보낸다.
(`Assets/Editor/MapEditor.cs`, 단축키 `Ctrl+Shift+G`)

```
Unity Tilemap ──▶ MapEditor ──▶ Common/MapData/*.txt ──▶ GameServer Map.LoadMap()
```

→ **클라이언트와 서버가 동일한 충돌 데이터**를 공유하므로 판정 불일치가 생기지 않는다.

---

## 기술 스택

**클라이언트** Unity 6000.2.10f1 · C# · Protocol Buffers · TextMeshPro · 2D Tilemap
**서버** .NET 8 · ASP.NET Core · Entity Framework Core 8 · SQL Server · Protocol Buffers

---

## 실행 방법

### 사전 준비
- Unity **6000.2.10f1**
- .NET SDK **8.0 이상**
- SQL Server **LocalDB** (Visual Studio 설치 시 포함)

### 1. 서버 빌드

```bash
cd Server
dotnet restore Server.sln
dotnet build   Server.sln -c Debug
```

### 2. DB 생성

DB 3개(`DungeonLadersDB` · `SharedDB` · `AccountDB`)가 필요하다.
스키마는 EF Core 마이그레이션으로 저장소에 포함되어 있고, 바로 쓸 수 있는 `.sql` 도 함께 있다.

```bash
cd Server/Sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -i 00_CreateDatabases.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d DungeonLadersDB -i 01_DungeonLadersDB.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d SharedDB        -i 02_SharedDB.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d AccountDB       -i 03_AccountDB.sql
```

> `-I`(QUOTED_IDENTIFIER ON) 는 **필수**다. 빼면 인덱스 생성이 실패하는데
> sqlcmd 가 종료 코드 0 을 돌려주기 때문에 성공한 것처럼 보인다. 이유는 아래 문서에.

EF 도구를 쓰거나, LocalDB 없이 Docker 로 띄우는 방법은 👉 **[Server/Sql/README.md](Server/Sql/README.md)**

### 3. HTTPS 개발 인증서 신뢰 (최초 1회)

계정 서버는 `https://localhost:5001` 로 뜨고 클라이언트가 여기에 HTTPS 로 붙는다.
인증서를 신뢰해두지 않으면 **Unity 쪽 로그인 요청이 인증서 오류로 실패한다.**

```bash
dotnet dev-certs https --trust
```

### 4. 서버 실행

```bash
# 계정 서버
cd Server/AccountServer && dotnet run

# 게임 서버 (원하는 채널 선택)
cd Server/GameServer/bin/Debug/Elenia/Build && ./GameServer.exe
```

동작 확인:

```bash
curl -X POST https://localhost:5001/api/account/create \
  -H "Content-Type: application/json" \
  -d '{"AccountName":"test","Password":"test"}'
# → {"CreateOk":true}

curl -X POST https://localhost:5001/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"AccountName":"test","Password":"test"}'
# → {"LoginOk":true,"AccountDbId":1,"Token":...,"ServerList":[{"Name":"엘리니아",...}]}
```

`ServerList` 가 채워져 나오면 게임 서버가 `SharedDB` 에 자기 정보를 등록한 것이다.
비어 있다면 게임 서버가 떠 있는지 확인한다.

각 채널의 설정은 [`Server/Configs/`](Server/Configs/) 에 있다 (포트 · 채널명 · 접속 문자열).

### 5. 클라이언트

`Client/` 를 Unity 로 열고 `Assets/Scenes/Login.unity` 에서 실행.

에디터 없이 빌드만 검증하려면:

```bash
"<Unity>/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath Client -executeMethod CIBuild.BuildWindows -logFile build.log
```

---

## 저장소 구조

```
Client/                  Unity 클라이언트
  Assets/Scripts/
    ServerCore/          클라이언트 측 소켓 계층 (서버와 공유하는 설계)
    Packet/              패킷 핸들러 · 큐
    Managers/            Resource · Sound · UI · Pool · Data · Network 매니저
    Controllers/         플레이어 · 몬스터 · 투사체 컨트롤러
    UI/                  씬 UI · 팝업 · 월드스페이스 UI
  Assets/Editor/
    MapEditor.cs         타일맵 → 서버 충돌 데이터 추출
    CIBuild.cs           커맨드라인 빌드 진입점

Server/
  ServerCore/            소켓 · 세션 · 버퍼 (엔진 비의존)
  GameServer/            게임 로직 · AOI · AI · DB
  AccountServer/         계정 인증 Web API
  SharedDB/              서버 목록 · 토큰 공유 DB
  PacketGenerator/       .proto → C# 패킷 코드 생성
  DummyClient/           부하 테스트 (3개 채널에 230 세션 동시 접속)
  Configs/               채널별 서버 설정
  Sql/                   DB 생성 스크립트 (EF 마이그레이션에서 생성)

Common/
  MapData/               서버가 읽는 맵 충돌 데이터
  protoc-3.17.3-win64/   프로토콜 정의 및 컴파일러

docs/
  MIGRATION.md           2021 → 2026 현대화 상세 기록
  ROADMAP.md             현재 위치 점검과 다음 할 일
  AUTH_FIX.md            인증 우회 구멍(게임 서버가 토큰을 안 봄) 수정 기록
  LOADTEST.md            230 세션 부하 실측 — CPU · 메모리 · 왕복 시간
```

---

## 개발 기록

2021년 8~9월, 28일간 진행한 프로젝트다.
2026년 8월에 최신 툴체인으로 현대화했다 — [docs/MIGRATION.md](docs/MIGRATION.md)

2026년 9월, 게임 서버가 계정 서버 토큰을 전혀 검사하지 않던 인증 구멍을 찾아 메웠다 — [docs/AUTH_FIX.md](docs/AUTH_FIX.md)
같은 달 230 세션 부하 테스트를 실제로 돌려 CPU · 메모리 · 왕복 시간을 기록했다 — [docs/LOADTEST.md](docs/LOADTEST.md)

앞으로의 개선 계획은 [docs/ROADMAP.md](docs/ROADMAP.md) 에 정리해두었다.
