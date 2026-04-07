# GameData 사용 가이드 (개발자용)

ScriptableObject로 변환된 게임 데이터를 실제 게임 코드에서 사용하는 방법을 설명합니다.

## 목차

1. [빠른 시작](#빠른-시작)
2. [GameDatabase 설정](#gamedatabase-설정)
3. [데이터 접근 방법](#데이터-접근-방법)
4. [실전 예제](#실전-예제)
5. [성능 팁](#성능-팁)
6. [FAQ](#faq)

---

## 빠른 시작

### 1. GameDatabase 준비

```csharp
// GameManager나 싱글톤에서 GameDatabase 참조
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameDatabase gameDatabase;
    
    private void Awake()
    {
        // 필수: 캐시 초기화
        gameDatabase.InitializeCache();
    }
}
```

### 2. Inspector에서 연결
- GameManager 오브젝트 선택
- Inspector에서 `Game Database` 필드에 `Assets/GameData/GameDatabase.asset` 드래그

---

## GameDatabase 설정

### 방법 A: Scene에 직접 배치 (권장)

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    
    // 다른 스크립트에서 접근
    public static GameDatabase Database { get; private set; }
    
    private void Awake()
    {
        Database = database;
        Database.InitializeCache();
    }
}
```

### 방법 B: Resources 폴더에서 로드

```csharp
public class GameManager : MonoBehaviour
{
    public static GameDatabase Database { get; private set; }
    
    private void Awake()
    {
        // Resources/GameData 폴더에 GameDatabase.asset 복사 필요
        Database = Resources.Load<GameDatabase>("GameData/GameDatabase");
        Database.InitializeCache();
    }
}
```

### 방법 C: Addressables 사용 (고급)

```csharp
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour
{
    public static GameDatabase Database { get; private set; }
    
    private async void Start()
    {
        var handle = Addressables.LoadAssetAsync<GameDatabase>("GameDatabase");
        Database = await handle.Task;
        Database.InitializeCache();
    }
}
```

---

## 데이터 접근 방법

### 기본 조회

```csharp
// ID로 단일 데이터 조회
var character = GameManager.Database.GetCharacter("Char_Lua");
var item = GameManager.Database.GetItem("ITEM_001");
var quest = GameManager.Database.GetQuest("Q_001");
```

### 데이터 속성 접근

```csharp
var character = GameManager.Database.GetCharacter("Char_Lua");

// 기본 정보
Debug.Log($"이름: {character.DisplayName}");
Debug.Log($"한글 이름: {character.NameKO}");
Debug.Log($"영어 이름: {character.NameEN}");

// 설명 (Inspector에서 설정한 TextArea)
Debug.Log($"설명: {character.Description}");

// 나이 범위 파싱 (문자열)
string ageRange = character.Age; // "6-18"
int minAge = int.Parse(ageRange.Split('-')[0]); // 6
int maxAge = int.Parse(ageRange.Split('-')[1]); // 18
```

### 조건으로 검색

```csharp
// 특정 나이에 발생하는 이벤트들
var age10Events = GameManager.Database.GetEventsByAge(10);
foreach (var evt in age10Events)
{
    Debug.Log($"10세 이벤트: {evt.NameKO}");
}

// 특정 NPC 관련 퀘스트
var inoQuests = GameManager.Database.GetQuestsByNPC("Char_Ino");

// 특정 타입의 아이템
var consumables = GameManager.Database.GetItemsByType("Consumable");

// 카테고리별 액션
var studyActions = GameManager.Database.GetActionsByCategory("Study");
```

### List 직접 접근

```csharp
// 모든 캐릭터 순회
foreach (var character in GameManager.Database.Characters)
{
    Debug.Log($"캐릭터: {character.DisplayName}");
}

// LINQ로 복잡한 쿼리
var romanceableNpcs = GameManager.Database.Characters
    .Where(c => c.IsRomanceable && c.Role == "NPC")
    .OrderBy(c => c.FavorMax)
    .ToList();
```

---

## 실전 예제

### 예제 1: 이벤트 시스템

```csharp
public class EventManager : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    
    // 현재 발생 가능한 이벤트 검색
    public List<EventData> GetAvailableEvents(int currentAge, int currentMonth, int currentDay)
    {
        var availableEvents = new List<EventData>();
        
        foreach (var evt in database.Events)
        {
            // 나이 체크
            if (currentAge < evt.AgeMin || currentAge > evt.AgeMax)
                continue;
            
            // 날짜 체크 (월/일이 설정된 경우)
            if (evt.Month > 0 && evt.Day > 0)
            {
                if (currentMonth != evt.Month || currentDay != evt.Day)
                    continue;
            }
            
            // 우선순위순 정렬을 위해 추가
            availableEvents.Add(evt);
        }
        
        // 우선순위 높은 순으로 정렬
        return availableEvents.OrderByDescending(e => e.Priority).ToList();
    }
    
    // 특정 이벤트 발생
    public void TriggerEvent(string eventId)
    {
        var evt = database.GetEvent(eventId);
        if (evt == null)
        {
            Debug.LogError($"이벤트를 찾을 수 없음: {eventId}");
            return;
        }
        
        // BGM 변경
        if (!string.IsNullOrEmpty(evt.BGMId))
        {
            AudioManager.Instance.PlayBGM(evt.BGMId);
        }
        
        // CG 표시
        if (!string.IsNullOrEmpty(evt.CGId))
        {
            CGManager.Instance.ShowCG(evt.CGId);
        }
        
        // 대화 스크립트 실행
        if (!string.IsNullOrEmpty(evt.ScriptPath))
        {
            DialogueManager.Instance.LoadScript(evt.ScriptPath);
        }
        
        Debug.Log($"이벤트 발생: {evt.NameKO}");
    }
}
```

### 예제 2: 퀘스트 시스템

```csharp
public class QuestManager : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    
    private List<QuestData> activeQuests = new List<QuestData>();
    
    // 퀘스트 시작
    public void StartQuest(string questId)
    {
        var quest = database.GetQuest(questId);
        if (quest == null)
        {
            Debug.LogError($"퀘스트를 찾을 수 없음: {questId}");
            return;
        }
        
        activeQuests.Add(quest);
        Debug.Log($"퀘스트 시작: {quest.NameKO}");
        Debug.Log($"설명: {quest.Description}");
    }
    
    // 조건 만족 확인
    public bool CanStartQuest(QuestData quest, int playerAge, int npcFavor)
    {
        // 나이 체크
        if (playerAge < quest.RequiredAge)
            return false;
        
        // 호감도 체크
        if (npcFavor < quest.RequiredFavor)
            return false;
        
        return true;
    }
    
    // 보상 지급
    public void GiveReward(QuestData quest)
    {
        switch (quest.RewardType)
        {
            case "Favor":
                int favorAmount = int.Parse(quest.RewardValue);
                // 호감도 증가 처리
                break;
                
            case "Item":
                var item = database.GetItem(quest.SpecialReward);
                if (item != null)
                {
                    Inventory.Instance.AddItem(item);
                }
                break;
                
            case "Skill":
                // 스킬 획득 처리
                break;
        }
    }
}
```

### 예제 3: 스탯 시스템

```csharp
public class CharacterStats : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    
    private Dictionary<string, int> currentStats = new Dictionary<string, int>();
    
    private void Start()
    {
        // 초기 스탯 설정
        foreach (var statData in database.Stats)
        {
            currentStats[statData.StatId] = statData.InitialValue;
        }
    }
    
    // 액션 실행
    public void PerformAction(string actionId)
    {
        var action = database.GetAction(actionId);
        if (action == null) return;
        
        // 스탯 변경 적용
        currentStats["HP"] = Mathf.Clamp(
            currentStats["HP"] + action.HPChange,
            database.GetStat("HP").MinValue,
            database.GetStat("HP").MaxValue
        );
        
        currentStats["CHARM"] = Mathf.Clamp(
            currentStats["CHARM"] + action.CharmChange,
            database.GetStat("CHARM").MinValue,
            database.GetStat("CHARM").MaxValue
        );
        
        // 스트레스는 특별 처리 (높을수록 나쁨)
        currentStats["STRESS"] = Mathf.Clamp(
            currentStats["STRESS"] + action.StressChange,
            database.GetStat("STRESS").MinValue,
            database.GetStat("STRESS").MaxValue
        );
        
        Debug.Log($"액션 실행: {action.NameKO}");
    }
    
    // 특정 스탯 값 가져오기
    public int GetStatValue(string statId)
    {
        return currentStats.GetValueOrDefault(statId, 0);
    }
}
```

### 예제 4: 엔딩 판정

```csharp
public class EndingJudge : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    
    // 현재 스탯으로 달성 가능한 엔딩 검색
    public List<EndingData> GetAchievableEndings(
        int hp, int charm, int intelligence, int art, int morality, int stress)
    {
        var achievableEndings = new List<EndingData>();
        
        foreach (var ending in database.Endings)
        {
            if (hp >= ending.RequiredHP &&
                charm >= ending.RequiredCharm &&
                intelligence >= ending.RequiredInt &&
                art >= ending.RequiredArt &&
                morality >= ending.RequiredMorality &&
                stress <= ending.MaxStress)
            {
                achievableEndings.Add(ending);
            }
        }
        
        return achievableEndings;
    }
    
    // 최종 엔딩 선택 (우선순위 또는 랜덤)
    public EndingData SelectEnding(int hp, int charm, int intelligence, int art, int morality, int stress)
    {
        var candidates = GetAchievableEndings(hp, charm, intelligence, art, morality, stress);
        
        if (candidates.Count == 0)
        {
            // 기본 엔딩 (노말 엔딩 등)
            return database.GetEnding("Ending_Normal");
        }
        
        // 여러 개 가능하면 타입별 우선순위 또는 랜덤 선택
        // 히든 엔딩 우선
        var hiddenEnding = candidates.FirstOrDefault(e => e.Type == "Hidden");
        if (hiddenEnding != null) return hiddenEnding;
        
        // 로맨스 엔딩
        var romanceEnding = candidates.FirstOrDefault(e => e.Type == "Romance");
        if (romanceEnding != null) return romanceEnding;
        
        // 그 외는 첫 번째
        return candidates[0];
    }
}
```

### 예제 5: NPC 대화 시스템

```csharp
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private string npcId;
    
    private int currentFavor = 0;
    
    // 현재 호감도에 맞는 대사 가져오기
    public string GetDialogue()
    {
        var favorData = database.GetNPCFavor(npcId);
        if (favorData == null) return "...";
        
        // 호감도 레벨 계산
        int level = 0;
        if (currentFavor >= favorData.Threshold4) level = 4;
        else if (currentFavor >= favorData.Threshold3) level = 3;
        else if (currentFavor >= favorData.Threshold2) level = 2;
        else if (currentFavor >= favorData.Threshold1) level = 1;
        
        // 레벨별 대사 선택
        string dialogueText = level switch
        {
            0 => favorData.FavorLevel0,
            1 => favorData.FavorLevel1,
            2 => favorData.FavorLevel2,
            3 => favorData.FavorLevel3,
            4 => favorData.FavorLevel4,
            _ => "..."
        };
        
        return dialogueText;
    }
    
    // 특정 호감도 레벨의 모든 대사 가져오기
    public List<NPCDialogueData> GetDialoguesByFavorLevel(string favorLevel)
    {
        return database.NPCDialogues
            .Where(d => d.NPCId == npcId && d.FavorLevel == favorLevel)
            .ToList();
    }
    
    // 호감도 증가
    public void AddFavor(int amount)
    {
        var npc = database.GetCharacter(npcId);
        if (npc == null) return;
        
        currentFavor = Mathf.Clamp(currentFavor + amount, npc.FavorMin, npc.FavorMax);
        Debug.Log($"{npc.DisplayName} 호감도: {currentFavor}");
    }
}
```

---

## 성능 팁

### 1. 캐시 필수 사용

```csharp
// Awake나 Start에서 한 번만 호출
void Awake()
{
    database.InitializeCache();
}

// 이제 GetCharacter 등의 메서드가 Dictionary로 O(1) 접근
```

### 2. 빈번한 검색은 로컬 캐싱

```csharp
// 나쁜 예: 매 프레임 검색
void Update()
{
    var character = database.GetCharacter("Char_Lua"); // 비효율적
}

// 좋은 예: 한 번만 로드
private CharacterData luaCharacter;

void Start()
{
    luaCharacter = database.GetCharacter("Char_Lua");
}

void Update()
{
    // luaCharacter 사용
}
```

### 3. LINQ 사용 주의

```csharp
// 나쁜 예: Update에서 LINQ
void Update()
{
    var items = database.Items.Where(i => i.Type == "Weapon").ToList(); // 매 프레임 GC!
}

// 좋은 예: 미리 필터링
private List<ItemData> weaponItems;

void Start()
{
    weaponItems = database.Items.Where(i => i.Type == "Weapon").ToList();
}

void Update()
{
    // weaponItems 사용
}
```

### 4. Null 체크

```csharp
var character = database.GetCharacter("Char_Lua");
if (character == null)
{
    Debug.LogError("캐릭터를 찾을 수 없습니다: Char_Lua");
    return;
}

// 안전하게 사용
Debug.Log(character.NameKO);
```

---

## FAQ

### Q: 데이터가 null이 나와요
**A:** 다음을 확인하세요:
1. GameDatabase가 Inspector에 연결되었나요?
2. `InitializeCache()`를 호출했나요?
3. CSV 변환이 제대로 되었나요?
4. ID가 정확히 일치하나요? (대소문자 구분)

### Q: Inspector에서 ScriptableObject가 안 보여요
**A:** 
1. Unity 메뉴: Assets → Refresh
2. GameDatabase.asset 파일이 존재하는지 확인
3. 파일을 Inspector에 드래그앤드롭

### Q: ScriptableObject 수정이 저장 안 돼요
**A:** ScriptableObject는 에디터 전용입니다. 런타임에서 수정해도 저장되지 않습니다. 게임 진행 데이터는 별도로 관리하세요:

```csharp
// 저장 가능한 게임 데이터
[System.Serializable]
public class GameProgress
{
    public int currentHP;
    public int currentStress;
    public Dictionary<string, int> npcFavors = new Dictionary<string, int>();
    public List<string> completedQuests = new List<string>();
}
```

### Q: Addressables 설정은 어떻게 하나요?
**A:**
1. Window → Asset Management → Addressables → Groups
2. GameDatabase.asset을 Default Local Group에 추가
3. Addressable Name을 "GameDatabase"로 설정

### Q: CSV 업데이트 후 적용이 안 돼요
**A:**
1. `Tools → CSV Converter`에서 다시 변환
2. `Tools → Game Database`에서 Auto Load All
3. 변경사항 저장 (Ctrl+S)
4. Scene 재시작 또는 플레이 모드 재진입

### Q: 특정 필드가 변환되지 않았어요
**A:**
1. CSV 헤더와 `ExcelColumn` Attribute가 정확히 일치하는지 확인
2. 대소문자, 공백, 특수문자 확인
3. CSV 파일 인코딩이 UTF-8인지 확인

### Q: 성능이 느려요
**A:**
1. `InitializeCache()` 호출 확인
2. Update에서 데이터 검색하지 않기
3. 빈번한 LINQ 쿼리 캐싱
4. 불필요한 Debug.Log 제거

---

## 데이터 업데이트 워크플로우

CSV 데이터를 업데이트할 때의 권장 작업 순서:

1. **CSV 파일 수정** (Excel 등으로)
2. **Unity에서**: Tools → CSV Converter → 변환 실행
3. **Unity에서**: Tools → Game Database → Auto Load All
4. **Unity에서**: Ctrl+S (저장)
5. **테스트**: Play 모드에서 데이터 확인
6. **Git**: 변경된 ScriptableObject 파일들 커밋

---

## 지원 및 문의

문제가 있거나 추가 기능이 필요하면 개발팀에 문의하세요.
