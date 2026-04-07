# Database ScriptableObject 사용 가이드

## 개요

GameProject의 CSV 데이터를 Unity Database ScriptableObject로 관리하는 방법입니다.

- **파일 개수**: CSV 파일 수만큼 (11개)
- **관리 방식**: 각 데이터 타입별 하나의 Database 파일
- **런타임 접근**: Dictionary 캐싱으로 O(1) 접근

## 파일 구조

```
Assets/GameData/
├── CharacterDatabase.asset    (List<CharacterData>)
├── StatDatabase.asset         (List<StatData>)
├── EventDatabase.asset        (List<EventData>)
├── QuestDatabase.asset        (List<QuestData>)
├── ItemDatabase.asset         (List<ItemData>)
├── EquipmentDatabase.asset    (List<EquipmentData>)
├── LocationDatabase.asset     (List<LocationData>)
├── ActionDatabase.asset       (List<ActionData>)
├── EndingDatabase.asset       (List<EndingData>)
├── NPCDialogueDatabase.asset  (List<NPCDialogueData>)
└── NPCFavorDatabase.asset     (List<NPCFavorData>)
```

## CSV 변환 방법

### 1. CSV Converter 실행

1. Unity 메뉴: `Tools → CSV Converter`
2. CSV 폴더 경로 확인 (기본값: `C:/우수민/Maker/GameProject/01_GameDesign/Data/`)
3. 출력 폴더 확인 (기본값: `Assets/GameData/`)
4. **"모두 변환"** 버튼 클릭

### 2. 변환 결과

- 각 CSV 파일 → Database ScriptableObject 생성
- 예: `Characters.csv` → `CharacterDatabase.asset`

### 3. 증분 업데이트

변경된 CSV만 다시 변환하려면:
- CSV Converter에서 해당 항목만 체크
- 또는 전체 변환 (이미 존재하면 업데이트)

## 런타임 사용법

### 1. Database 참조 설정

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDB;
    [SerializeField] private EventDatabase eventDB;
    [SerializeField] private QuestDatabase questDB;
    [SerializeField] private ItemDatabase itemDB;
    [SerializeField] private EquipmentDatabase equipmentDB;
    [SerializeField] private LocationDatabase locationDB;
    [SerializeField] private ActionDatabase actionDB;
    [SerializeField] private EndingDatabase endingDB;
    [SerializeField] private NPCDialogueDatabase dialogueDB;
    [SerializeField] private NPCFavorDatabase favorDB;
    [SerializeField] private StatDatabase statDB;
    
    private void Awake()
    {
        // 모든 Database 캐시 초기화
        characterDB.Initialize();
        eventDB.Initialize();
        questDB.Initialize();
        itemDB.Initialize();
        equipmentDB.Initialize();
        locationDB.Initialize();
        actionDB.Initialize();
        endingDB.Initialize();
        dialogueDB.Initialize();
        favorDB.Initialize();
        statDB.Initialize();
    }
}
```

### 2. 데이터 접근

```csharp
// 단일 데이터 조회
var lua = characterDB.Get("Char_Lua");
var healthStat = statDB.Get("HP");
var event = eventDB.Get("EVT_PROLOGUE");

// 모든 데이터 조회
var allCharacters = characterDB.GetAll();
var allEvents = eventDB.GetAll();

// 조건 검색
var age10Events = eventDB.GetByAge(10);
var romanceableNpcs = characterDB.GetRomanceable();
var itemsByType = itemDB.GetByType("Consumable");
```

## Database 클래스 API

### CharacterDatabase

```csharp
CharacterData Get(string characterId)
List<CharacterData> GetAll()
List<CharacterData> GetByRole(string role)
List<CharacterData> GetRomanceable()
```

### EventDatabase

```csharp
EventData Get(string eventId)
List<EventData> GetAll()
List<EventData> GetByAge(int age)
List<EventData> GetByDate(int month, int day)
List<EventData> GetByType(string type)
List<EventData> GetByTrigger(string triggerType)
```

### QuestDatabase

```csharp
QuestData Get(string questId)
List<QuestData> GetAll()
List<QuestData> GetByNPC(string npcId)
List<QuestData> GetByAge(int age)
```

### ItemDatabase

```csharp
ItemData Get(string itemId)
List<ItemData> GetAll()
List<ItemData> GetByType(string itemType)
List<ItemData> GetConsumables()
```

### EquipmentDatabase

```csharp
EquipmentData Get(string equipmentId)
List<EquipmentData> GetAll()
List<EquipmentData> GetBySlot(string slot)
```

### ActionDatabase

```csharp
ActionData Get(string actionId)
List<ActionData> GetAll()
List<ActionData> GetByCategory(string category)
```

### EndingDatabase

```csharp
EndingData Get(string endingId)
List<EndingData> GetAll()
List<EndingData> GetByType(string endingType)
List<EndingData> GetAchievableEndings(int hp, int charm, int int, int art, int morality, int stress)
```

### NPCDialogueDatabase

```csharp
NPCDialogueData Get(string dialogueId)
List<NPCDialogueData> GetAll()
List<NPCDialogueData> GetByNPC(string npcId)
List<NPCDialogueData> GetByFavorLevel(string favorLevel)
```

### NPCFavorDatabase

```csharp
NPCFavorData Get(string npcId)
List<NPCFavorData> GetAll()
int GetFavorLevel(string npcId, int currentFavor)
int GetRequiredFavorForNextLevel(string npcId, int currentFavor)
```

### LocationDatabase

```csharp
LocationData Get(string locationId)
List<LocationData> GetAll()
```

### StatDatabase

```csharp
StatData Get(string statId)
List<StatData> GetAll()
List<StatData> GetPrimaryStats()
Dictionary<string, int> GetInitialValues()
```

## 실전 예제

### 예제 1: 이벤트 시스템

```csharp
public class EventManager : MonoBehaviour
{
    [SerializeField] private EventDatabase eventDB;
    
    void Start()
    {
        eventDB.Initialize();
    }
    
    public List<EventData> GetAvailableEvents(int age, int month, int day)
    {
        // 나이와 날짜로 이벤트 검색
        var ageEvents = eventDB.GetByAge(age);
        var dateEvents = eventDB.GetByDate(month, day);
        
        // 두 조건을 모두 만족하는 이벤트
        return ageEvents.Intersect(dateEvents).ToList();
    }
}
```

### 예제 2: 스탯 시스템

```csharp
public class CharacterStats : MonoBehaviour
{
    [SerializeField] private StatDatabase statDB;
    [SerializeField] private ActionDatabase actionDB;
    
    private Dictionary<string, int> currentStats = new();
    
    void Start()
    {
        statDB.Initialize();
        actionDB.Initialize();
        
        // 초기 스탯 설정
        currentStats = statDB.GetInitialValues();
    }
    
    public void PerformAction(string actionId)
    {
        var action = actionDB.Get(actionId);
        if (action == null) return;
        
        currentStats["HP"] += action.HPChange;
        currentStats["CHARM"] += action.CharmChange;
        // ...
    }
}
```

### 예제 3: 엔딩 판정

```csharp
public class EndingJudge : MonoBehaviour
{
    [SerializeField] private EndingDatabase endingDB;
    
    void Start()
    {
        endingDB.Initialize();
    }
    
    public EndingData GetEnding(int hp, int charm, int intel, int art, int morality, int stress)
    {
        var achievable = endingDB.GetAchievableEndings(hp, charm, intel, art, morality, stress);
        return achievable.FirstOrDefault();
    }
}
```

### 예제 4: NPC 호감도

```csharp
public class NPCManager : MonoBehaviour
{
    [SerializeField] private NPCFavorDatabase favorDB;
    [SerializeField] private NPCDialogueDatabase dialogueDB;
    
    private Dictionary<string, int> npcFavors = new();
    
    void Start()
    {
        favorDB.Initialize();
        dialogueDB.Initialize();
    }
    
    public string GetDialogue(string npcId)
    {
        int currentFavor = npcFavors.GetValueOrDefault(npcId, 0);
        int level = favorDB.GetFavorLevel(npcId, currentFavor);
        
        var dialogues = dialogueDB.GetByNPC(npcId)
                                  .Where(d => d.FavorLevel == $"Level_{level}")
                                  .ToList();
        
        if (dialogues.Count == 0) return "...";
        return dialogues[Random.Range(0, dialogues.Count)].DialogueTextKO;
    }
}
```

## 성능 팁

### 1. 캐시 초기화는 한 번만

```csharp
void Awake()
{
    // 한 번만 호출
    database.Initialize();
}
```

### 2. GetAll() 결과 캐싱

```csharp
private List<EventData> allEvents;

void Start()
{
    allEvents = eventDB.GetAll();
}

void Update()
{
    // allEvents 사용 (매번 GetAll() 호출하지 않음)
}
```

### 3. LINQ 쿼리 결과 캐싱

```csharp
// 나쁜 예: 매 프레임 LINQ
void Update()
{
    var items = itemDB.GetAll().Where(i => i.Type == "Weapon"); // GC 발생
}

// 좋은 예: 미리 필터링
private List<ItemData> weapons;

void Start()
{
    weapons = itemDB.GetAll().Where(i => i.Type == "Weapon").ToList();
}
```

## 데이터 업데이트 워크플로우

1. **CSV 파일 수정** (Excel 등으로)
2. **Unity에서**: Tools → CSV Converter → "모두 변환"
3. **Unity에서**: 변경된 Database 파일 확인
4. **테스트**: Play 모드에서 데이터 확인
5. **Git**: Database 파일들 커밋

## 주의사항

1. **런타임 수정 금지**: Database는 ScriptableObject이므로 런타임에 수정해도 저장되지 않습니다. 게임 진행 데이터는 별도로 관리하세요.

2. **Initialize 호출 필수**: Database 사용 전 반드시 `Initialize()`를 호출해야 합니다.

3. **ID 대소문자**: `Get()` 메서드는 ID가 정확히 일치해야 합니다.

4. **Null 체크**: `Get()`은 찾지 못하면 null을 반환하므로 항상 null 체크를 하세요.

## 문제 해결

### Q: Database가 null이에요
A: Inspector에서 Database 필드에 ScriptableObject를 연결했는지 확인하세요.

### Q: Get()이 null을 반환해요
A: ID가 정확한지 확인하세요. 대소문자가 일치해야 합니다.

### Q: 데이터가 업데이트되지 않았어요
A: CSV 변환 후 Unity에서 Refresh(Ctrl+R)를 눌러보세요.

### Q: Initialize()를 깜빡했어요
A: 캐시가 초기화되지 않으면 Get()이 null을 반환할 수 있습니다. 반드시 Awake나 Start에서 Initialize()를 호출하세요.
