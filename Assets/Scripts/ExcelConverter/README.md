# Excel & CSV Converter Module

Excel 및 CSV 파일을 Unity ScriptableObject로 변환하는 범용 모듈입니다.
ExcelDataReader와 CSV 파서를 사용하여 리플렉션 기반으로 동작합니다.

## 📁 폴더 구조

```
Assets/
├── Scripts/
│   └── ExcelConverter/
│       ├── Attributes/              # Attribute 정의
│       │   ├── ExcelIdAttribute.cs
│       │   ├── ExcelIgnoreAttribute.cs
│       │   └── ExcelColumnAttribute.cs
│       ├── Core/                    # 핵심 로직
│       │   ├── ExcelDataConverter.cs      # Excel 변환
│       │   ├── ExcelSheetReader.cs        # Excel 리더
│       │   ├── CsvSheetReader.cs          # CSV 리더 (신규)
│       │   ├── TypeConverter.cs
│       │   ├── ExcelFileInfo.cs
│       │   └── FieldMapping.cs
│       ├── Editor/                  # Editor 툴
│       │   ├── ExcelConverterWindow.cs    # Excel 변환 툴
│       │   ├── CsvConverterWindow.cs      # CSV 변환 툴 (신규)
│       │   └── GameDatabaseEditor.cs      # GameDatabase 관리 (신규)
│       └── Data/                    # 데이터 클래스 (신규)
│           ├── GameDataScriptableObjects.cs  # 모든 SO 클래스
│           └── GameDatabase.cs              # 메인 Database
└── GameData/                        # 변환된 데이터 저장 위치
    ├── Characters/
    ├── Stats/
    ├── Events/
    ├── Quests/
    ├── Items/
    ├── Equipment/
    ├── Locations/
    ├── Actions/
    ├── Endings/
    └── NPCs/
```

## 🆕 신규 기능: CSV to ScriptableObject

GameProject의 CSV 데이터를 Unity의 ScriptableObject로 변환하여 관리할 수 있습니다.

### 주요 기능

1. **CSV 파싱**: GameProject/Data 폴더의 CSV 파일을 자동으로 파싱
2. **ScriptableObject 생성**: 각 데이터 행을 개별 ScriptableObject로 변환
3. **GameDatabase 관리**: 모든 ScriptableObject를 한 곳에서 관리
4. **빠른 접근**: Dictionary 캐시를 통한 런타임 고성능 데이터 접근

---

## 📦 필수 패키지 설치

### Excel 변환용 (기존)
ExcelDataReader 패키지를 설치해야 합니다:

**방법 1: NuGet for Unity 사용 (권장)**
1. Unity Asset Store 또는 GitHub에서 NuGet for Unity 설치
2. Unity 메뉴: `NuGet` → `Manage NuGet Packages`
3. 검색: `ExcelDataReader`
4. `ExcelDataReader` 및 `ExcelDataReader.DataSet` 설치

**방법 2: DLL 직접 다운로드**
1. https://www.nuget.org/packages/ExcelDataReader/ 접속
2. "Download package" 클릭
3. `.nupkg` 파일을 `.zip`로 변경 후 압축 해제
4. `lib/netstandard2.0/ExcelDataReader.dll`을 `Assets/Plugins/ExcelDataReader/`에 복사

### CSV 변환용
추가 패키지 설치가 필요 없습니다. C# 기본 라이브러리만 사용합니다.

---

## 🚀 사용 방법

### 1. CSV 파일 변환 (신규)

1. Unity 메뉴에서 **Tools → CSV Converter** 선택
2. CSV 폴더 경로 확인 (기본값: `C:/우수민/Maker/GameProject/01_GameDesign/Data/`)
3. 변환할 파일들이 체크되어 있는지 확인:
   - Characters.csv → CharacterData
   - Events.csv → EventData
   - Quests.csv → QuestData
   - Actions.csv → ActionData
   - etc.
4. **"폴더 생성"** 버튼으로 출력 폴더 먼저 생성
5. **"변환 실행"** 버튼 클릭
6. 변환된 ScriptableObject 파일들이 `Assets/GameData/` 하위에 생성됨

### 2. GameDatabase 생성 및 설정 (신규)

1. Unity 메뉴에서 **Tools → Game Database** 선택
2. **"Create New"** 버튼으로 새 Database 생성
   - 또는 **"Find Existing"**으로 기존 Database 찾기
3. **"Auto Load All"** 버튼으로 모든 ScriptableObject를 Database에 연결
4. **"Initialize Cache"** 버튼으로 런타임 캐시 초기화

### 3. 런타임에서 데이터 사용 (신규)

```csharp
using GameData.ScriptableObjects;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameDatabase gameDatabase;
    
    private void Start()
    {
        // 캐시 초기화 (한 번만 실행)
        gameDatabase.InitializeCache();
        
        // 데이터 접근 예시
        var lua = gameDatabase.GetCharacter("Char_Lua");
        Debug.Log($"캐릭터 이름: {lua.DisplayName}");
        
        var healthStat = gameDatabase.GetStat("HP");
        Debug.Log($"스탯 설명: {healthStat.Description}");
        
        // 특정 조건으로 검색
        var ageEvents = gameDatabase.GetEventsByAge(10);
        foreach (var evt in ageEvents)
        {
            Debug.Log($"이벤트: {evt.NameKO}");
        }
        
        // NPC 퀘스트 검색
        var inoQuests = gameDatabase.GetQuestsByNPC("Char_Ino");
    }
}
```

---

### 4. Excel 파일 변환 (기존)

```csharp
using System;
using ExcelConverter.Attributes;

[Serializable]
public class MyData
{
    [ExcelId]  // 반드시 하나의 필드에 적용 (PK 역할)
    public string ID;
    
    public string Name;        // 자동 매핑: Excel 헤더 "Name"
    public int Value;          // 자동 매핑: Excel 헤더 "Value"
    
    [ExcelColumn("Description Text")]  // 컬럼명이 다를 때
    public string Description;
    
    [ExcelIgnore]  // 변환에서 제외
    public float RuntimeValue;
}
```

1. Unity 메뉴: `Tools` → `Excel Converter`
2. 경로 설정 후 변환 실행

---

## 📋 지원하는 데이터 타입

| CSV 파일 | ScriptableObject | 설명 |
|---------|-----------------|------|
| Characters.csv | CharacterData | 캐릭터 기본 정보 |
| Character_Stats.csv | StatData | 스탯 정의 |
| Events.csv | EventData | 게임 이벤트 |
| Quests.csv | QuestData | 퀘스트 정보 |
| Items_Master.csv | ItemData | 아이템 정보 |
| Equipment.csv | EquipmentData | 장비 정보 |
| Locations.csv | LocationData | 장소/위치 |
| Actions.csv | ActionData | 활동/액션 |
| Ending_Conditions.csv | EndingData | 엔딩 조건 |
| NPC_Favor.csv | NPCFavorData | NPC 호감도 레벨 |
| NPC_Dialogues_KO.csv | NPCDialogueData | NPC 대사 |

---

## 📋 Attribute 설명

- `[ExcelId]`: ID(PK) 필드 지정 (필수)
- `[ExcelIgnore]`: 변환에서 제외
- `[ExcelColumn("name")]`: 명시적 매핑

---

## 🔧 지원 타입

- `string`, `int`, `float`, `double`, `long`
- `bool` (true/false, 1/0, yes/no, y/n)
- `Enum` (문자열 기반, 대소문자 무시)

---

## 📝 CSV 규칙

1. 첫 번째 행: 헤더 (필드명과 동일하거나 `[ExcelColumn]`로 매핑)
2. `#`로 시작하는 행: 주석 (무시됨)
3. 빈 행: 무시됨
4. 따옴표(`"`)로 감싼 값: 쉼표 포함 가능

---

## ⚠️ 주의사항

1. `[ExcelId]`가 없는 클래스는 변환되지 않습니다
2. Excel 파일이 열려있으면 읽기가 실패할 수 있습니다
3. CSV 파일은 UTF-8 인코딩 권장
4. 같은 ID를 가진 데이터는 마지막 것으로 덮어써짐
5. ScriptableObject는 에디터에서만 수정 가능 (런타임 수정 불가)

---

## 🔍 문제 해결

### CSV 변환이 안 될 때
- CSV 파일 경로가 올바른지 확인
- CSV 파일 인코딩이 UTF-8인지 확인
- CSV 헤더가 데이터 클래스의 `ExcelColumn`과 일치하는지 확인
- `#` 주석 행이 있는지 확인

### GameDatabase가 비어있을 때
- Auto Load All 버튼을 눌렀는지 확인
- ScriptableObject 파일들이 올바른 폴더에 있는지 확인
- Initialize Cache를 실행했는지 확인

---

## 🎮 GameDatabase API 예시

### 단일 데이터 조회
```csharp
var character = gameDatabase.GetCharacter("Char_Lua");
var stat = gameDatabase.GetStat("HP");
var evt = gameDatabase.GetEvent("EVT_PROLOGUE");
var quest = gameDatabase.GetQuest("Q_001");
```

### 조건 검색
```csharp
// 나이별 이벤트
var events = gameDatabase.GetEventsByAge(10);

// NPC 퀘스트
var quests = gameDatabase.GetQuestsByNPC("Char_Ino");

// 타입별 아이템
var items = gameDatabase.GetItemsByType("Consumable");
```

---

## 📝 변경 로그

### v2.0 (신규)
- CSV to ScriptableObject 변환 기능 추가
- GameDatabase 및 관리 툴 추가
- RPG 게임 데이터 타입 지원 (Character, Event, Quest, etc.)
- 런타임 캐시 시스템 추가

### v1.0 (기존)
- Excel to ScriptableObject 변환 기능
- Attribute 기반 리플렉션 매핑
