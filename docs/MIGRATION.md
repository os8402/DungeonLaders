# DungeonLaders 현대화 기록 (2021 → 2026)

2021년 9월에 멈춰 있던 MMO 포트폴리오를 **오늘 날짜의 툴체인에서 그대로 빌드·실행되도록** 되살린 작업 기록이다.

게임 로직·기획·리소스는 손대지 않았다. 바뀐 것은 **빌드가 되게 만드는 데 필요한 부분과, 5년 사이 바뀐 플랫폼 규약**뿐이다.

---

## 1. 한눈에 보기

| 구분 | 이전 (2021-09) | 이후 (2026-08) |
|---|---|---|
| Unity 에디터 | 2019.4.3f1 | **6000.2.10f1 (Unity 6.2)** |
| TextMeshPro | `com.unity.textmeshpro` 2.0.1 (별도 패키지) | `com.unity.ugui` 2.0.0 에 흡수 |
| 서버 런타임 | .NET Core 3.1 *(2022-12 지원 종료)* | **.NET 8 LTS** |
| EF Core | 5.0.9 *(2022-05 지원 종료)* | **8.0.10** |
| Google.Protobuf | 3.17.3 | **3.28.3** (서버) |
| 저장소 위생 | 루트 `.gitignore` 없음 · 서버 빌드산출물 458개 커밋됨 | `.gitignore` / `.gitattributes` 정비 |
| 빌드 검증 | 수동(에디터에서만) | **커맨드라인 1줄로 재현 가능** |

### 검증 결과

| 검증 항목 | 결과 |
|---|---|
| 서버 전체 빌드 (`dotnet build Server.sln`) | ✅ **에러 0** / 경고 7 (전부 기존 코드 스타일 경고) |
| 서버 기동 (`GameServer.exe`) | ✅ config 로드 → 데이터 로드 → `Listening...` |
| Unity 임포트 · 스크립트 컴파일 | ✅ **에러 0 / 경고 0** |
| Windows 플레이어 빌드 | ✅ **성공** (에러 0, 경고 0, 118 MB, 46초) |
| DB 구축 (3개 DB · 테이블 6개 · 마이그레이션 10개) | ✅ 빈 LocalDB 에서 전부 생성 |
| **DB 읽기/쓰기** | ✅ 서버가 `SharedDB.ServerInfo` 에 자기 정보 기록 확인 |

> 위 항목들은 전부 실제로 실행해서 확인했다. "컴파일될 것이다"가 아니라 "컴파일된다".

최종 확인은 **빈 LocalDB 인스턴스에서 시작해** 스크립트로 DB 를 만들고 서버를 띄운 뒤,
서버가 `StartServerInfoTask` 로 기록한 행을 직접 조회하는 방식으로 했다.

```console
$ sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d SharedDB -Q "SELECT Name, IpAddress, Port FROM ServerInfo"
엘리니아 | 192.168.219.105 | 9999
```

이게 통과했다는 것은 **접속 문자열 · EF Core 8 · 스키마 · 서버 기동이 한 줄로 이어졌다**는 뜻이다.

> 이 검증이 중요했던 이유: `StartServerInfoTask` 의 DB 쓰기는 `System.Timers.Timer` 안에서 돌기 때문에
> **실패해도 예외가 삼켜져 콘솔에 아무것도 찍히지 않는다.** 즉 `Listening...` 이 떴다고 DB 가 붙은 게 아니다.
> 실제로 DB 를 만들기 전 테스트에서는 서버가 멀쩡히 떠 있었지만
> SQL Server 로그에는 `Login failed ... Failed to open the explicitly specified database 'SharedDB'` 가 쌓이고 있었다.

---

## 2. 클라이언트 (Unity 2019.4.3f1 → Unity 6.2)

### 2-1. 컴파일이 깨지던 것 — `UnityWebRequest`

Unity 2020.2 에서 `isNetworkError` / `isHttpError` 가 폐기되고 이후 **삭제**됐다.
`WebManager` 의 HTTP 응답 분기가 이 두 프로퍼티에 의존하고 있어 그대로는 컴파일 자체가 되지 않는다.

```diff
- if (uwr.isNetworkError || uwr.isHttpError)
+ // Unity 2020.2+ : isNetworkError / isHttpError removed -> UnityWebRequest.result
+ if (uwr.result != UnityWebRequest.Result.Success)
      Debug.Log(uwr.error);
```

`UnityWebRequest.Result` 는 `Success / ConnectionError / ProtocolError / DataProcessingError` 를 구분하므로
기존의 2분기보다 오히려 원인 파악이 쉬워졌다.

📄 `Client/Assets/Scripts/Managers/Contents/WebManager.cs`

### 2-2. 삭제 예정 API — `FindObjectOfType`

Unity 6 에서 `Object.FindObjectOfType` 계열은 **Obsolete** 로 바뀌었다.
새 API 는 "정렬 보장이 필요한가"를 호출자가 명시하게 한다.

| 기존 | 대체 | 차이 |
|---|---|---|
| `FindObjectOfType<T>()` | `FindFirstObjectByType<T>()` | 씬 정렬 순서상 **첫 번째**를 보장 (느림) |
| `FindObjectOfType<T>()` | `FindAnyObjectByType<T>()` | **아무거나** 하나 (빠름) |

이 프로젝트의 3개 호출부는 전부 씬에 단 하나만 존재하는 싱글턴성 객체를 찾는 용도라
정렬 보장이 불필요하다. 따라서 더 빠른 `FindAnyObjectByType` 을 선택했다.

- `Contents/Camera/ChasePlayerCam.cs` — `MyPlayerController`
- `Managers/Core/SceneManagerEx.cs` — `BaseScene`
- `Scenes/BaseScene.cs` — `EventSystem`

### 2-3. 런타임을 깨뜨리는 것 — BCL 어셈블리 중복

`Assets/Libs/` 에 아래 3개 DLL 이 수동으로 들어가 있었다.

```
System.Buffers.dll                          (4.6.26515.06)
System.Memory.dll                           (4.6.28619.01)
System.Runtime.CompilerServices.Unsafe.dll
```

2019.4 시절엔 `Google.Protobuf` 가 `Span<T>` / `Memory<T>` 를 쓰려면 이걸 직접 넣어줘야 했다.
그러나 Unity 6 은 .NET Standard 2.1 프로파일에서 **이 어셈블리들을 런타임에 내장**한다.

```
<Unity>/Editor/Data/NetStandard/compat/2.1.0/shims/netstandard/System.Buffers.dll
<Unity>/Editor/Data/NetStandard/compat/2.1.0/shims/netstandard/System.Memory.dll
```

같은 어셈블리가 둘이 되면 타입 동일성이 깨져 빌드 또는 런타임에서 터진다.
→ **3개 DLL 을 제거**했다. Protobuf 는 이제 Unity 내장 shim 에 바인딩된다.
(플레이어 빌드가 통과한 것으로 실제 바인딩을 확인)

### 2-4. TextMeshPro 패키지 흡수

Unity 6 에서 `com.unity.textmeshpro` 는 **폐기**되고 TMP 가 `com.unity.ugui` 2.0.0 안으로 들어갔다.
매니페스트에 옛 패키지가 남아 있으면 패키지 해석 단계에서 실패한다.

```diff
- "com.unity.textmeshpro": "2.0.1",
  "com.unity.ugui": "2.0.0",
```

`Assets/TextMesh Pro/` 폴더의 에셋들은 GUID 가 유지되어 **참조가 끊기지 않았다**.
다만 TMP 3.x 가 기존 폰트 에셋을 자동 마이그레이션하면서 이런 로그를 남긴다.

```
Font Asset [LiberationSans SDF - Fallback] Units Per EM set to 2048.
Please commit the newly serialized value.
```

→ 재직렬화된 폰트 에셋을 **커밋해야** 다음 사람이 같은 마이그레이션을 반복하지 않는다.

### 2-5. 패키지 매니페스트 전면 갱신

`Packages/manifest.json` 의 버전을 전부 Unity 6.2 동봉 버전으로 맞췄다.
(에디터의 `Resources/PackageManager/Editor/manifest.json` 을 기준으로 함 — 임의로 최신을 찍은 게 아니다)

| 패키지 | 이전 | 이후 |
|---|---|---|
| com.unity.2d.animation | 3.2.3 | 12.0.2 |
| com.unity.2d.psdimporter | 2.1.4 | 11.0.1 |
| com.unity.2d.spriteshape | 3.0.12 | 12.0.1 |
| com.unity.2d.pixel-perfect | 2.0.4 | 5.1.0 |
| com.unity.collab-proxy | 1.2.16 | 2.10.0 |
| com.unity.test-framework | 1.1.14 | 1.6.0 |
| com.unity.timeline | 1.2.15 | 1.8.9 |
| com.unity.ide.rider | 1.1.4 | 3.0.38 |
| com.unity.ide.vscode | 1.2.1 | *(폐기)* → com.unity.ide.visualstudio 2.0.25 |
| com.unity.textmeshpro | 2.0.1 | *(폐기)* → com.unity.ugui 2.0.0 에 포함 |

`com.unity.modules.*` 는 Unity 6.2 에도 전부 존재해서 그대로 두었다.

### 2-6. 에셋 임포터 재직렬화

Unity 6 이 프로젝트를 열면서 `.meta` **66개**를 자동 갱신했다. 예: `TextureImporter serializedVersion 11 → 13`

```diff
-  serializedVersion: 11
+  serializedVersion: 13
+    flipGreenChannel: 0
+  vTOnly: 0
+  ignoreMipmapLimit: 0
-    aniso: -1
-    mipBias: -100
+    aniso: 1
+    mipBias: 0
```

의도된 정상 업그레이드다. `aniso: -1` / `mipBias: -100` 은 구버전의 "미설정" 표현이었고
Unity 6 이 유효한 기본값으로 정규화한 것이다. **반드시 커밋해야** 팀원마다 재임포트가 반복되지 않는다.

### 2-7. 커맨드라인 빌드 추가 (신규)

`Assets/Editor/CIBuild.cs` 를 추가했다. 에디터를 열지 않고 빌드를 검증할 수 있다.

```bash
"<Unity>/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath Client -executeMethod CIBuild.BuildWindows -logFile build.log
```

실패 시 예외를 던지므로 **종료 코드로 성패가 갈린다** → GitHub Actions 등에 그대로 물릴 수 있다.

---

## 3. 서버 (.NET Core 3.1 → .NET 8 LTS)

### 3-1. 대상 프레임워크 일원화

6개 프로젝트가 각자 `<TargetFramework>netcoreapp3.1</TargetFramework>` 를 들고 있었다.
`Server/Directory.Build.props` **하나로 통합**해서, 다음 업그레이드 때 한 줄만 고치면 되게 했다.

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

> `Nullable` / `ImplicitUsings` 를 명시적으로 끈 이유: 2021년 C# 8 스타일 코드에
> 최신 SDK 기본값이 끼어들면 수백 개의 경고가 쏟아진다. 현대화의 목적은
> "돌아가게 만들기"이지 "코드 전면 재작성"이 아니므로 기존 스타일을 존중했다.

### 3-2. 의존 패키지

| 패키지 | 이전 | 이후 | 비고 |
|---|---|---|---|
| Microsoft.EntityFrameworkCore.SqlServer | 5.0.9 | 8.0.10 | 지원 종료 → LTS |
| Microsoft.EntityFrameworkCore.Design / .Tools | 5.0.9 | 8.0.10 | |
| Microsoft.Extensions.Logging.Console | 5.0.0 | 8.0.1 | |
| Newtonsoft.Json | 13.0.1 | 13.0.3 | |
| Google.Protobuf | 3.17.3 | 3.28.3 | 3.17 생성 코드와 호환 확인 |

기존 마이그레이션(`Migrations/*.cs`)과 `Startup.cs` 방식은 .NET 8 에서도 그대로 동작해서 건드리지 않았다.

### 3-3. ⚠️ 조용히 죽는 함정 — SqlClient 의 `Encrypt` 기본값 변경

**이번 작업에서 가장 찾기 어려운 부분이었다.**

`Microsoft.Data.SqlClient` 4.0 부터 접속 문자열의 `Encrypt` **기본값이 `false` → `true` 로 바뀌었다**.
EF Core 8 은 이 버전대를 쓴다. 그런데 GameServer 의 `config.json` 접속 문자열에는 `Encrypt` 가 아예 없었다.

```
Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DungeonLadersDB;
```

즉 패키지만 올리면 **LocalDB 접속이 인증서 오류로 전부 실패**한다.
코드는 멀쩡히 컴파일되고 서버도 뜨는데 DB만 안 붙으므로 원인 추적이 오래 걸린다.

```diff
- Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DungeonLadersDB;
+ Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DungeonLadersDB;
+ Integrated Security=True;Encrypt=False;TrustServerCertificate=True;
```

(`AccountServer/appsettings.json` 은 원래부터 `Encrypt=False` 가 있어 영향 없음)

### 3-4. 요즘 PC 에서 터지는 버그 — `AddressList[1]`

```csharp
IPHostEntry ipHost = Dns.GetHostEntry(host);
IPAddress ipAddr = ipHost.AddressList[1];   // ← 2021년엔 우연히 동작했다
```

2021년 개발 PC 에서는 `AddressList[1]` 이 마침 쓸 만한 IPv4 주소였다.
이 코드를 되살린 PC 에서 실제로 찍어보니 이랬다.

```
[0] fe80::832c:ae16:2a16:3a11   IPv6 링크로컬
[1] fe80::ce9:1bce:4a4:66ed     IPv6 링크로컬    ← 원본이 고르던 값
[2] fe80::4c3f:920d:a997:501e   IPv6 링크로컬
[3] 172.23.64.1                 Hyper-V 가상 스위치
[4] 192.168.219.105             실제 Wi-Fi       ← 이게 정답
[5] 172.18.112.1                에뮬레이터 가상 어댑터
```

즉 원본은 **IPv6 링크로컬 주소를 서버 접속 주소로 DB 에 등록**하고 있었다.
클라이언트는 이 주소로 접속할 수 없다. (어댑터가 1개뿐인 환경이면 `IndexOutOfRangeException` 으로 즉사한다.)

**첫 번째 수정은 충분하지 않았다.** 처음엔 "IPv4 중 첫 번째"로 고쳤는데,
그러면 `[3] 172.23.64.1` — Hyper-V 가상 스위치에 걸린다. 실제로 DB 에 그 주소가 기록되는 것을 보고 알았다.

가상 어댑터를 이름으로 걸러내는 건 환경마다 달라 믿을 수 없다.
그래서 **OS 라우팅 테이블에 직접 물어보는** 방식으로 바꿨다.

```csharp
// UDP 소켓을 외부 주소로 Connect 하면 (UDP 는 실제 패킷을 보내지 않는다)
// OS 가 어떤 인터페이스를 쓸지 결정하고 LocalEndPoint 에 그 주소를 채워준다.
using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
{
    socket.Connect("8.8.8.8", 65530);
    if (socket.LocalEndPoint is IPEndPoint endPoint)
        return endPoint.Address;
}
```

네트워크가 없는 환경(오프라인 CI 등)에서는 링크로컬(`169.254.*`)·루프백을 제외한 IPv4 로 폴백한다.

수정 후 DB 에 기록된 값:

```
엘리니아 | 192.168.219.105 | 9999      ← 실제 Wi-Fi 주소
```

📄 `Server/GameServer/Program.cs` · `Server/DummyClient/Program.cs` (같은 코드가 양쪽에 복사되어 있었다)

### 3-5. 컴파일되지 않는 폴더 발견 — `Server/Common/`

`Server/Common/Packet/` 에 `GenPackets.cs` · `ClientPacketManager.cs` · `ServerPacketManager.cs` 가 있는데
**`Server.sln` 에 포함되어 있지 않아 애초에 컴파일되지 않는다.**

내용도 Protobuf 이전 세대의 수동 직렬화 코드이고, 정의조차 없는 `SendBufferHelper` 를 참조한다.
현재 실제로 쓰이는 것은 각 프로젝트의 `Packet/Protocol.cs` (Protobuf 생성 코드) 다.

이번에는 **삭제하지 않고 남겨두었다** — 동작에 영향이 없고, 삭제는 현대화가 아니라 리팩터링의 영역이기 때문이다.
다만 리뷰어가 혼동할 수 있으므로 여기 기록해 둔다.

### 3-6. 빌드 스텝 수리 — `PreBuildEvent`

원본은 프리빌드에서 cmd `copy` 로 산출물을 서버 인스턴스 4개 폴더에 복사했다.

```
copy "$(TargetDir)" "bin\Debug\Elenia\Build";
copy "$(TargetDir)" "bin\Debug\Henesis\Build";  ...
```

문제:

1. **대상 폴더가 없으면 `copy` 가 실패해서 빌드 전체가 깨진다.** 이 폴더들은 `bin/` 안에 있어
   깨끗하게 클론하면 존재하지 않는다 → *클론 직후 빌드 불가*.
2. cmd `copy` 는 하위 폴더를 복사하지 못한다.

MSBuild `Copy` 태스크로 교체했다 — 폴더 자동 생성 + 재귀 복사 + 증분 복사(`SkipUnchangedFiles`).

```xml
<Target Name="FanOutServerInstances" AfterTargets="Build">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="CopyToServerInstance"
           Properties="InstanceName=%(ServerInstance.Identity);..." />
</Target>
```

### 3-7. DB 구축 절차가 저장소에 없었다

스키마 자체는 EF Core 마이그레이션으로 **원래부터 저장소에 있었다** (총 10개 마이그레이션).
하지만 "그래서 어떻게 DB 를 만드나"가 어디에도 적혀 있지 않았고, 실제로 해보면 걸리는 지점이 있었다.

**① DB 가 2개가 아니라 3개다**

| DB | 소유 프로젝트 | 마이그레이션 |
|---|---|---|
| `DungeonLadersDB` | GameServer | 7개 |
| `SharedDB` | SharedDB | 2개 |
| `AccountDB` | AccountServer | 1개 |

`SharedDB` 는 별도 클래스 라이브러리 프로젝트라 잊기 쉽다. 이게 없으면 서버 목록·토큰이 동작하지 않는다.

**② `AccountServer` 는 `--context` 없이는 실패한다**

`AccountServer` 가 `SharedDB` 를 참조하기 때문에 EF 도구가 컨텍스트를 2개 발견한다.

```console
$ cd Server/AccountServer && dotnet ef database update
More than one DbContext was found. Specify which one to use.
```

```bash
# 올바른 명령
dotnet ef database update --context AccountServer.DB.AppDbContext
```

**③ EF 도구 없이도 되게 — `Server/Sql/` 추가**

리뷰어가 `dotnet-ef` 를 전역 설치해야만 프로젝트를 볼 수 있다면 진입 장벽이 된다.
멱등(idempotent) SQL 스크립트를 뽑아 저장소에 넣었다.

```
Server/Sql/
├── 00_CreateDatabases.sql   DB 3개 생성 (재실행 안전)
├── 01_DungeonLadersDB.sql   Account · Player · Item
├── 02_SharedDB.sql          ServerInfo · Token
├── 03_AccountDB.sql         Account
└── README.md                LocalDB / EF 도구 / Docker 3가지 경로
```

`dotnet ef migrations script` 는 **DB 에 접속하지 않고** 생성되므로,
DB 가 없는 상태에서도 스크립트를 뽑을 수 있다 (이번 작업이 실제로 그 상황이었다).

**④ `sqlcmd -I` 를 빼면 조용히 반쯤 깨진다**

스크립트를 만들어놓고 실제로 돌려봤더니 실패했다.

```
Msg 1934: CREATE INDEX failed because the following SET options have
          incorrect settings: 'QUOTED_IDENTIFIER'.
```

`sqlcmd` 는 기본이 `QUOTED_IDENTIFIER OFF` 인데 EF Core 의 유니크 인덱스는 `ON` 을 요구한다.
`-I` 로 켜주면 된다 — SSMS 는 기본이 `ON` 이라 이 문제를 겪지 않는다.

**더 위험한 건 `sqlcmd` 가 이 오류에도 종료 코드 0 을 돌려준다는 점이다.**
테이블이 절반만 생긴 채 "성공"처럼 보인다. 실제로 첫 시도에서 `Player` 테이블이 없는 상태로
후속 구문이 줄줄이 실패했는데도 스크립트는 `ok` 로 끝났다.
자동화에서 돌린다면 `-b` 를 함께 줘서 오류가 종료 코드에 반영되게 해야 한다.

스키마의 원본은 어디까지나 마이그레이션이다. `.sql` 은 파생물이므로
스키마를 바꾸면 스크립트를 다시 뽑아야 한다 — 그 명령도 `Server/Sql/README.md` 에 적어두었다.

### 3-8. `config.json` 을 소스로 승격

멀티 서버(엘리니아 / 헤네시스 / 페리온 / 커닝시티) 설정이 **`bin/Debug/<이름>/config.json` 에만** 있었다.
즉 저장소에는 빌드산출물로만 존재했고, `.gitignore` 를 제대로 넣는 순간 **영원히 사라질 파일**이었다.

→ `Server/Configs/<이름>/config.json` 으로 옮겨 **소스로 관리**하고,
빌드 시 각 인스턴스 폴더로 자동 배포되게 했다. 실행 레이아웃은 원본과 동일하게 유지된다.

```
Server/GameServer/bin/Debug/<이름>/
├── config.json          ← Server/Configs/<이름>/ 에서 복사됨
└── Build/GameServer.exe ← 실행 (config 를 ../config.json 으로 읽음)
```

---

## 4. 실제로 플레이해보고 드러난 것

빌드가 되고 서버가 뜬 뒤 클라이언트로 로그인을 해보니, **빌드와 무관한 버그 3개**가 나왔다.
전부 2021년부터 있던 것이고, 이번 현대화 과정에서 처음 드러났다.

발단은 "계정이 안 만들어진다"는 증상이었다. 그런데 DB 를 열어보니 계정은 멀쩡히 들어가 있었고,
Unity 에디터 로그에는 로그인이 성공(`True`)으로 찍혀 있었다. 실제로 막힌 건 그 다음 단계였다.

```
OnClickLogin
True                                          ← 로그인 성공
Loaded scene 'Lobby.unity'
OnConnectCompleted Fail: ConnectionRefused    ← 게임 서버 접속 거부
```

### 4-1. 실패하면 아무 일도 일어나지 않았다

`WebManager.CoSendWebRequest` 는 요청이 실패하면 `Debug.Log(uwr.error)` 만 하고 끝났다.
**콜백을 아예 호출하지 않는다.**

```csharp
if (uwr.result != UnityWebRequest.Result.Success)
{
    Debug.Log(uwr.error);   // 여기서 끝. res.Invoke() 는 불리지 않는다.
}
else { ... res.Invoke(resObj); }
```

호출부는 이렇게 생겼다.

```csharp
_loading = Managers.UI.ShowPopupUI<UI_Loading>();
Managers.Web.SendPostRequest<...>(..., (res) => {
    ...
    Managers.UI.ClosePopupUI(_loading);   // 성공했을 때만 실행된다
});
```

즉 **계정 서버가 꺼져 있으면 로딩 팝업이 영원히 안 닫히고 화면이 멈춘 것처럼 보인다.**
성공했을 때조차 화면에는 아무 메시지가 없어서, 성공인지 실패인지 구분할 방법이 콘솔뿐이었다.

→ `onError` 콜백을 추가해 **성공/실패 중 반드시 하나는 호출**되도록 했고,
실패 원인을 `UnityWebRequest.Result` 별로 사람이 읽을 수 있는 문장으로 변환한다.

```csharp
case UnityWebRequest.Result.ConnectionError:
    return "계정 서버에 연결할 수 없습니다. AccountServer 가 실행 중인지 확인하세요.";
```

그리고 프리팹에 있으면서 **아무도 쓰지 않던 `LoginInfoText`** 를 찾아 결과 표시에 연결했다.
(UI 를 새로 만들 필요가 없었다 — 자리는 이미 있었다.)

### 4-2. 빈 계정이 만들어졌다

DB 에 이런 행이 있었다.

```
2 | (빈 문자열) | (빈 문자열)
```

클라이언트에도 서버에도 입력 검증이 없어서, 빈 칸으로 생성 버튼을 누르면 그대로 들어갔다.
→ 양쪽 모두에 `IsNullOrWhiteSpace` 검사를 넣었다. 클라이언트는 요청을 보내기 전에 막고,
서버는 클라이언트를 우회한 요청도 거부한다.

### 4-3. 죽은 서버가 목록에 영원히 남았다 — `ConnectionRefused` 의 진범

`ServerInfo` 테이블에는 게임 서버가 10초마다 자기 정보를 upsert 한다.
그런데 **종료할 때 행을 지우지 않고, 마지막 갱신 시각도 기록하지 않았다.**

결과적으로 한 번이라도 켰던 채널은 서버를 꺼도 목록에 계속 떠 있고,
클라이언트가 그걸 고르면 `ConnectionRefused` 를 맞는다. 이번에 겪은 증상이 정확히 이거였다.

→ `ServerDb.LastPingTime` 컬럼을 추가하고(마이그레이션 `ServerHeartbeat`),
`AccountServer` 가 로그인 응답을 만들 때 **최근 30초 안에 갱신된 서버만** 내려준다.

```csharp
// 하트비트 주기 10초의 3배를 넘기면 죽은 것으로 본다
DateTime aliveSince = DateTime.UtcNow.AddSeconds(-ServerAliveTimeoutSeconds);
foreach (ServerDb serverDb in _shared.Servers.Where(s => s.LastPingTime >= aliveSince))
```

부수적으로 두 가지를 더 고쳤다.

- `System.Timers.Timer` 는 **Interval 이 지난 뒤에야 처음 발동**한다. 그래서 서버를 켜고
  10초 안에 로그인하면 목록이 비어 있었다 → 기동 직후 한 번 즉시 기록하도록 했다.
- 이 타이머 콜백은 예외를 조용히 삼킨다. DB 가 없어도 서버는 `Listening...` 을 찍고
  멀쩡히 떠 있는 것처럼 보였다 → `try/catch` 로 감싸 실패를 콘솔에 남긴다.

**검증**

| 상황 | 기대 | 결과 |
|---|---|---|
| 빈 이름/비밀번호로 생성 | 거부 | `{"CreateOk":false}` ✅ |
| 게임 서버 꺼진 상태로 로그인 | 목록 비어야 함 | `"ServerList":[]` ✅ |
| 게임 서버 켠 직후 로그인 | 즉시 나타남 | `[{"Name":"엘리니아",...}]` ✅ |
| 게임 서버 종료 35초 후 | 목록에서 빠짐 | `"ServerList":[]` ✅ |

> 마지막 항목이 중요하다. 이제 클라이언트는 **죽은 서버를 아예 보지 못하므로**
> `ConnectionRefused` 자체가 발생하지 않는다.

---

## 5. 저장소 위생

### 4-1. `.gitignore` (신규)

원본에는 `Client/.gitignore` (Unity 표준 템플릿) 만 있었고 **저장소 루트에는 없었다.**
그 결과 서버 쪽이 전혀 보호되지 않아 빌드산출물 **458개 파일**이 그대로 커밋되어 있었다
(`Server/*/bin/`, `Server/*/obj/` — DLL, PDB, deps.json 등).

빌드산출물을 커밋하면:

- 리뷰어가 `git diff` 에서 실제 코드 변경을 찾을 수 없다
- 저장소가 불필요하게 무겁다
- 머지 충돌이 상시 발생한다

→ 458개를 `git rm --cached` 로 추적 해제하고 루트 `.gitignore` 를 추가했다.
기존 `Client/.gitignore` 는 그대로 두고, 루트 파일은 **서버와 저장소 전역만** 담당하도록 역할을 분리했다.

주의할 점 하나 — 흔한 Unity `.gitignore` 템플릿은 `*.csproj` / `*.sln` 을 통째로 무시한다.
이 저장소는 **서버 솔루션이 손으로 관리되는 소스**이므로 그대로 쓰면 안 된다.
따라서 Unity 가 자동 생성하는 `Client/*.csproj` / `Client/*.sln` 만 경로를 한정해 무시하도록 했다.

### 4-2. `.gitattributes` (신규)

Unity 프로젝트에서 특히 중요한데 원본에 없었다.

- **줄바꿈 정규화** — 없으면 Windows/Mac 협업 시 파일 전체가 변경된 것처럼 보인다
- **`merge=unityyamlmerge`** — 씬·프리팹 충돌을 Unity SmartMerge 로 해결
- **바이너리 명시** — PNG/PSD/TTF/DLL 을 Git 이 텍스트로 오판해 손상시키는 것을 방지

---

## 6. 변경하지 않은 것 (의도적)

| 항목 | 이유 |
|---|---|
| 게임 로직 · 밸런스 · 리소스 | 현대화의 범위가 아니다 |
| Legacy Input Manager | Unity 6 에서 여전히 지원. 신규 Input System 전환은 별도 과제 |
| `Startup.cs` 방식 (AccountServer) | .NET 8 에서 정상 동작. Minimal API 전환은 기능 이득이 없다 |
| Built-in Render Pipeline | 2D 프로젝트이고 URP 전환은 전체 머티리얼 재작업을 수반 |
| `Nullable` 참조 타입 | 켜면 수백 개 경고. 점진 도입이 맞다 |

---

## 7. 다음 단계 제안

1. **GitHub Actions** — `dotnet build` + `CIBuild.BuildWindows` 를 CI 로 걸면 "빌드되는 프로젝트"가 배지로 증명된다
2. **Docker Compose** — SQL Server + AccountServer + GameServer 를 한 번에 띄우면 리뷰어가 5분 안에 실행해볼 수 있다
3. **README 보강** — 플레이 GIF, 아키텍처 다이어그램, 실행 방법
4. **Input System 전환** — 게임패드 지원이 필요해지면

---

## 부록 — 재현 방법

```bash
# 서버
cd Server
dotnet restore Server.sln
dotnet build   Server.sln -c Debug

# DB 구축 (3개)
cd Sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -i 00_CreateDatabases.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d DungeonLadersDB -i 01_DungeonLadersDB.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d SharedDB        -i 02_SharedDB.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -d AccountDB       -i 03_AccountDB.sql

# 서버 실행 (인스턴스별)
cd GameServer/bin/Debug/Elenia/Build && ./GameServer.exe

# Unity 임포트 + 스크립트 컴파일 검증
"<Unity>/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath Client -logFile import.log

# Unity 플레이어 빌드
"<Unity>/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath Client -executeMethod CIBuild.BuildWindows -logFile build.log
```

**검증 환경** — Windows 11 Pro 26200 · Unity 6000.2.10f1 · .NET SDK 9.0.306 · SQL Server LocalDB (MSSQLLocalDB)
