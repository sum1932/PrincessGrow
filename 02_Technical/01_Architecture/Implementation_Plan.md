# 헤드리스 POCO 아키텍처 구현 계획서

## 프로젝트 개요

### 목표
- **헤드리스(Headless)**: 핵심 게임 로직은 순수 C#으로 구현, Unity 의존성 제거
- **POCO**: Plain Old C# Object 기반 도메인 모델
- **어댑터 패턴**: Unity는 View(표현)만 담당, 핵심 로직은 독립적으로 관리
- **시뮬레이션 가능**: Unity 없이도 게임 진행 시뮬레이션 및 테스트 가능

---

## 프로젝트 구조

```
GameProject/
├── 01_GameDesign/           # 기획 문서 (기존)
├── 02_Technical/
│   ├── 01_Architecture/     # 아키텍처 문서
│   ├── 03_CoreSystems/      # 핵심 시스템 (C#)
│   └── UnityIntegration/    # Unity 연결 (어댑터)
└── Implementation/          # 실제 구현 폴더
    ├── Core/               # 순수 C# 게임 로직
    │   ├── Domain/         # 도메인 모델 (POCO)
    │   ├── Services/       # 서비스 레이어
    │   ├── Data/           # 데이터 관리
    │   └── Simulation/     # 시뮬레이션 엔진
    ├── Adapters/           # Unity 연결 어댑터
    │   ├── Unity/          # Unity MonoBehaviour 어댑터
    │   └── ViewModels/     # ViewModel for Unity
    └── Tests/              # 단위 테스트
        ├── Core/           # 순수 로직 테스트
        └── Integration/    # 통합 테스트
```

---

## 아키텍처 다이어그램

```
┌─────────────────────────────────────────────────────────────┐
│                     UNITY (PRESENTATION)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │    Views     │  │  Presenters  │  │  Controllers │       │
│  │  (UI, CG)    │  │  (ViewModel) │  │  (Input)     │       │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
└─────────┼─────────────────┼─────────────────┼───────────────┘
          │                 │                 │
          └─────────────────┼─────────────────┘
                            │
                    ┌───────▼────────┐
                    │    ADAPTER     │
                    │   (Interface)  │
                    └───────┬────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                    CORE (BUSINESS LOGIC)                    │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                 DOMAIN (POCO)                        │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐   │   │
│  │  │  Turn   │ │ Stats   │ │ Economy │ │ Events  │   │   │
│  │  │ Calendar│ │Character│ │ Money   │ │ Trigger │   │   │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              SERVICES                                │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐   │   │
│  │  │Turn     │ │Growth   │ │Event    │ │Ending   │   │   │
│  │  │Manager  │ │Service  │ │Manager  │ │Judge    │   │   │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              DATA ACCESS                             │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐   │   │
│  │  │Activity │ │Event    │ │Ending   │ │NPC      │   │   │
│  │  │Repository│ │Repository│ │Repository│ │Repository│   │   │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 구현 단계

### Phase 1: 도메인 모델 설계 (POCO)

#### 1.1 핵심 엔티티 정의

**Turn/Calendar 도메인**
```csharp
// 순수 C#, Unity 의존성 없음
public class GameTurn
{
    public int CurrentTurn { get; private set; }      // 0~144
    public int CurrentAge { get; private set; }       // 6~18
    public int CurrentMonth { get; private set; }     // 1~12
    public int CurrentChapter { get; private set; }   // 1~3
    public Season CurrentSeason { get; private set; }
    
    // 비즈니스 로직만 포함
    public void AdvanceTurn();
    public bool IsLastTurn();
    public int GetRemainingTurns();
}

public enum Season { Spring, Summer, Autumn, Winter }
```

**Character 도메인**
```csharp
public class CharacterStats
{
    // 6개 기본 스탯
    public int HP { get; private set; }
    public int Charm { get; private set; }
    public int Intelligence { get; private set; }
    public int Art { get; private set; }
    public int Morality { get; private set; }
    public int Stress { get; private set; }
    
    // 스탯 등급 계산
    public StatGrade GetGrade(StatType type);
    public PersonalityType GetPersonality();
    
    // 스탯 변경 메서드 (유효성 검증 포함)
    public Result<StatChange> ModifyStat(StatType type, int amount);
}
```

**Economy 도메인**
```csharp
public class Economy
{
    public int CurrentMoney { get; private set; }
    public int MonthlyIncome { get; private set; }
    public int MonthlyExpense { get; private set; }
    
    public bool CanAfford(int amount);
    public Result<Transaction> Spend(int amount, string reason);
    public void Earn(int amount, string reason);
}
```

**Schedule/Activity 도메인**
```csharp
public class Activity
{
    public ActivityId Id { get; }
    public string Name { get; }
    public ActivityType Type { get; }
    public int Cost { get; }
    public StatEffects StatEffects { get; }
    public int StressChange { get; }
    public int MinAge { get; }
    public int? RequiredStat { get; }
}

public class MonthlySchedule
{
    public Activity Activity1 { get; private set; }
    public Activity Activity2 { get; private set; }
    
    public bool CanSetActivity(Activity activity, int slot);
    public void ApplyEffects(CharacterStats stats, Economy economy);
}
```

**Event 도메인**
```csharp
public class GameEvent
{
    public EventId Id { get; }
    public EventType Type { get; }
    public EventTrigger Trigger { get; }
    public List<Choice> Choices { get; }
    public bool CanTrigger(GameState state);
}

public class EventTrigger
{
    public TriggerType Type { get; }
    public Dictionary<string, object> Conditions { get; }
    public bool Evaluate(GameState state);
}
```

**NPC 도메인**
```csharp
public class NPCRelationship
{
    public NPCId Id { get; }
    public int Favorability { get; private set; }
    public RelationshipLevel Level { get; private set; }
    
    public void ChangeFavorability(int amount);
    public bool CanTriggerEvent(int requiredLevel);
}
```

**Ending 도메인**
```csharp
public class EndingCondition
{
    public EndingId Id { get; }
    public int Priority { get; }
    public Dictionary<StatType, int> RequiredStats { get; }
    public Dictionary<NPCId, int> RequiredFavor { get; }
    public List<string> RequiredEvents { get; }
    
    public bool IsMet(GameState finalState);
}
```

#### 1.2 게임 상태 관리

```csharp
public class GameState
{
    public GameTurn Turn { get; }
    public CharacterStats Character { get; }
    public Economy Economy { get; }
    public List<NPCRelationship> NPCs { get; }
    public MonthlySchedule CurrentSchedule { get; }
    public List<string> CompletedEvents { get; }
    public List<string> ChoiceHistory { get; }
    public GamePhase Phase { get; private set; }
    
    // 세이브/로드용 직렬화
    public string Serialize();
    public static GameState Deserialize(string data);
}

public enum GamePhase
{
    Prologue,
    ScheduleSelection,
    ActivityProcessing,
    EventCheck,
    EventProcessing,
    TurnEnd,
    Ending
}
```

---

### Phase 2: 서비스 레이어 구현

#### 2.1 턴 관리 서비스

```csharp
public interface ITurnManager
{
    event Action<TurnStartedEvent> OnTurnStarted;
    event Action<TurnEndedEvent> OnTurnEnded;
    
    Result<TurnResult> StartTurn();
    Result<TurnResult> EndTurn();
    Result<ScheduleResult> SetSchedule(Activity activity1, Activity activity2);
}

public class TurnManager : ITurnManager
{
    private readonly GameState _state;
    private readonly IActivityRepository _activityRepo;
    private readonly IEventRepository _eventRepo;
    
    public Result<TurnResult> EndTurn()
    {
        // 1. 일정 효과 적용
        var scheduleEffects = ApplySchedule();
        
        // 2. 월간 정산
        var monthlyResult = ProcessMonthly();
        
        // 3. 이벤트 체크
        var triggeredEvents = CheckEvents();
        
        // 4. 다음 턴 준비
        _state.Turn.AdvanceTurn();
        
        // 5. 엔딩 체크
        if (_state.Turn.IsLastTurn())
        {
            return Result<TurnResult>.Success(new TurnResult { 
                Phase = GamePhase.Ending 
            });
        }
        
        return Result<TurnResult>.Success(new TurnResult {
            ScheduleEffects = scheduleEffects,
            MonthlyResult = monthlyResult,
            TriggeredEvents = triggeredEvents
        });
    }
}
```

#### 2.2 이벤트 관리 서비스

```csharp
public interface IEventManager
{
    event Action<GameEvent> OnEventTriggered;
    event Action<Choice, ChoiceResult> OnChoiceMade;
    
    List<GameEvent> CheckEvents(GameState state);
    Result<EventResult> ProcessEvent(GameEvent event, string choiceId);
}

public class EventManager : IEventManager
{
    public List<GameEvent> CheckEvents(GameState state)
    {
        var availableEvents = new List<GameEvent>();
        
        // 고정 이벤트 체크
        availableEvents.AddRange(CheckFixedEvents(state));
        
        // 랜덤 이벤트 체크
        availableEvents.AddRange(CheckRandomEvents(state));
        
        // NPC 이벤트 체크
        availableEvents.AddRange(CheckNPCEvents(state));
        
        // 우선순위 정렬
        return availableEvents.OrderByDescending(e => e.Priority).ToList();
    }
}
```

#### 2.3 엔딩 판정 서비스

```csharp
public interface IEndingJudge
{
    EndingCondition EvaluateEnding(GameState finalState);
    List<EndingCondition> GetAvailableEndings();
}

public class EndingJudge : IEndingJudge
{
    private readonly List<EndingCondition> _endings;
    
    public EndingCondition EvaluateEnding(GameState finalState)
    {
        // 우선순위 순서로 체크 (히든 → 기본)
        foreach (var ending in _endings.OrderBy(e => e.Priority))
        {
            if (ending.IsMet(finalState))
                return ending;
        }
        
        // 기본값
        return _endings.First(e => e.Id == "default");
    }
}
```

#### 2.4 성장 계산 서비스

```csharp
public interface IGrowthCalculator
{
    StatChanges CalculateActivityEffects(Activity activity, CharacterStats currentStats);
    int CalculateStressChange(Activity activity1, Activity activity2);
    StatGrowthRate GetGrowthRate(int age);
}
```

---

### Phase 3: 데이터 레이어 구현

#### 3.1 저장소 패턴

```csharp
// 인터페이스 (Core)
public interface IActivityRepository
{
    Activity GetById(ActivityId id);
    List<Activity> GetAvailable(int age, CharacterStats stats);
    List<Activity> GetByType(ActivityType type);
}

public interface IEventRepository
{
    GameEvent GetById(EventId id);
    List<GameEvent> GetAll();
    List<GameEvent> GetByType(EventType type);
}

public interface IEndingRepository
{
    EndingCondition GetById(EndingId id);
    List<EndingCondition> GetAll();
}
```

#### 3.2 JSON 기반 구현 (Unity 독립)

```csharp
// 순수 C# JSON 파싱
public class JsonActivityRepository : IActivityRepository
{
    private readonly Dictionary<ActivityId, Activity> _activities;
    
    public JsonActivityRepository(string jsonData)
    {
        // System.Text.Json 사용 (Unity 의존성 없음)
        _activities = JsonSerializer.Deserialize<List<ActivityData>>(jsonData)
            .ToDictionary(a => new ActivityId(a.Id), a => MapToDomain(a));
    }
}

// 또는 CSV 파싱
public class CsvActivityRepository : IActivityRepository
{
    // CSV 파싱 구현
}
```

---

### Phase 4: Unity 어댑터 구현

#### 4.1 어댑터 패턴 설계

```csharp
// Core에서 정의한 인터페이스
public interface IGamePresenter
{
    void DisplayTurn(TurnDisplayData data);
    void DisplayScheduleSelection(ScheduleSelectionData data);
    void DisplayEvent(EventDisplayData data);
    void DisplayEnding(EndingDisplayData data);
    void UpdateStats(StatsDisplayData data);
    void UpdateEconomy(EconomyDisplayData data);
}

// Unity 구현
public class UnityGamePresenter : MonoBehaviour, IGamePresenter
{
    [SerializeField] private TurnView turnView;
    [SerializeField] private ScheduleView scheduleView;
    [SerializeField] private EventView eventView;
    [SerializeField] private StatsView statsView;
    [SerializeField] private EconomyView economyView;
    [SerializeField] private EndingView endingView;
    
    // Core의 GameState를 Unity UI에 표시
    public void DisplayTurn(TurnDisplayData data)
    {
        turnView.Show(data);
    }
    
    public void DisplayScheduleSelection(ScheduleSelectionData data)
    {
        scheduleView.Show(data);
    }
    
    // ... 나머지 메서드
}
```

#### 4.2 입력 어댑터

```csharp
// Core에서 정의한 인터페이스
public interface IGameInput
{
    event Action<ActivitySelectedEvent> OnActivitySelected;
    event Action<ChoiceMadeEvent> OnChoiceMade;
    event Action<CommandEvent> OnCommand;
}

// Unity 입력 처리
public class UnityGameInput : MonoBehaviour, IGameInput
{
    // UI 버튼 클릭 → Core 이벤트 발행
    public void OnActivityButtonClicked(ActivityId id, int slot)
    {
        OnActivitySelected?.Invoke(new ActivitySelectedEvent(id, slot));
    }
    
    public void OnChoiceButtonClicked(string choiceId)
    {
        OnChoiceMade?.Invoke(new ChoiceMadeEvent(choiceId));
    }
}
```

#### 4.3 게임 컨트롤러 (Unity)

```csharp
public class GameController : MonoBehaviour
{
    // Core 의존성 (Unity 독립)
    private ITurnManager _turnManager;
    private IEventManager _eventManager;
    private IEndingJudge _endingJudge;
    private GameState _gameState;
    
    // Unity 어댑터
    private IGamePresenter _presenter;
    private IGameInput _input;
    
    void Start()
    {
        // Core 초기화
        InitializeCore();
        
        // Unity 어댑터 연결
        _presenter = GetComponent<IGamePresenter>();
        _input = GetComponent<IGameInput>();
        
        // 이벤트 구독
        SubscribeToEvents();
        
        // 게임 시작
        StartGame();
    }
    
    private void InitializeCore()
    {
        // 데이터 로드
        var activityRepo = LoadActivityData();
        var eventRepo = LoadEventData();
        var endingRepo = LoadEndingData();
        
        // 서비스 생성
        _turnManager = new TurnManager(_gameState, activityRepo, eventRepo);
        _eventManager = new EventManager(eventRepo);
        _endingJudge = new EndingJudge(endingRepo);
    }
}
```

---

### Phase 5: 시뮬레이션 엔진

#### 5.1 헤드리스 시뮬레이션

```csharp
// Unity 없이 실행 가능한 시뮬레이터
public class GameSimulator
{
    private readonly ITurnManager _turnManager;
    private readonly IEventManager _eventManager;
    private readonly IEndingJudge _endingJudge;
    private readonly GameState _state;
    
    public SimulationResult RunFullSimulation(SimulationStrategy strategy)
    {
        var results = new List<TurnResult>();
        
        while (!_state.Turn.IsLastTurn())
        {
            // 자동 전략으로 일정 선택
            var schedule = strategy.SelectActivities(_state);
            _turnManager.SetSchedule(schedule.Activity1, schedule.Activity2);
            
            // 턴 진행
            var result = _turnManager.EndTurn();
            results.Add(result);
        }
        
        // 엔딩 판정
        var ending = _endingJudge.EvaluateEnding(_state);
        
        return new SimulationResult
        {
            TurnResults = results,
            FinalState = _state,
            Ending = ending
        };
    }
    
    // 특정 시나리오 테스트
    public SimulationResult RunScenario(List<Activity> activities)
    {
        // 미리 정의된 활동 시퀀스 실행
    }
}

// 시뮬레이션 전략 인터페이스
public interface ISimulationStrategy
{
    (Activity Activity1, Activity Activity2) SelectActivities(GameState state);
    string MakeChoice(GameEvent gameEvent, List<Choice> choices);
}

// 예시: 랜덤 전략
public class RandomStrategy : ISimulationStrategy
{
    private readonly Random _random = new Random();
    
    public (Activity, Activity) SelectActivities(GameState state)
    {
        var available = GetAvailableActivities(state);
        return (available[_random.Next(available.Count)], 
                available[_random.Next(available.Count)]);
    }
}

// 예시: 극단적 육성 전략
public class ExtremeStrategy : ISimulationStrategy
{
    private readonly StatType _targetStat;
    
    public (Activity, Activity) SelectActivities(GameState state)
    {
        // _targetStat을 최대화하는 활동 선택
    }
}
```

#### 5.2 대량 시뮬레이션

```csharp
public class BatchSimulator
{
    public BatchResult RunBatchSimulation(int count, ISimulationStrategy strategy)
    {
        var results = new List<SimulationResult>();
        
        for (int i = 0; i < count; i++)
        {
            var simulator = CreateNewSimulator();
            results.Add(simulator.RunFullSimulation(strategy));
        }
        
        return AnalyzeResults(results);
    }
    
    private BatchResult AnalyzeResults(List<SimulationResult> results)
    {
        // 통계 분석
        return new BatchResult
        {
            EndingDistribution = results.GroupBy(r => r.Ending.Id)
                .ToDictionary(g => g.Key, g => g.Count() / (double)results.Count),
            AverageStats = CalculateAverageStats(results),
            AverageMoney = results.Average(r => r.FinalState.Economy.CurrentMoney)
        };
    }
}
```

---

## 데이터 파일 구조

### CSV/JSON 데이터 파일

```
Implementation/Core/Data/
├── Activities.json         # 활동 데이터
├── Events/
│   ├── FixedEvents.json    # 고정 이벤트
│   ├── RandomEvents.json   # 랜덤 이벤트
│   ├── NPCEvents.json      # NPC 이벤트
│   └── HiddenEvents.json   # 히든 이벤트
├── NPCs.json              # NPC 데이터
├── Endings.json           # 엔딩 조건
└── Items.json             # 아이템 데이터
```

### 예시: Activities.json

```json
{
  "activities": [
    {
      "id": "ACT_HP_TRAINING",
      "name": "체력 단련",
      "type": "Lesson",
      "cost": 200,
      "minAge": 6,
      "effects": {
        "hp": 15,
        "charm": -3
      },
      "stressChange": 10,
      "description": "체력을 단련하는 수업"
    },
    {
      "id": "ACT_CONVENIENCE",
      "name": "편의점 아르바이트",
      "type": "PartTime",
      "income": 150,
      "minAge": 10,
      "effects": {
        "intelligence": 5,
        "morality": 2
      },
      "stressChange": 5
    }
  ]
}
```

---

## 인터페이스 계약

### Core → Unity (Presenter)

| 메서드 | 파라미터 | 설명 |
|--------|----------|------|
| `DisplayTurn` | TurnDisplayData | 현재 턴 정보 표시 |
| `DisplaySchedule` | ScheduleSelectionData | 일정 선택 화면 |
| `DisplayEvent` | EventDisplayData | 이벤트 씬 재생 |
| `DisplayEnding` | EndingDisplayData | 엔딩 씬 재생 |
| `UpdateStats` | StatsDisplayData | 스탯 UI 갱신 |
| `UpdateEconomy` | EconomyDisplayData | 자산 UI 갱신 |

### Unity → Core (Input)

| 이벤트 | 파라미터 | 설명 |
|--------|----------|------|
| `OnActivitySelected` | ActivitySelectedEvent | 활동 선택 |
| `OnChoiceMade` | ChoiceMadeEvent | 선택지 선택 |
| `OnCommand` | CommandEvent | 명령 (저장/로드 등) |

---

## 구현 체크리스트

### Phase 1: 도메인 모델
- [ ] Turn/Calendar 엔티티
- [ ] CharacterStats 엔티티
- [ ] Economy 엔티티
- [ ] Activity/Schedule 엔티티
- [ ] Event 엔티티
- [ ] NPCRelationship 엔티티
- [ ] GameState 직렬화

### Phase 2: 서비스 레이어
- [ ] TurnManager 구현
- [ ] EventManager 구현
- [ ] EndingJudge 구현
- [ ] GrowthCalculator 구현

### Phase 3: 데이터 레이어
- [ ] IActivityRepository 인터페이스
- [ ] IEventRepository 인터페이스
- [ ] JSON 저장소 구현
- [ ] CSV 저장소 구현

### Phase 4: Unity 어댑터
- [ ] IGamePresenter 인터페이스
- [ ] IGameInput 인터페이스
- [ ] UnityGamePresenter 구현
- [ ] UnityGameInput 구현
- [ ] GameController 구현

### Phase 5: 시뮬레이션
- [ ] GameSimulator 구현
- [ ] ISimulationStrategy 인터페이스
- [ ] 랜덤/극단적 전략 구현
- [ ] BatchSimulator 구현

### Phase 6: 테스트
- [ ] 단위 테스트 (Core)
- [ ] 통합 테스트
- [ ] 시뮬레이션 테스트

---

## 기술 스택

### Core 프로젝트
- **언어**: C# 10.0+
- **프레임워크**: .NET Standard 2.1 (Unity 호환)
- **JSON**: System.Text.Json
- **테스트**: xUnit

### Unity 프로젝트
- **버전**: Unity 2022.3 LTS
- **스크립팅**: C# 10.0
- **UI**: uGUI 또는 UI Toolkit

---

## 다음 단계

1. **패턴 선택 확인**: 어댑터 패턴 구조에 대한 사용자 확인
2. **도메인 모델 구현**: 핵심 POCO 클래스 작성
3. **데이터 파일 생성**: CSV/JSON 데이터 작성
4. **서비스 레이어 구현**: 비즈니스 로직 작성
5. **Unity 어댑터 구현**: View 연결
6. **시뮬레이터 구현**: 헤드리스 테스트

---

*작성일: 2026-03-26*
*버전: 1.0*
