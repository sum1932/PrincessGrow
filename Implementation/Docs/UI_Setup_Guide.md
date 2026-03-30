# Unity UI 배치 및 테스트 가이드

## 문서 개요

**목적**: GameController와 연동되는 기본 UI 배치 및 테스트 방법
**타겟**: Unity 2022.3 LTS + uGUI
**완료 시 확인**: 버튼 클릭 → Core 로직 실행 → UI 업데이트

---

## 1. 씬 설정

### 1.1 새 씬 생성
```
1. File → New Scene
2. 저장: Assets/Scenes/MainScene.unity
3. Hierarchy에서 Main Camera 설정:
   - Projection: Orthographic
   - Size: 5
   - Position: (0, 0, -10)
```

### 1.2 Canvas 생성
```
Hierarchy 우클릭 → UI → Canvas
```

**Canvas 설정:**
| 속성 | 값 | 설명 |
|------|-----|------|
| Render Mode | Screen Space - Overlay | 기본 2D UI |
| Canvas Scaler | Scale With Screen Size | 반응형 UI |
| Reference Resolution | 1920 x 1080 | 기준 해상도 |
| Screen Match Mode | Match Width Or Height | 0.5 |

---

## 2. UI 오브젝트 배치

### 2.1 상단 정보 패널 (Turn/Stats)

**패널 생성:**
```
Canvas 우클릭 → UI → Panel (이름: TopPanel)
```

**RectTransform 설정:**
```
TopPanel:
  Anchors: Min(0, 1), Max(1, 1)  // 상단 전체
  Pivot: (0.5, 1)
  Position: (0, 0, 0)
  Size: (0, 150)  // 높이 150
```

**자식 오브젝트:**

| 오브젝트 | 타입 | 위치 | 설명 |
|---------|------|------|------|
| TurnText | TextMeshPro | 좌측 상단 | "6세 1월 (턴 0/144)" |
| AgeText | TextMeshPro | 좌측 상단 아래 | "Chapter 1" |
| MoneyText | TextMeshPro | 우측 상단 | "500 스위트" |

**스탯 바들:**
```
TopPanel 우클릭 → UI → Panel (이름: StatsPanel)
  Anchors: Min(0, 0), Max(1, 0)
  Pivot: (0.5, 0)
  Position: (0, -50, 0)
  Size: (0, 100)
```

StatsPanel에 6개 슬라이더 배치:
- HP_Slider, Charm_Slider, Int_Slider, Art_Slider, Moral_Slider, Stress_Slider
- 각 슬라이더 Min: 0, Max: 999
- Fill Color:
  - HP: 빨강 (#FF4444)
  - Charm: 핑크 (#FF88CC)
  - Int: 파랑 (#4488FF)
  - Art: 보라 (#AA66FF)
  - Moral: 초록 (#44CC88)
  - Stress: 회색 (#888888)

### 2.2 중앙 활동 선택 패널

**패널 생성:**
```
Canvas 우클릭 → UI → Panel (이름: ActivityPanel)
```

**RectTransform:**
```
Anchors: Min(0.1, 0.2), Max(0.9, 0.8)
Pivot: (0.5, 0.5)
```

**스크롤 뷰:**
```
ActivityPanel 우클릭 → UI → Scroll View
  Content 크기: 자동 확장 (Vertical Layout Group)
```

**버튼 프리팹:**
```
ActivityPanel/Content 우클릭 → UI → Button (이름: ActivityButton)
```

**ActivityButton 구조:**
```
ActivityButton (Button)
├── Background (Image)
├── Icon (Image) - 왼쪽
├── NameText (TextMeshPro) - 중앙
├── CostText (TextMeshPro) - 우측 하단
└── EffectPreview (TextMeshPro) - 하단 작게
```

**버튼 텍스트 예시:**
```
NameText: "체력 단련 - 기본"
CostText: "200 스위트"
EffectPreview: "체력+42, 매력-5"
```

### 2.3 하단 컨트롤 패널

**패널 생성:**
```
Canvas 우클릭 → UI → Panel (이름: BottomPanel)
```

**RectTransform:**
```
Anchors: Min(0, 0), Max(1, 0)
Pivot: (0.5, 0)
Size: (0, 120)
```

**버튼들:**

| 버튼 이름 | 텍스트 | 위치 | 기능 |
|-----------|--------|------|------|
| SaveBtn | 저장 | 좌측 | CommandType.Save |
| LoadBtn | 불러오기 | 좌측 | CommandType.Load |
| EndTurnBtn | 턴 종료 | 중앙 | TurnManager.EndTurn() |
| NewGameBtn | 새 게임 | 우측 | CommandType.NewGame |

### 2.4 메시지 패널 (팝업)

**패널 생성:**
```
Canvas 우클릭 → UI → Panel (이름: MessagePanel)
```

**초기 상태:**
```
Inspector에서 체크 해제: Enabled (비활성화)
```

**구조:**
```
MessagePanel
├── Background (Image - 반투명 검정)
└── MessageText (TextMeshPro)
    - Font Size: 24
    - Alignment: Center
    - Color: White
```

---

## 3. 스크립트 연결

### 3.1 View 스크립트 생성

**TurnView.cs** (`Assets/Scripts/Views/TurnView.cs`):
```csharp
using UnityEngine;
using TMPro;
using DessertKingdom.Adapters.Interfaces;

namespace DessertKingdom.Views
{
    public class TurnView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI chapterText;
        
        public void UpdateTurn(TurnDisplayData data)
        {
            turnText.text = $"{data.Age}세 {data.Month}월";
            ageText.text = $"턴 {data.Turn}/144";
            chapterText.text = $"Chapter {data.Chapter}";
        }
    }
}
```

**StatsView.cs**:
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DessertKingdom.Adapters.Interfaces;

namespace DessertKingdom.Views
{
    public class StatsView : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider charmSlider;
        [SerializeField] private Slider intSlider;
        [SerializeField] private Slider artSlider;
        [SerializeField] private Slider moralSlider;
        [SerializeField] private Slider stressSlider;
        
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI charmText;
        // ... 나머지 텍스트
        
        public void UpdateStats(StatsDisplayData data)
        {
            hpSlider.value = data.HP;
            hpText.text = $"체력: {data.HP}";
            
            charmSlider.value = data.Charm;
            charmText.text = $"매력: {data.Charm}";
            
            // ... 나머지 스탯
        }
    }
}
```

**ActivityButtonView.cs**:
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    public class ActivityButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI effectText;
        
        private Activity _activity;
        private int _slot;
        
        public void SetActivity(Activity activity, int slot)
        {
            _activity = activity;
            _slot = slot;
            
            nameText.text = activity.Name;
            costText.text = $"{activity.Cost} 스위트";
            
            // 효과 문자열 생성
            string effects = "";
            foreach (var effect in activity.StatEffects)
            {
                effects += $"{effect.Key}+{effect.Value}, ";
            }
            effectText.text = effects.TrimEnd(',', ' ');
            
            // 버튼 클릭 이벤트
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClick());
        }
        
        private void OnClick()
        {
            // GameController에 선택 이벤트 발생
            var input = FindObjectOfType<UnityGameInput>();
            if (input != null)
            {
                input.SelectActivity(_activity, _slot);
            }
        }
    }
}
```

### 3.2 UnityGamePresenter에 View 연결

**UnityGamePresenter.cs 수정:**
```csharp
using UnityEngine;
using DessertKingdom.Views;  // 추가

namespace DessertKingdom.Adapters.Unity
{
    public class UnityGamePresenter : MonoBehaviour, IGamePresenter
    {
        [SerializeField] private TurnView turnView;
        [SerializeField] private StatsView statsView;
        [SerializeField] private ActivityPanelView activityPanelView;  // 새로 생성
        [SerializeField] private MessagePanelView messagePanelView;   // 새로 생성
        
        public void DisplayTurn(GameTurn turn)
        {
            var data = new TurnDisplayData { ... };
            turnView?.UpdateTurn(data);
        }
        
        public void DisplayStats(CharacterStats stats)
        {
            var data = new StatsDisplayData { ... };
            statsView?.UpdateStats(data);
        }
        
        public void DisplayScheduleSelection()
        {
            activityPanelView?.Show();
            var activities = GameController.Instance.GetAvailableActivities();
            activityPanelView?.SetActivities(activities);
        }
        
        public void ShowMessage(string message)
        {
            messagePanelView?.Show(message);
        }
        
        // ... 나머지 메서드
    }
}
```

### 3.3 Inspector 설정

**GameController 오브젝트:**
```
Hierarchy에서 GameController 선택
└── Inspector:
    ├── Game Presenter: UnityGamePresenter (드래그)
    ├── Game Input: UnityGameInput (드래그)
    └── Use Csv Data: 체크
```

**UnityGamePresenter 컴포넌트:**
```
UnityGamePresenter:
  Turn View: [TurnView 오브젝트 드래그]
  Stats View: [StatsView 오브젝트 드래그]
  Activity Panel View: [ActivityPanel 오브젝트 드래그]
  Message Panel View: [MessagePanel 오브젝트 드래그]
```

---

## 4. 테스트 시나리오

### 테스트 1: 게임 시작
```
예상 결과:
- 진 시작 시 "6세 1월 (턴 0/144)" 표시
- 스탯 바들이 초기값 (50)으로 설정
- 활동 버튼들이 CSV에서 로드되어 표시
```

### 테스트 2: 활동 선택
```
단계:
1. "체력 단련" 버튼 클릭
2. "카페 알바" 버튼 클릭
3. "턴 종료" 버튼 클릭

예상 결과:
- Console에 "턴 종료" 로그
- 스탯 변경 (체력 증가, 자산 변화)
- 다음 턴으로 진행 ("6세 2월")
```

### 테스트 3: 저장/로드
```
단계:
1. 턴 진행 (턴 3까지)
2. "저장" 버튼 클릭
3. 씬 재시작
4. "불러오기" 버튼 클릭

예상 결과:
- 턴 3 상태 복원
- 스탯 값 복원
```

### 테스트 4: CSV 데이터 로드
```
확인 사항:
- Console에 "활동 36개 로드 완료" 출력
- Actions.csv의 모든 활동이 버튼으로 표시
```

---

## 5. 문제 해결 체크리스트

| 문제 | 원인 | 해결 방법 |
|------|------|----------|
| 버튼 클릭 안 됨 | Raycaster 없음 | Canvas에 Graphic Raycaster 추가 |
| 텍스트 깨짐 | 폰트 미설정 | TextMeshPro 폰트 할당 |
| CSV 로드 실패 | 경로 문제 | Resources/Data/ 폴더 확인 |
| 슬라이더 안 움직임 | MaxValue 미설정 | Slider Max Value = 999 설정 |
| 이벤트 안 받음 | GameController 싱글톤 | Instance null 체크 |

---

## 6. 다음 단계 진행을 위한 준비

UI 테스트 완료 후 확인 사항:
- [ ] 모든 버튼 클릭 시 Console 로그 출력
- [ ] 턴 진행 시 날짜 변경 확인
- [ ] 스탯 슬라이더 값 변경 확인
- [ ] CSV 데이터 36개 활동 로드 확인
- [ ] 저장/로드 기능 동작 확인

완료되면 다음 문서 참고하여 진행.

---

*작성일: 2026-03-30*
*버전: 1.0*
