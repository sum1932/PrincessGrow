# 헤드리스 POCO 구현 완료 보고서

## 프로젝트 개요

**패턴**: 어댑터 패턴 (Adapter Pattern)  
**아키텍처**: 헤드리스 (Headless) + POCO (Plain Old C# Object)  
**목표**: Unity 의존성 없이 게임 로직 구현, 시뮬레이션 가능

---

## 구현된 파일 구조

```
GameProject/
├── 02_Technical/
│   └── 01_Architecture/
│       └── Implementation_Plan.md      # 구현 계획 문서
│
└── Implementation/
    ├── Core/                           # 순수 C# 게임 로직
    │   ├── Domain/                     # 도메인 모델 (POCO)
    │   │   ├── Enums.cs                # 열거형 정의
    │   │   ├── GameTurn.cs             # 턴/달력 시스템
    │   │   ├── CharacterStats.cs       # 캐릭터 스탯
    │   │   ├── Economy.cs              # 경제 시스템
    │   │   ├── Activity.cs             # 활동 정의
    │   │   ├── MonthlySchedule.cs      # 일정 시스템
    │   │   ├── NPCRelationship.cs      # NPC 관계
    │   │   ├── GameEvent.cs            # 이벤트 시스템
    │   │   ├── EndingCondition.cs      # 엔딩 조건
    │   │   └── GameState.cs            # 게임 상태
    │   │
    │   ├── Services/                   # 서비스 레이어
    │   │   ├── TurnManager.cs          # 턴 관리
    │   │   ├── EventManager.cs         # 이벤트 관리
    │   │   └── EndingJudge.cs          # 엔딩 판정
    │   │
    │   └── Data/                       # 데이터 레이어
    │       └── IRepositories.cs        # 저장소 인터페이스
    │
    ├── Adapters/                       # Unity 연결
    │   ├── Interfaces/
    │   │   ├── IGameInput.cs           # 입력 인터페이스
    │   │   └── IGamePresenter.cs       # 출력 인터페이스
    │   │
    │   └── Unity/
    │       └── UnityAdapters.cs        # Unity 구현
    │
    └── Simulation/
        └── GameSimulator.cs            # 시뮬레이션 엔진
```

---

## 핵심 구성 요소

### 1. 도메인 모델 (POCO)

| 클래스 | 설명 |
|--------|------|
| `GameTurn` | 6세~18세, 144턴 관리 |
| `CharacterStats` | 6개 스탯 + 성격 성향 |
| `Economy` | 자산 관리 (스위트) |
| `Activity` | 수업/아르바이트/휴식/외출 |
| `MonthlySchedule` | 월간 일정 2개 관리 |
| `NPCRelationship` | 호감도 0~100 |
| `GameEvent` | 이벤트 트리거 시스템 |
| `EndingCondition` | 엔딩 조건 + 우선순위 |
| `GameState` | 전체 게임 상태 |

### 2. 서비스 레이어

| 클래스 | 설명 |
|--------|------|
| `TurnManager` | 턴 진행, 이벤트 체크, 효과 적용 |
| `EventManager` | 이벤트 발동 조건 체크 |
| `EndingJudge` | 우선순위 기반 엔딩 판정 |

### 3. 어댑터 패턴

```
┌─────────────────────────────────────────┐
│              UNITY (View)               │
│  UnityGameInput ← 사용자 입력           │
│  UnityGamePresenter → 화면 표시         │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│            ADAPTER (Interface)          │
│  IGameInput (입력)                      │
│  IGamePresenter (출력)                  │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│              CORE (Logic)               │
│  TurnManager, EventManager, etc.        │
└─────────────────────────────────────────┘
```

### 4. 시뮬레이션 엔진

| 클래스 | 설명 |
|--------|------|
| `GameSimulator` | Unity 없이 게임 진행 |
| `ISimulationStrategy` | 전략 패턴 인터페이스 |
| `RandomStrategy` | 랜덤 선택 전략 |

---

## 아키텍처 특징

### 장점
1. **Unity 독립적**: 순수 C#으로 구현, Unity 의존성 없음
2. **테스트 용이**: 헤드리스로 단위 테스트 가능
3. **확장성**: 어댑터 패턴으로 View 교체 가능
4. **시뮬레이션**: 대량 플레이 테스트 가능

### 연결 방법
```csharp
// Unity GameController
public class GameController : MonoBehaviour
{
    private ITurnManager _turnManager;
    private IGamePresenter _presenter;
    private IGameInput _input;
    
    void Start()
    {
        // Core 초기화
        var state = new GameState();
        var eventManager = new EventManager(events);
        _turnManager = new TurnManager(state, eventManager);
        
        // Unity 어댑터 연결
        _presenter = GetComponent<UnityGamePresenter>();
        _input = GetComponent<UnityGameInput>();
        
        // 이벤트 연결
        _input.OnActivitySelected += HandleActivitySelected;
        _turnManager.OnTurnProcessed += HandleTurnProcessed;
    }
}
```

---

## 다음 단계

### 1. 데이터 파일 작성
- `Activities.json` - 활동 데이터
- `Events.json` - 이벤트 데이터
- `Endings.json` - 엔딩 조건

### 2. Repository 구현
- `JsonActivityRepository`
- `JsonEventRepository`
- `JsonEndingRepository`

### 3. Unity UI 연결
- `TurnView` 구현
- `ScheduleView` 구현
- `EventView` 구현
- `StatsView` 구현

### 4. 테스트
- 단위 테스트 작성
- 통합 테스트
- 시뮬레이션 테스트

---

## 사용 예시

```csharp
// 시뮬레이션 예시
var state = new GameState();
var eventManager = new EventManager(new List<GameEvent>());
var endingJudge = new EndingJudge(new List<EndingCondition>());
var turnManager = new TurnManager(state, eventManager);

var simulator = new GameSimulator(turnManager, eventManager, endingJudge, state);
simulator.SetStrategy(new RandomStrategy(activities));

var result = simulator.RunFullSimulation();
Console.WriteLine($"엔딩: {result.Ending.Name}");
```

---

*구현 완료일: 2026-03-26*
*버전: 1.0*
