# 데이터 테이블 정의서

## 목차
1. [Items_Master](#1-items_master)
2. [Equipment](#2-equipment)
3. [Events](#3-events)
4. [NPC_Favor](#4-npc_favor)
5. [Ending_Conditions](#5-ending_conditions)
6. [Character_Stats](#6-character_stats)
7. [CG_Master](#7-cg_master)
8. [BGM_Master](#8-bgm_master)

---

## 1. Items_Master

### 개요
게임 내 모든 아이템의 기본 정보를 관리하는 마스터 테이블

### 파일명
`Items_Master.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| Item_ID | String | Yes | 아이템 고유 ID | ITEM_001 |
| Name_KO | String | Yes | 한글 이름 | 영양제 |
| Name_EN | String | No | 영문 이름 | Nutrient |
| Type | Enum | Yes | 아이템 타입 | Consumable/Equipment/Gift/Special |
| Rarity | Int | Yes | 희귀도 (1-5) | 1=Common, 2=Uncommon, 3=Rare, 4=Epic, 5=Legendary |
| Price | Int | Yes | 가격 | 100 |
| Description | String | Yes | 설명 | 건강을 챙기는 영양제 |
| Effect_HP | Int | No | 체력 변화 | 10 |
| Effect_Charm | Int | No | 매력 변화 | 0 |
| Effect_Int | Int | No | 지능 변화 | 0 |
| Effect_Art | Int | No | 예술 변화 | 0 |
| Effect_Morality | Int | No | 도덕성 변화 | 0 |
| Effect_Stress | Int | No | 스트레스 변화 | -5 |
| Effect_Favor_Ino | Int | No | 이노 호감도 변화 | 0 |
| Effect_Favor_Aileen | Int | No | 아이린 호감도 변화 | 0 |
| Effect_Favor_Kyle | Int | No | 카일 호감도 변화 | 0 |
| Effect_Favor_Lian | Int | No | 리안 호감도 변화 | 0 |
| Icon_Path | String | Yes | 아이콘 경로 | Items/nutrient.png |
| Unlock_Condition | String | No | 해금 조건 | Age>=10 |
| Usable_Age_Min | Int | No | 사용 가능 최소 연령 | 6 |
| Usable_Age_Max | Int | No | 사용 가능 최대 연령 | 18 |
| Max_Stack | Int | Yes | 최대 중첩 수량 | 99 |
| Is_Sellable | Bool | Yes | 판매 가능 여부 | TRUE/FALSE |
| Is_Usable | Bool | Yes | 사용 가능 여부 | TRUE/FALSE |

### Type Enum 값

| 값 | 설명 |
|----|------|
| Consumable | 소비 아이템 (사용 시 소모됨) |
| Equipment | 장비 아이템 (착용 가능) |
| Gift | 선물 아이템 (NPC에게 증정 가능) |
| Special | 특수 아이템 (스토리 관련) |
| Material | 재료 아이템 (제작용) |

### 예시 데이터

```csv
Item_ID,Name_KO,Type,Rarity,Price,Description,Effect_HP,Effect_Stress,Icon_Path
ITEM_001,영양제,Consumable,1,100,건강을 챙기는 영양제,10,-5,Items/nutrient.png
ITEM_002,초콜릿,Gift,2,200,달콤한 초콜릿,0,0,Items/chocolate.png
ITEM_003,레몬 케이크,Special,3,0,이노가 만든 생일 케이크,20,-10,Items/lemon_cake.png
ITEM_004,왕국의 크리스탈,Special,5,0,왕국과 연결되는 보석,0,0,Items/crystal.png
```

---

## 2. Equipment

### 개요
캐릭터가 착용 가능한 장비 아이템 정보

### 파일명
`Equipment.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| Equip_ID | String | Yes | 장비 ID | EQUIP_001 |
| Name_KO | String | Yes | 이름 | 학생복 |
| Name_EN | String | No | 영문 이름 | School Uniform |
| Type | Enum | Yes | 장비 슬롯 | Head/Body/Accessory |
| Rarity | Int | Yes | 희귀도 (1-5) | 2 |
| Required_Age | Int | No | 필요 연령 | 6 |
| Required_Stat | String | No | 필요 스탯 조건 | INT>=100 |
| Effect_HP | Int | No | 체력 보너스 | 0 |
| Effect_Charm | Int | No | 매력 보너스 | 5 |
| Effect_Int | Int | No | 지능 보너스 | 10 |
| Effect_Art | Int | No | 예술 보너스 | 0 |
| Effect_Morality | Int | No | 도덕성 보너스 | 0 |
| Effect_Stress_Reduction | Int | No | 스트레스 감소율 | 0 |
| CG_Variation | String | Yes | 적용될 CG 변형 | Uniform_School |
| Icon_Path | String | Yes | 아이콘 경로 | Equipment/uniform.png |
| Description | String | Yes | 설명 | 학교에서 입는 교복 |
| Is_Unisex | Bool | Yes | 성별 구분 없음 | TRUE |
| Price | Int | Yes | 가격 | 500 |

### Type Enum 값

| 값 | 설명 | 최대 착용 수 |
|----|------|-------------|
| Head | 머리 장비 | 1 |
| Body | 몸통 장비 | 1 |
| Accessory | 액세서리 | 2 |

### 예시 데이터

```csv
Equip_ID,Name_KO,Type,Rarity,Required_Age,Effect_Int,Effect_Charm,CG_Variation,Price
EQUIP_001,학생복,Body,2,6,5,5,Uniform_School,500
EQUIP_002,운동복,Body,2,6,0,0,Uniform_Sports,400
EQUIP_003,예복,Body,3,10,0,20,Dress_Formal,2000
EQUIP_004,리본,Accessory,1,6,0,3,Acc_Ribbon,200
EQUIP_005,안경,Accessory,2,10,5,0,Acc_Glasses,300
```

---

## 3. Events

### 개요
게임 내 모든 이벤트의 트리거 조건 및 실행 정보

### 파일명
`Events.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| Event_ID | String | Yes | 이벤트 ID | EVT_001 |
| Name_KO | String | Yes | 이벤트명 | 6세 생일 |
| Type | Enum | Yes | 이벤트 타입 | Fixed/Random/NPC/Special/Hidden |
| Trigger_Type | Enum | Yes | 트리거 타입 | Age/Date/Stat/Favor/Random |
| Trigger_Value | String | Yes | 트리거 조건 값 | Age=6,Date=1224,Favor_Ino>=50 |
| Required_Previous_Event | String | No | 선행 필요 이벤트 | EVT_000 |
| Age_Min | Int | No | 최소 연령 | 6 |
| Age_Max | Int | No | 최대 연령 | 18 |
| Month | Int | No | 발생 월 | 12 |
| Day | Int | No | 발생 일 | 24 |
| Stat_Condition | String | No | 스탯 조건 | HP>=500,Stress<=30 |
| NPC_Condition | String | No | NPC 조건 | Favor_Ino>=50,Favor_Aileen>=30 |
| Script_Path | String | Yes | 스크립트 파일 경로 | Events/Birthday_006.json |
| CG_ID | String | No | 사용 CG ID | CG_Birthday_001 |
| BGM_ID | String | No | 사용 BGM ID | BGM_Happy |
| Next_Event | String | No | 다음 이벤트 ID | EVT_002 |
| Is_Repeatable | Bool | Yes | 반복 가능 여부 | FALSE |
| Is_Hidden | Bool | Yes | 히든 이벤트 여부 | FALSE |
| Priority | Int | Yes | 실행 우선순위 | 1-10 |
| Description | String | Yes | 이벤트 설명 | 루아의 6번째 생일 이벤트 |

### Type Enum 값

| 값 | 설명 |
|----|------|
| Fixed | 고정 이벤트 (특정 조건에서 항상 발생) |
| Random | 랜덤 이벤트 (확률에 따라 발생) |
| NPC | NPC 관련 이벤트 |
| Special | 특별 이벤트 (생일, 기념일 등) |
| Hidden | 히든 이벤트 (특별한 조건 필요) |

### Trigger_Type Enum 값

| 값 | 설명 | 예시 |
|----|------|------|
| Age | 연령 기반 | Age=6 |
| Date | 날짜 기반 | Date=1224 (12월 24일) |
| Stat | 스탯 기반 | INT>=500 |
| Favor | 호감도 기반 | Favor_Ino>=50 |
| Random | 랜덤 발생 | Random=20% |
| Story | 스토리 진행 | Event_EVT_001_Complete |
| Manual | 수동 발생 | - |

### 예시 데이터

```csv
Event_ID,Name_KO,Type,Trigger_Type,Trigger_Value,Age_Min,Month,Day,Script_Path,Is_Repeatable,Priority
EVT_PROLOGUE,프롤로그,Fixed,Age,Age=6,6,-1,-1,Events/Prologue.json,FALSE,10
EVT_BIRTHDAY_006,6세 생일,Special,Date,Date=1224,6,12,24,Events/Birthday_006.json,FALSE,9
EVT_AILEEN_FIRST,아이린 첫 등장,NPC,Favor,Favor_Aileen=0,7,-1,-1,Events/Aileen_First.json,FALSE,5
EVT_RANDOM_STUDY,랜덤 학습 이벤트,Random,Random,Random=20%,6,-1,-1,Events/Random_Study.json,TRUE,3
```

---

## 4. NPC_Favor

### 개요
NPC 호감도 시스템 및 레벨 정의

### 파일명
`NPC_Favor.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| NPC_ID | String | Yes | NPC ID | NPC_Ino |
| NPC_Name | String | Yes | NPC 이름 | 이노 |
| Favor_Level | Int | Yes | 호감도 레벨 (1-5) | 1 |
| Min_Favor | Int | Yes | 최소 호감도 값 | 0 |
| Max_Favor | Int | Yes | 최대 호감도 값 | 20 |
| Event_ID | String | Yes | 해당 레벨 대표 이벤트 ID | EVT_Ino_Level1 |
| Dialogue_Path | String | Yes | 대사 파일 경로 | Dialogues/Ino_Level1.json |
| CG_Variation | String | No | CG 변형 | Normal/Happy/Blush |
| Unlock_Content | String | No | 해금 콘텐츠 | Shop_Item_001 |
| Special_Reward | String | No | 특별 보상 | Item_Special_001 |

### 호감도 레벨 정의

| 레벨 | 이름 | 범위 | 관계 상태 |
|------|------|------|----------|
| 1 | 첫 만남 | 0-20 | 어색함, 거리감 |
| 2 | 친구 | 21-40 | 편안함, 친근함 |
| 3 | 친한 친구 | 41-60 | 신뢰, 친밀감 |
| 4 | 높은 신뢰 | 61-80 | 특별한 감정, 깊은 유대 |
| 5 | 깊은 유대 | 81-100 | 영원한 인연, 연인/절친 |

### 특별 조건 (리안)

| NPC_ID | 특별 조건 | 시작 호감도 | 첫 만남 연령 |
|--------|----------|------------|------------|
| NPC_Lian | 13세에 첫 등장 | 50 | 13 |

### 예시 데이터

```csv
NPC_ID,NPC_Name,Favor_Level,Min_Favor,Max_Favor,Event_ID,Dialogue_Path
NPC_Ino,이노,1,0,20,EVT_Ino_L1,Dialogues/Ino_L1.json
NPC_Ino,이노,2,21,40,EVT_Ino_L2,Dialogues/Ino_L2.json
NPC_Ino,이노,3,41,60,EVT_Ino_L3,Dialogues/Ino_L3.json
NPC_Ino,이노,4,61,80,EVT_Ino_L4,Dialogues/Ino_L4.json
NPC_Ino,이노,5,81,100,EVT_Ino_L5,Dialogues/Ino_L5.json
NPC_Aileen,아이린,1,0,20,EVT_Aileen_L1,Dialogues/Aileen_L1.json
NPC_Lian,리안,3,50,60,EVT_Lian_First,Dialogues/Lian_L3.json
```

---

## 5. Ending_Conditions

### 개요
엔딩 달성 조건 및 판정 우선순위 정의

### 파일명
`Ending_Conditions.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| Ending_ID | String | Yes | 엔딩 ID | END_001 |
| Name_KO | String | Yes | 엔딩명 | 여왕의 길 |
| Name_EN | String | No | 영문명 | Path of Queen |
| Type | Enum | Yes | 엔딩 타입 | Basic/Hidden |
| Priority | Int | Yes | 판정 우선순위 (1-20) | 1 |
| Req_HP | String | No | 체력 조건 | 500+ |
| Req_Charm | String | No | 매력 조건 | 800+ |
| Req_Int | String | No | 지능 조건 | 800+ |
| Req_Art | String | No | 예술 조건 | 400+ |
| Req_Morality | String | No | 도덕성 조건 | 600+ |
| Req_Stress | String | No | 스트레스 조건 | <=30 |
| Req_Favor_Ino | String | No | 이노 호감도 조건 | >=80 |
| Req_Favor_Aileen | String | No | 아이린 호감도 조건 | >=50 |
| Req_Favor_Kyle | String | No | 카일 호감도 조건 | >=50 |
| Req_Favor_Lian | String | No | 리안 호감도 조건 | >=70 |
| Req_Events | String | No | 필요 이벤트 ID 목록 | EVT_001,EVT_002 |
| Req_Choices | String | No | 필요 선택지 ID | Choice_001_A |
| Req_Flags | String | No | 필요 플래그 | Flag_Lover_Route=TRUE |
| Script_Path | String | Yes | 엔딩 스크립트 경로 | Endings/Queen.json |
| CG_Path | String | Yes | 엔딩 CG 경로 | CG/Ending_Queen.png |
| BGM_Path | String | Yes | 엔딩 BGM 경로 | BGM/Ending_Queen.mp3 |
| Description | String | Yes | 엔딩 설명 | 왕국의 여왕이 되는 엔딩 |
| Unlock_Next_Run | String | No | 2주차 해금 요소 | Item_Special_001 |

### Type Enum 값

| 값 | 설명 | 개수 |
|----|------|------|
| Basic | 기본 엔딩 | 13개 |
| Hidden | 히든 엔딩 | 2개 |
| Secret | 시크릿 엔딩 | - |

### 우선순위 가이드

| 우선순위 | 엔딩 타입 |
|---------|----------|
| 1-2 | 히든 엔딩 (가장 높은 우선순위) |
| 3-15 | 기본 엔딩 (스탯 기반) |
| 16 | 균형잡힌 인재 |
| 17 | 평범한 행복 (자동) |

### 예시 데이터

```csv
Ending_ID,Name_KO,Type,Priority,Req_Charm,Req_Int,Req_Morality,Req_Favor_Ino,Req_Flags
END_001,여왕의 길,Basic,5,800+,800+,600+,>=80,-
END_002,현명한 학자,Basic,6,-,900+,-,>=60,-
END_003,반짝이는 아이돌,Basic,7,900+,-,-,>=60,-
END_014,차원의 연결자,Hidden,1,-,-,-,-,Flag_Lian_Secret=TRUE
END_015,새로운 여왕,Hidden,2,800+,800+,600+,>=100,Flag_Lover_Route=TRUE
```

---

## 6. Character_Stats

### 개요
캐릭터(루아)의 스탯 성장 및 관리

### 파일명
`Character_Stats.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| Stat_ID | String | Yes | 스탯 ID | HP |
| Name_KO | String | Yes | 스탯 이름 | 체력 |
| Name_EN | String | No | 영문명 | Health |
| Description | String | Yes | 설명 | 신체적 건강과 활력 |
| Min_Value | Int | Yes | 최소 값 | 0 |
| Max_Value | Int | Yes | 최대 값 | 999 |
| Initial_Value | Int | Yes | 초기 값 | 50 |
| Growth_Rate | Float | Yes | 기본 성장률 | 1.0 |
| Is_Primary | Bool | Yes | 주요 스탯 여부 | TRUE |
| Icon_Path | String | Yes | 아이콘 경로 | Stats/hp.png |
| Color_Code | String | Yes | UI 색상 코드 | #FF0000 |

### 스탯 종류

| Stat_ID | 이름 | 설명 | 엔딩 관련 |
|---------|------|------|----------|
| HP | 체력 | 신체적 건강 | 운동선수 |
| CHARM | 매력 | 외모와 매력 | 아이돌, 사교적 리더 |
| INT | 지능 | 학습 능력 | 학자, 여왕 |
| ART | 예술 | 예술적 감각 | 예술가, 요리사 |
| MORALITY | 도덕성 | 도덕적 기준 | 성녀, 교사 |
| STRESS | 스트레스 | 정신적 부담 | 자유로운 영혼 (낮을수록 좋음) |

### 예시 데이터

```csv
Stat_ID,Name_KO,Description,Min_Value,Max_Value,Initial_Value,Is_Primary
HP,체력,신체적 건강과 활력,0,999,50,TRUE
CHARM,매력,외모와 매력,0,999,30,TRUE
INT,지능,학습 능력과 지식,0,999,30,TRUE
ART,예술,예술적 감각과 창의성,0,999,20,TRUE
MORALITY,도덕성,도덕적 기준과 양심,0,999,50,TRUE
STRESS,스트레스,정신적 부담과 압박,0,100,0,FALSE
```

---

## 7. CG_Master

### 개요
게임 내 모든 CG 이미지 메타데이터

### 파일명
`CG_Master.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| CG_ID | String | Yes | CG ID | CG_001 |
| Name_KO | String | Yes | CG 이름 | 6세 생일 |
| Category | Enum | Yes | CG 카테고리 | Event/Ending/Character |
| Characters | String | Yes | 등장 캐릭터 | 루아,이노 |
| Background | String | Yes | 배경 ID | BG_Room_Day |
| File_Path | String | Yes | 파일 경로 | CG/Event_Birthday_006.png |
| Thumbnail_Path | String | No | 썸네일 경로 | Thumbnails/CG_001.png |
| Unlock_Condition | String | Yes | 해금 조건 | Event_EVT_BIRTHDAY_006 |
| Is_Hidden | Bool | Yes | 히든 CG 여부 | FALSE |
| Description | String | Yes | CG 설명 | 루아의 6번째 생일 파티 장면 |

### Category Enum 값

| 값 | 설명 |
|----|------|
| Event | 이벤트 CG |
| Ending | 엔딩 CG |
| Character | 캐릭터 CG |
| Special | 특별 CG |

### 예시 데이터

```csv
CG_ID,Name_KO,Category,Characters,Background,File_Path,Unlock_Condition
CG_001,6세 생일,Event,루아;이노,BG_Room_Day,CG/Birthday_006.png,Event_EVT_BIRTHDAY_006
CG_100,여왕의 길,Ending,루아;이노,BG_Castle_Throne,CG/Ending_Queen.png,Ending_END_001
CG_200,이노 표정 변화,Character,이노,BG_None,CG/Chara_Ino_Blush.png,Favor_Ino>=80
```

---

## 8. BGM_Master

### 개요
게임 내 모든 배경음악 메타데이터

### 파일명
`BGM_Master.csv`

### 스키마

| 컬럼명 | 데이터 타입 | 필수 | 설명 | 예시 |
|--------|------------|------|------|------|
| BGM_ID | String | Yes | BGM ID | BGM_001 |
| Name_KO | String | Yes | BGM 이름 | 행복한 하루 |
| Name_EN | String | No | 영문명 | Happy Day |
| Category | Enum | Yes | BGM 카테고리 | Daily/Event/Ending |
| Mood | String | Yes | 분위기 | Happy/Calm/Sad/Tense |
| File_Path | String | Yes | 파일 경로 | BGM/Happy_Day.mp3 |
| Loop_Start | Float | No | 루프 시작 시간(초) | 0.0 |
| Loop_End | Float | No | 루프 종료 시간(초) | 120.0 |
| Volume | Float | Yes | 기본 볼륨 (0.0-1.0) | 0.8 |
| Is_Locked | Bool | Yes | 잠금 여부 | FALSE |
| Unlock_Condition | String | No | 해금 조건 | - |

### Category Enum 값

| 값 | 설명 |
|----|------|
| Title | 타이틀 화면 |
| Daily | 일상 |
| Event | 이벤트 |
| Ending | 엔딩 |
| Minigame | 미니게임 |
| System | 시스템 |

### Mood 값

| 값 | 설명 |
|----|------|
| Happy | 밝고 행복한 |
| Calm | 차분한 |
| Sad | 슬픈 |
| Tense | 긴장감 있는 |
| Romantic | 로맨틱한 |
| Mysterious | 신비로운 |

### 예시 데이터

```csv
BGM_ID,Name_KO,Category,Mood,File_Path,Volume
BGM_001,행복한 하루,Daily,Happy,BGM/Happy_Day.mp3,0.8
BGM_050,생일 축하,Event,Happy,BGM/Birthday.mp3,0.9
BGM_100,여왕의 길,Ending,Romantic,BGM/Ending_Queen.mp3,1.0
```

---

## 데이터 관계도

```
Items_Master
    │
    ├─ Equipment (Type=Equipment)
    │
    └─ Gift (Type=Gift)
         │
         └─ NPC_Favor (호감도 상승)

Events
    │
    ├─ Script_Path (NPC_Dialogues 참조)
    │
    ├─ CG_ID → CG_Master
    │
    └─ BGM_ID → BGM_Master

Ending_Conditions
    │
    ├─ Req_Stats → Character_Stats
    │
    ├─ Req_Favor → NPC_Favor
    │
    └─ Req_Events → Events

Character_Stats
    │
    └─ 모든 테이블에서 참조
```

---

## 파일 저장 경로

```
01_GameDesign/
├── Data/
│   ├── Items_Master.csv
│   ├── Equipment.csv
│   ├── Events.csv
│   ├── NPC_Favor.csv
│   ├── Ending_Conditions.csv
│   ├── Character_Stats.csv
│   ├── CG_Master.csv
│   └── BGM_Master.csv
└── DataSchema.md
```

---

*문서 작성일: 2026-03-25*
*버전: 1.0*
