# 게임 프로젝트 작업 목록

## 프로젝트 개요

**프로젝트명:** 프린세스 메이커 스타일 육성 시뮬레이션 게임  
**장르:** 육성 시뮬레이션, 비주얼 노벨  
**타겟 플랫폼:** PC, 모바일  
**개발 기간:** 2026년 3월 ~

---

## Phase 1: 핵심 콘텐츠 (완료)

### 1.1 게임 디자인 문서
- [x] Document_Tasks.md - 작업 지시서 작성
- [x] WorldSetting.md - 세계관 설정
- [x] CharacterProfiles.md - 캐릭터 프로필
- [x] MainStory.md - 메인 스토리

### 1.2 데이터 스키마 정의
- [x] DataSchema.md - 모든 데이터 테이블 정의
  - Items_Master 테이블
  - Equipment 테이블
  - Events 테이블
  - NPC_Favor 테이블
  - Ending_Conditions 테이블
  - Character_Stats 테이블
  - CG_Master 테이블
  - BGM_Master 테이블

### 1.3 NPC 콘텐츠
- [x] NPC_Dialogues.md - 4명 NPC 대사 스크립트
  - 이노: 20개 이벤트 (완료)
  - 아이린: 16개 이벤트 (완료)
  - 카일: 13개 이벤트 (완료)
  - 리안: 9개 이벤트 (완료)
- [x] NPC_Favor.csv - 호감도 시스템 데이터

### 1.4 이벤트 콘텐츠
- [x] EventScripts.md - 고정 이벤트 스크립트
  - 프로롤로그 (1개)
  - 생일 이벤트 (7개: 6~18세)
  - 연령별 이벤트 (6개)
  - 계절/기념일 (8개)
  - 학교 이벤트 (2개)
  - NPC 이벤트 (3개)
  - 히든 이벤트 (2개)
- [x] Events.csv - 이벤트 데이터 테이블

### 1.5 엔딩 콘텐츠
- [x] EndingStories.md - 15개 엔딩 스토리
  - 기본 엔딩 13개
  - 히든 엔딩 2개
- [x] Ending_Conditions.csv - 엔딩 달성 조건

### 1.6 데이터 파일 통합
- [x] Items_Master.csv - 아이템 마스터 (48개)
- [x] Equipment.csv - 장비 아이템 (12개)
- [x] Character_Stats.csv - 스탯 정의 (6개)
- [x] CG_Master.csv - CG 메타데이터 (50개)
- [x] BGM_Master.csv - BGM 메타데이터 (37개)

### 1.7 Excel 통합 파일
- [x] GameData.xlsx - 모든 데이터 통합
  - 11개 시트 (Items, Equipment, Events, NPC, Endings, Stats, CG, BGM, Dialogues)

### 1.8 다국어 지원 구조
- [x] Localization_Guide.md - 다국어 지원 가이드
- [x] NPC_Dialogues_KO.csv - 한국어 대사 (256행)
- [x] NPC_Dialogues_EN.csv - 영어 대사 샘플
- [x] 언어별 시트 분리 구조

---

## Phase 2: 진행 중 및 예정 작업

### 2.1 NPC 대사 데이터 정리
- [ ] 선택지(Choice) 데이터 파싱 추가
- [ ] 원본 Markdown과 CSV 동기화 확인
- [ ] 누락된 대사 보완
- [ ] 결과(Result) 데이터 정리

### 2.2 랜덤 이벤트 작성
- [ ] 일상 이벤트 (20개)
- [ ] 스트레스 관련 이벤트 (10개)
- [ ] 스탯 관련 이벤트 (10개)
- [ ] 특별 랜덤 이벤트 (10개)

### 2.3 NPC 이벤트 확장
- [ ] 각 NPC별 추가 이벤트 (10개 × 4명 = 40개)
- [ ] 이노 연인 루트 이벤트
- [ ] 리안 히든 루트 이벤트

### 2.4 밸런싱 데이터
- [ ] BalanceSheet.md 작성
  - 스탯 성장 밸런스
  - 경제 밸런스
  - 이벤트 확률
  - 호감도 상승량
  - 엔딩 난이도

### 2.5 히든 콘텐츠
- [x] 히든 엔딩 2개 상세 스토리
- [x] 히든 이벤트 20개
- [x] 퀘스트 이벤트 10개

---

## Phase 3: 기술 구현 (예정)

### 3.1 게임 엔진 설정
- [ ] Unity 프로젝트 생성
- [ ] 게임 매니저 시스템
- [ ] 데이터 로더 개발
- [ ] CSV/Excel 파서

### 3.2 UI/UX 개발
- [ ] 메인 화면 UI
- [ ] 육성 화면 UI
- [ ] 이벤트 화면 UI
- [ ] 엔딩 화면 UI

### 3.3 시스템 구현
- [ ] 턴 진행 시스템
- [ ] 스탯 계산 시스템
- [ ] 호감도 시스템
- [ ] 이벤트 트리거 시스템
- [ ] 엔딩 판정 시스템

### 3.4 다국어 지원
- [ ] 텍스트 로컬라이제이션 시스템
- [ ] 언어 전환 기능
- [ ] 폰트 적용

---

## Phase 4: 아트/사운드 (예정)

### 4.1 CG 아트
- [ ] 캐릭터 스탠딩 CG
- [ ] 이벤트 CG 제작
- [ ] 엔딩 CG 제작
- [ ] 배경 아트

### 4.2 UI 아트
- [ ] UI 디자인
- [ ] 아이콘 제작
- [ ] 버튼/인터페이스 요소

### 4.3 사운드
- [ ] BGM 제작/구입
- [ ] 효과음
- [ ] UI 사운드

---

## Phase 5: 테스트 및 배포 (예정)

### 5.1 테스트
- [ ] 내부 알파 테스트
- [ ] 클로즈드 베타 테스트
- [ ] 밸런싱 조정

### 5.2 최종 작업
- [ ] 버그 수정
- [ ] 최적화
- [ ] 빌드

### 5.3 배포
- [ ] 스토어 등록
- [ ] 마케팅 자료
- [ ] 출시

---

## 파일 구조

```
GameProject/
├── 01_GameDesign/
│   ├── Data/                      # 데이터 파일
│   │   ├── Items_Master.csv
│   │   ├── Equipment.csv
│   │   ├── Events.csv
│   │   ├── NPC_Favor.csv
│   │   ├── Ending_Conditions.csv
│   │   ├── Character_Stats.csv
│   │   ├── CG_Master.csv
│   │   ├── BGM_Master.csv
│   │   ├── NPC_Dialogues_Events.csv
│   │   ├── NPC_Dialogues_KO.csv
│   │   ├── NPC_Dialogues_EN.csv
│   │   └── GameData.xlsx
│   │
│   ├── 02_Narrative/              # 내러티브 문서
│   │   ├── WorldSetting.md
│   │   ├── CharacterProfiles.md
│   │   ├── MainStory.md
│   │   ├── NPC_Dialogues.md
│   │   ├── EventScripts.md
│   │   ├── EndingStories.md
│   │   └── Localization_Guide.md
│   │
│   ├── 04_Content/                # 콘텐츠 문서
│   │   └── Items/
│   │       ├── Consumables.md
│   │       └── CSV_Integration_Guide.md
│   │
│   └── Document_Tasks.md          # 작업 지시서
│
├── 04_WorkDoce/                   # 작업 문서 (현재 위치)
│   └── Work_Summary.md            # 본 문서
│
└── README.md                      # 프로젝트 개요
```

---

## 현재 진행률

| Phase | 상태 | 완료율 |
|-------|------|--------|
| Phase 1: 핵심 콘텐츠 | ✅ 완료 | 100% |
| Phase 2: 확장 콘텐츠 | 🔄 진행 중 | 40% |
| Phase 3: 기술 구현 | ⏳ 예정 | 0% |
| Phase 4: 아트/사운드 | ⏳ 예정 | 0% |
| Phase 5: 테스트/배포 | ⏳ 예정 | 0% |

**전체 진행률: 35%**

---

## 우선순위 작업 (다음 주)

1. **NPC 대사 CSV 완성**
   - 선택지(Choice) 데이터 추가
   - 원본 Markdown과 동기화

2. **랜덤 이벤트 작성**
   - 일상 이벤트 20개
   - 스트레스 이벤트 10개

3. **밸런싱 시트 작성**
   - BalanceSheet.md 초안
   - 기본 수치 설정

4. **엔진 설정 시작**
   - Unity 프로젝트 생성
   - 기본 구조 설계

---

## 참고 문서

- DataSchema.md - 데이터 구조 정의
- Localization_Guide.md - 다국어 지원 가이드
- CSV_Integration_Guide.md - CSV 파일 관리 가이드

---

*문서 버전: 1.0*  
*업데이트: 2026-03-25*  
*작성자: AI Assistant*
