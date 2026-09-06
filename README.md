# Dessert Princess

2D 비주얼 노벨 감성의 육성 시뮬레이션 게임 프로젝트입니다. Unity 화면/입력 계층과 순수 C# Core 로직을 분리하고, 랜덤/LLM 전략 기반 자동 플레이 시뮬레이션으로 월 진행, 이벤트, 엔딩, 경제 밸런스를 검증할 수 있도록 구성했습니다.

![Dessert Princess title](docs/assets/title.jpg)

## 프로젝트 개요

- 장르: 2D, 비주얼 노벨, 육성 시뮬레이션, 캐주얼
- 개발 기간: 2026.03.27 - 2026.05.31
- 엔진: Unity 6 (`6000.2.15f1`)
- 언어: C#
- 구조: Headless POCO, Clean Architecture, Ports & Adapters
- 데이터: CSV / JSON 기반 콘텐츠 파이프라인
- 시뮬레이션: 랜덤 전략 및 LLM 플레이어 전략

## 담당 역할

- 월 진행, 스탯, 경제, 이벤트, 인벤토리, 엔딩 판정을 담당하는 Core 도메인 설계
- 시뮬레이션과 테스트를 위한 Unity 독립 C# Core 구현
- 게임 규칙을 UI 코드에 넣지 않고 Core 상태를 표현하는 Unity Adapter/View 계층 구현
- 활동, 이벤트, 엔딩, NPC 대화, 아이템 데이터를 위한 CSV/JSON Repository 계층 구현
- 밸런스 확인과 QA 탐색을 위한 자동 플레이 시뮬레이션 구현
- AI 기반 플레이 테스트를 위한 LLM 전략 연동

## 주요 화면

![Main menu](docs/assets/main-menu.png)

![Schedule selection](docs/assets/schedule.png)

![Monthly result](docs/assets/monthly-result.png)

## 기술 포인트

### Unity 독립 Core 구조

핵심 게임 규칙은 순수 C# 도메인 모델과 서비스로 구현했습니다. 같은 게임 로직을 Unity 안에서도, 별도 콘솔 시뮬레이션에서도 실행할 수 있습니다.

주요 파일:

- `Assets/Scripts/Core/Domain/GameState.cs`
- `Assets/Scripts/Core/Services/TurnManager.cs`
- `Assets/Scripts/Core/Services/EventManager.cs`
- `Assets/Scripts/Core/Services/EndingJudge.cs`

### Ports & Adapters 구조

Unity UI는 `IGamePresenter`, `IGameInput` 같은 인터페이스에 의존하고, Core 로직은 Unity 씬, 프리팹, 입력 장치를 알지 못하도록 분리했습니다.

주요 파일:

- `Assets/Scripts/Adapters/Interfaces/IGamePresenter.cs`
- `Assets/Scripts/Adapters/Interfaces/IGameInput.cs`
- `Assets/Scripts/Adapters/Unity/UnityGamePresenter.cs`
- `Assets/Scripts/Controllers/GameController.cs`

### 데이터 기반 콘텐츠

활동, 이벤트, 엔딩, NPC 대화, 아이템 데이터를 CSV/JSON으로 관리하고 Repository 구현을 통해 도메인 객체로 변환합니다.

주요 파일:

- `Assets/Scripts/Core/Data/IRepositories.cs`
- `Assets/Scripts/Core/Data/CsvActivityRepository.cs`
- `Assets/Scripts/Repositories/DatabaseActionRepository.cs`
- `Assets/Scripts/ExcelConverter/`

### 자동 시뮬레이션과 LLM 플레이 테스트

Standalone 구현은 교체 가능한 전략을 사용해 전체 게임 시뮬레이션을 반복 실행할 수 있습니다. Unity를 열지 않고도 랜덤 플레이, 사람 기준 플레이, LLM 기반 플레이 흐름을 비교할 수 있습니다.

주요 파일:

- `Implementation/Simulation/GameSimulator.cs`
- `Implementation/TestConsole/LLMStrategy.cs`
- `Implementation/TestConsole/ILLMClient.cs`
- `Implementation/TestConsole/OpenAIClient.cs`
- `Implementation/TestConsole/GeminiClient.cs`
- `Implementation/TestConsole/KimiClient.cs`

## 저장소 구조

```text
Assets/Scripts/
├── Core/               # Unity 독립 게임 로직
├── Adapters/           # 인터페이스와 Unity Adapter 구현
├── Controllers/        # 얇은 MonoBehaviour 조율 계층
├── Views/              # UI 표현 컴포넌트
└── ExcelConverter/     # CSV/Excel 데이터 가져오기 도구

Implementation/
├── Core/               # Standalone .NET Core 로직
├── Simulation/         # 자동 시뮬레이션 실행기
└── TestConsole/        # 인터랙티브 및 LLM 플레이 테스트 콘솔

docs/                   # GitHub Pages 포트폴리오 페이지
```

## 실행 참고

Unity 게임 플레이는 Unity Editor에서 실행하는 것을 기준으로 합니다. Standalone Core와 시뮬레이션 프로젝트는 `Implementation/` 아래에 있습니다.

LLM 클라이언트용 API 키는 저장소에 포함하지 않습니다. `.gitignore`에서 제외되는 로컬 환경 변수나 로컬 설정 파일을 사용합니다.
