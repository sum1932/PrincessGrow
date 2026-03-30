# 다음 세션 작업 가이드

## 문서 개요

**작성일**: 2026-03-30  
**대상**: Unity UI 프로토타입 완료 후 다음 개발자  
**목적**: Phase 4 이후 작업 방향성 및 참고 문서 안내

---

## 현재 완료 상태 (Phase 1-3)

### ✅ Phase 1: Core Domain (완료)
- 6개 기본 스탯 시스템
- 144턴 달력 시스템 (6세→18세)
- 활동/이벤트/엔딩 POCO 모델

### ✅ Phase 2: 어댑터 구현 (완료)
- `IGamePresenter` / `IGameInput` 인터페이스
- `UnityGamePresenter` Unity 이벤트 연결
- `UnityGameInput` 버튼 입력 처리
- `GameController` MonoBehaviour 연결

### ✅ Phase 3: CSV 데이터 로드 (완료)
- `CsvParser` 유틸리티 (따옴표/쉼표 처리)
- `CsvActivityRepository` - 활동 36개 로드
- `CsvEventRepository` - 이벤트 데이터
- `CsvEndingRepository` - 엔딩 15개 로드
- `Resources/Data/` 폴더에 CSV 파일 배치

---

## 다음 세션 작업: Phase 4 - UI 프로토타입

### 작업 목표
Unity 씬에 기본 UI 배치 후 GameController와 연동하여 동작 확인

### 작업 단계

#### Step 1: 씬 기본 설정 (30분)
```
1. MainScene.unity 생성
2. Canvas 설정 (Screen Space - Overlay, 1920x1080)
3. EventSystem 추가 (버튼 클릭용)
```

#### Step 2: UI 오브젝트 배치 (1시간)
```
필수 UI 요소:
├── Canvas
│   ├── TopPanel (턴/스탯 정보)
│   │   ├── TurnText (6세 1월)
│   │   ├── MoneyText (500 스위트)
│   │   └── StatsPanel (6개 슬라이더)
│   ├── ActivityPanel (활동 선택)
│   │   └── ScrollView + ActivityButton 프리팹
│   ├── BottomPanel (컨트롤 버튼)
│   │   ├── SaveBtn
│   │   ├── LoadBtn
│   │   ├── EndTurnBtn
│   │   └── NewGameBtn
│   └── MessagePanel (팝업 메시지)
```

#### Step 3: View 스크립트 작성 (1시간)
```
생성할 파일:
- Assets/Scripts/Views/TurnView.cs
- Assets/Scripts/Views/StatsView.cs
- Assets/Scripts/Views/ActivityPanelView.cs
- Assets/Scripts/Views/MessagePanelView.cs
```

#### Step 4: GamePresenter 연결 (30분)
```
UnityGamePresenter.cs 수정:
- View 참조 추가
- Unity Events → View 메서드 호출
- Inspector에 View 오브젝트 연결
```

#### Step 5: 테스트 및 디버깅 (30분)
```
테스트 항목:
□ CSV 데이터 로드 확인 (Console 로그)
□ 활동 버튼 표시 확인 (36개)
□ 버튼 클릭 시 선택 이벤트 발생
□ 턴 종료 시 스탯 변경 확인
□ 저장/로드 동작 확인
```

---

## 참고해야 할 문서

### 1. 기획 문서 (필수)
| 문서 | 경로 | 용도 |
|------|------|------|
| **System Overview** | `01_GameDesign/03_GameSystems/SystemOverview.md` | 전체 시스템 구조 이해 |
| **Calendar System** | `01_GameDesign/03_GameSystems/Calendar_TurnSystem.md` | 턴 진행 순서 |
| **Character Growth** | `01_GameDesign/03_GameSystems/CharacterGrowth.md` | 스탯 계산 공식 |
| **Available Actions** | `01_GameDesign/04_Content/Schedules/AvailableActions.md` | 활동 효과 값 |
| **Event System** | `01_GameDesign/03_GameSystems/EventSystem.md` | 이벤트 트리거 조건 |
| **Ending Conditions** | `01_GameDesign/03_GameSystems/MultiEndingSystem.md` | 엔딩 달성 조건 |

### 2. 데이터 스키마
| 문서 | 경로 | 용도 |
|------|------|------|
| **Data Schema** | `01_GameDesign/DataSchema.md` | CSV 컬럼 정의 |
| **Actions.csv** | `01_GameDesign/Data/Actions.csv` | 활동 데이터 |
| **Events.csv** | `01_GameDesign/Data/Events.csv` | 이벤트 데이터 |
| **Ending_Conditions.csv** | `01_GameDesign/Data/Ending_Conditions.csv` | 엔딩 데이터 |

### 3. 구현 문서 (이번 세션에서 생성)
| 문서 | 경로 | 용도 |
|------|------|------|
| **UI Setup Guide** | `Implementation/Docs/UI_Setup_Guide.md` | UI 배치 단계별 가이드 |
| **README** | `Implementation/README.md` | 전체 아키텍처 개요 |

### 4. 코드 참조
| 파일 | 경로 | 역할 |
|------|------|------|
| **GameController** | `Assets/Scripts/Controllers/GameController.cs` | 메인 컨트롤러 |
| **UnityGamePresenter** | `Assets/Scripts/Adapters/Unity/UnityGamePresenter.cs` | View 연결 |
| **UnityGameInput** | `Assets/Scripts/Adapters/Unity/UnityGameInput.cs` | 입력 처리 |
| **TurnManager** | `Assets/Scripts/Core/Services/TurnManager.cs` | 턴 진행 로직 |

---

## 주의사항 및 팁

### ⚠️ 주의사항

1. **Unity Event 네임스페이스 충돌**
   ```csharp
   using EventType = DessertKingdom.Core.Domain.EventType;  // 꼭 추가!
   ```

2. **TextMeshPro 설정**
   - 모든 텍스트는 TextMeshPro 사용 (Legacy Text 사용 금지)
   - 폰트 Asset 생성 필요 (Window → TextMeshPro → Import Essential Resources)

3. **CSV 인코딩**
   - UTF-8 BOM으로 저장되어야 한글 깨짐 방지
   - Excel에서 "CSV UTF-8" 형식으로 저장

4. **Scene 저장**
   - 작업 중 Ctrl+S 자주 저장
   - UI 배치 변경 시 Scene 저장 필수

### 💡 개발 팁

1. **빠른 테스트**
   ```csharp
   // GameController에 임시 버튼 연결
   void Update() {
       if (Input.GetKeyDown(KeyCode.Space)) {
           EndTurn();  // 스페이스바로 턴 종료 테스트
       }
   }
   ```

2. **Debug.Log 활용**
   ```csharp
   // CSV 로드 확인
   Debug.Log($"활동 {_activities.Count}개 로드 완료");
   
   // 버튼 클릭 확인
   Debug.Log($"활동 선택됨: {activity.Name}");
   ```

3. **Inspector 설정 단축키**
   - GameObject 선택 → Inspector에서 컴포넌트 드래그
   - UnityGamePresenter의 View 필드에 View 오브젝트 연결

---

## 완료 기준 (Definition of Done)

Phase 4 완료 시 확인 사항:

### 기능적 완료
- [ ] 36개 활동 버튼이 CSV에서 로드되어 표시됨
- [ ] 버튼 클릭 시 Console에 선택 로그 출력
- [ ] "턴 종료" 버튼 클릭 시 턴 진행 (6세 1월 → 6세 2월)
- [ ] 스탯 슬라이더 값이 실제 데이터와 동기화
- [ ] 저장/로드 버튼이 PlayerPrefs에 저장/복원

### 비주능적 완료
- [ ] 모든 UI 요소가 1920x1080 해상도에서 정상 표시
- [ ] 버튼 클릭 시 시각적 피드백 (색상 변경)
- [ ] ScrollView가 10개 이상 버튼에서도 스크롤 가능

---

## Phase 5 예고 (UI 테스트 후)

Phase 4 완료 후 진행할 작업:

### Phase 5: 이벤트 시스템 연동
- 이벤트 트리거 조건 체크
- 이벤트 발생 시 모달 팝업
- 선택지 버튼 및 결과 적용

### Phase 6: 엔딩 시스템
- 144턴 도달 시 엔딩 판정
- 엔딩 CG 표시
- 엔딩 도감 (Collection)

### Phase 7: Polish
- 애니메이션 추가 (DOTween)
- 사운드 연결
- 저장 슬롯 UI (3개)

---

## 연락처 및 참고

**프로젝트 위치**: `C:\우수민\Maker\GameProject`  
**Unity 프로젝트**: `C:\우수민\Maker\Maker`  
**Git 저장소**: (해당되면 작성)

**핵심 키워드**:
- 헤드리스 아키텍처
- 어댑터 패턴
- POCO 도메인 모델
- CSV 데이터 드리븐

---

*이 문서는 Phase 4 작업 시작 전에 반드시 읽고 이해해야 합니다.*
