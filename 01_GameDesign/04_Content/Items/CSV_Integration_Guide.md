# 아이템 CSV 통합 가이드

## 통합 방식: 단일 Items_Master (권장)

### 최종 파일 구조

```
04_Content/Items/
├── Items_Master.csv          # 모든 아이템 통합 (소비/선물/특별/히든)
├── Equipment.csv             # 장비 아이템 (착용용)
├── Items_Categories.md       # 카테고리별 설명 문서
└── README.md                 # 파일 설명
```

---

## 통합 Items_Master.csv 구조

### 컬럼 정의 (DataSchema 기준)

| 컬럼 | 데이터 타입 | 필수 | 설명 | 예시 값 |
|------|------------|------|------|--------|
| Item_ID | String | Yes | 고유 ID | ITEM_001 |
| Name_KO | String | Yes | 한글 이름 | 영양제 |
| Name_EN | String | No | 영문 이름 | Nutrient |
| Type | Enum | Yes | 아이템 타입 | Consumable/Gift/Special/Quest |
| Category | String | No | 세부 카테고리 | Recovery/Stat/Gift_Special |
| Rarity | Int | Yes | 희귀도 (1-5) | 1=Common ~ 5=Legendary |
| Price | Int | Yes | 가격 (0=획득/이벤트) | 100 |
| Description | String | Yes | 설명 | 건강을 챙기는 영양제 |
| Effect_HP | Int | No | 체력 변화 | +10 |
| Effect_Charm | Int | No | 매력 변화 | 0 |
| Effect_Int | Int | No | 지능 변화 | 0 |
| Effect_Art | Int | No | 예술 변화 | 0 |
| Effect_Morality | Int | No | 도덕성 변화 | 0 |
| Effect_Stress | Int | No | 스트레스 변화 | -5 |
| Effect_Favor_Ino | Int | No | 이노 호감도 | 0 |
| Effect_Favor_Aileen | Int | No | 아이린 호감도 | 0 |
| Effect_Favor_Kyle | Int | No | 카일 호감도 | 0 |
| Effect_Favor_Lian | Int | No | 리안 호감도 | 0 |
| Target_NPC | String | No | 선물 대상 NPC | Ino/Aileen/Kyle/Lian/All |
| Icon_Path | String | Yes | 아이콘 경로 | Items/nutrient.png |
| Unlock_Condition | String | No | 해금 조건 | Shop/AGE:6/EVENT:001 |
| Max_Stack | Int | Yes | 최대 중첩 | 99 |
| Is_Sellable | Bool | Yes | 판매 가능 | TRUE/FALSE |
| Is_Consumable | Bool | Yes | 사용 시 소모 | TRUE/FALSE |

### Type Enum 값

- **Consumable**: 소비 아이템 (사용 시 소모)
- **Gift**: 선물 아이템 (NPC에게 증정)
- **Special**: 특별 아이템 (스토리/히든 관련)
- **Quest**: 퀘스트 아이템 (진행용)

---

## 데이터 마이그레이션 예시

### 1. Consumables → Items_Master

**변환 전 (Consumables.md):**
```
| 영양제 | 100 | 체력 +10 | 건강을 챙기는 기본 영양제 | ★☆☆☆☆ |
```

**변환 후 (Items_Master.csv):**
```csv
Item_ID,Name_KO,Type,Rarity,Price,Description,Effect_HP,Effect_Stress,Icon_Path,Max_Stack,Is_Sellable,Is_Consumable
ITEM_001,영양제,Consumable,1,100,건강을 챙기는 기본 영양제,10,-5,Items/nutrient.png,99,TRUE,TRUE
```

### 2. Gifts → Items_Master

**변환 전 (Gifts.csv):**
```csv
Gift_ID,Name,Target_NPC,Price,Favorability,Description
GIFT_001,커피 원두,이노,120,10,신선한 원두 커피
```

**변환 후 (Items_Master.csv):**
```csv
Item_ID,Name_KO,Type,Category,Rarity,Price,Description,Effect_Favor_Ino,Target_NPC,Icon_Path
ITEM_101,커피 원두,Gift,Gift_Food,2,120,신선한 원두 커피,10,Ino,Items/gift_coffee.png
```

### 3. SpecialItems → Items_Master

**변환 전 (SpecialItems.csv):**
```csv
Special_ID,Name,Category,Effect,Description,Unlock_Condition
SPECIAL_001,고대의 책,퀘스트,HIDDEN:EVENT:1,왕국의 고대 역사가 담긴 책,BOOKSTORE:SPECIAL:EVENT
```

**변환 후 (Items_Master.csv):**
```csv
Item_ID,Name_KO,Type,Category,Rarity,Price,Description,Effect_Stress,Icon_Path,Unlock_Condition,Max_Stack
ITEM_201,고대의 책,Special,Quest,3,0,왕국의 고대 역사가 담긴 책,0,Items/book_ancient.png,EVENT:BOOKSTORE,1
```

---

## Equipment.csv 유지 구조

Equipment는 **착용 개념**이므로 별도 테이블 유지

### 기존 구조 (변경 없음)

```csv
Equipment_ID,Name,Type,Price,Charm_Bonus,HP_Bonus,Int_Bonus,Art_Bonus,Morality_Bonus,Description,Unlock_Condition,Rarity
EQ_001,평상복,의상,0,0,0,0,0,0,기본적인 평상복,NONE,1
EQ_002,귀여운 원피스,의상,300,15,0,0,0,0,리본이 달린 귀여운 원피스,NONE,2
```

### 장비 슬롯 시스템

- **Head**: 머리 장비 (1개)
- **Body**: 몸통 장비 (1개)
- **Accessory**: 액세서리 (2개)

---

## 최종 Items_Master.csv 샘플

```csv
Item_ID,Name_KO,Type,Category,Rarity,Price,Description,Effect_HP,Effect_Charm,Effect_Int,Effect_Art,Effect_Morality,Effect_Stress,Effect_Favor_Ino,Effect_Favor_Aileen,Effect_Favor_Kyle,Effect_Favor_Lian,Target_NPC,Icon_Path,Unlock_Condition,Max_Stack,Is_Sellable,Is_Consumable
# 소비 아이템 - 회복
ITEM_001,영양제,Consumable,Recovery,1,100,건강을 챙기는 기본 영양제,10,0,0,0,0,0,0,0,0,0,All,Items/nutrient.png,Shop,99,TRUE,TRUE
ITEM_002,비타민 세트,Consumable,Recovery,2,180,고급 영양제 세트,15,0,0,0,0,-5,0,0,0,0,All,Items/vitamin.png,Shop,99,TRUE,TRUE
ITEM_003,스트레스 해소제,Consumable,Recovery,2,200,긴장을 풀어주는 특제 약,0,0,0,0,0,-20,0,0,0,0,All,Items/stress_relief.png,Shop,50,TRUE,TRUE

# 소비 아이템 - 스탯 강화
ITEM_011,화장품 세트,Consumable,StatBoost,2,150,피부 관리용 화장품 (일회성),0,10,0,0,0,0,0,0,0,0,All,Items/cosmetics.png,Shop,30,TRUE,TRUE
ITEM_012,명작 소설,Consumable,StatBoost,2,120,세계적인 문학 작품,0,0,8,0,0,0,0,0,0,0,All,Items/book_novel.png,Shop,30,TRUE,TRUE
ITEM_013,미술 도구 세트,Consumable,StatBoost,2,180,붓과 물감 세트,0,0,0,10,0,0,0,0,0,0,All,Items/art_tools.png,Shop,30,TRUE,TRUE

# 선물 - 이노
ITEM_101,커피 원두,Gift,Gift_Food,2,120,신선한 원두 커피,0,0,0,0,0,0,10,0,0,0,Ino,Items/gift_coffee.png,Shop,99,TRUE,FALSE
ITEM_102,토피넛 시럽,Gift,Gift_Food,2,150,이노가 생각나는 선물,0,0,0,0,0,0,15,0,0,0,Ino,Items/gift_syrup.png,Shop,99,TRUE,FALSE
ITEM_103,수제 쿠키,Gift,Gift_Food,2,100,정성껏 만든 쿠키,0,0,0,0,0,0,12,0,0,0,Ino,Items/gift_cookie.png,Shop,99,TRUE,FALSE
ITEM_104,특별 선물,Gift,Gift_Special,4,300,이노가 좋아하는 특별한 것,0,0,0,0,0,0,25,0,0,0,Ino,Items/gift_special_ino.png,Shop,10,TRUE,FALSE

# 선물 - 아이린
ITEM_111,핸드크림,Gift,Gift_Cosmetic,2,100,예쁘게 포장된 핸드크림,0,0,0,0,0,0,0,10,0,0,Aileen,Items/gift_handcream.png,Shop,99,TRUE,FALSE
ITEM_112,마카롱 세트,Gift,Gift_Food,2,180,아이린이 생각나는 선물,0,0,0,0,0,0,0,20,0,0,Aileen,Items/gift_macaron.png,Shop,99,TRUE,FALSE
ITEM_113,특별 선물,Gift,Gift_Special,4,300,아이린의 취향 저격 선물,0,0,0,0,0,0,0,25,0,0,Aileen,Items/gift_special_aileen.png,Shop,10,TRUE,FALSE

# 선물 - 카일
ITEM_121,차 세트,Gift,Gift_Lifestyle,2,150,고급 차 세트,0,0,0,0,0,0,0,0,10,0,Kyle,Items/gift_tea.png,Shop,99,TRUE,FALSE
ITEM_122,블랙 티,Gift,Gift_Food,2,200,카일이 생각나는 선물,0,0,0,0,0,0,0,0,20,0,Kyle,Items/gift_blacktea.png,Shop,99,TRUE,FALSE
ITEM_123,특별 선물,Gift,Gift_Special,4,300,카일이 필요한 특별한 것,0,0,0,0,0,0,0,0,25,0,Kyle,Items/gift_special_kyle.png,Shop,10,TRUE,FALSE

# 선물 - 리안
ITEM_131,디저트,Gift,Gift_Food,2,80,맛있는 디저트,0,0,0,0,0,0,0,0,0,10,Lian,Items/gift_dessert.png,Shop,99,TRUE,FALSE
ITEM_132,레몬 스콘,Gift,Gift_Food,3,120,루아와 같은 레몬 스콘,0,0,0,0,0,0,0,0,0,20,Lian,Items/gift_lemonscone.png,Shop,50,TRUE,FALSE
ITEM_133,특별 선물,Gift,Gift_Special,4,300,리안과의 관계를 깊게 하는 선물,0,0,0,0,0,0,0,0,0,25,Lian,Items/gift_special_lian.png,Shop,10,TRUE,FALSE

# 특별 아이템 - 퀘스트
ITEM_201,고대의 책,Special,Quest,3,0,왕국의 고대 역사가 담긴 책,0,0,0,0,0,0,0,0,0,0,All,Items/book_ancient.png,EVENT:BOOKSTORE,1,FALSE,FALSE
ITEM_202,신비한 돌,Special,Quest,4,0,차원의 힘이 담긴 돌,0,0,0,0,0,0,0,0,0,0,All,Items/stone_mystery.png,AGE:12,1,FALSE,FALSE
ITEM_203,여왕의 편지,Special,Quest,4,0,현 여왕이 직접 쓴 편지,0,0,0,0,0,0,0,0,0,0,All,Items/letter_queen.png,MAIL:DELIVERY,1,FALSE,FALSE
ITEM_204,차원의 열쇠,Special,Quest,5,0,차원을 넘나드는 열쇠 (히든 엔딩),0,0,0,0,0,0,0,0,0,0,All,Items/key_dimension.png,EVENT:HIDDEN_1,1,FALSE,FALSE

# 특별 아이템 - 성장
ITEM_211,성장의 비약,Special,Growth,3,5000,순간적인 성장을 돕는 비약,20,20,20,20,20,0,0,0,0,0,All,Items/potion_growth.png,Shop:SPECIAL,10,TRUE,TRUE
ITEM_212,지혜의 서,Special,Growth,4,8000,모든 지식이 담긴 전설의 서,0,0,30,0,0,0,0,0,0,0,All,Items/book_wisdom.png,QUEST:REWARD,1,TRUE,TRUE

# 특별 아이템 - 기념품
ITEM_221,처음 받은 인형,Special,Memory,2,0,6세 생일에 받은 인형,0,0,0,0,0,-5,0,0,0,0,All,Items/doll_first.png,AGE:6:BIRTHDAY,1,FALSE,FALSE
ITEM_222,졸업식 꽃다발,Special,Memory,2,0,초등학교 졸업 기념,0,0,0,0,0,0,0,0,0,0,All,Items/flowers_graduation.png,AGE:10:EVENT,1,FALSE,FALSE
ITEM_223,왕국의 크리스탈,Special,Memory,5,0,왕국과 연결되는 보석,0,0,0,0,0,0,0,0,0,0,All,Items/crystal_kingdom.png,AGE:10:GIFT,1,FALSE,FALSE
```

---

## CSV 파일 생성 단계

### 단계 1: 기존 파일 백업
```
Items_Backup/
├── Original_Consumables.md
├── Original_Gifts.csv
├── Original_SpecialItems.csv
└── Original_Equipment.csv
```

### 단계 2: 통합 Items_Master 생성
1. 새 파일: `Items_Master.csv`
2. 위 샘플의 헤더 행 복사
3. 기존 파일들의 데이터를 새 형식으로 변환하여 입력
4. 중복 ID 확인 (Item_ID 유일성)

### 단계 3: Equipment 정리
1. 기존 Equipment.csv 컬럼명 확인
2. DataSchema와 차이점 수정:
   - `Random_Bouse` → `Random_Bonus` (오타 수정)
3. CG_Variation 컬럼 추가 (선택)

### 단계 4: 데이터 검증
1. **ID 중복 체크**: 모든 Item_ID가 유일해야 함
2. **Rarity 범위**: 1-5 범위 확인
3. **Price 음수 여부**: 무료 아이템은 0
4. **Effect 합산**: 논리적 오류 없는지 확인

---

## Excel 변환

통합 후 Excel 변환은 기존 스크립트 사용:

```bash
# 통합된 Items_Master.csv와 Equipment.csv를 Excel로 변환
python convert_csv_to_excel.py
```

변환 결과:
```
Items_Data.xlsx
├── Sheet: Items_Master (통합된 모든 아이템)
├── Sheet: Equipment (장비 아이템)
└── Sheet: Categories (카테고리 정의)
```

---

## 데이터 관계 다이어그램

```
Items_Master
├── Type: Consumable (소비)
│   ├── Category: Recovery (회복)
│   └── Category: StatBoost (스탯 강화)
├── Type: Gift (선물)
│   ├── Category: Gift_Food (음식)
│   ├── Category: Gift_Cosmetic (화장품)
│   ├── Category: Gift_Lifestyle (생활용품)
│   └── Category: Gift_Special (특별 선물)
├── Type: Special (특별)
│   ├── Category: Quest (퀘스트)
│   ├── Category: Growth (성장)
│   └── Category: Memory (기념품)
└── Type: Quest (퀘스트 전용)

Equipment (별도 테이블)
├── Type: Head (머리)
├── Type: Body (몸통)
└── Type: Accessory (액세서리)
```

---

## 주의사항

### 1. 인코딩
- 모든 CSV 파일은 **UTF-8 with BOM**로 저장
- 한글 깨짐 방지

### 2. 필수 컬럼
- Items_Master: Item_ID, Name_KO, Type, Rarity, Price, Description
- Equipment: Equipment_ID, Name, Type, Price, Description

### 3. 외래키 관계
- Items_Master.Target_NPC → NPC_Favorite 테이블 참조
- Equipment.Unlock_Condition → Events/Event_ID 참조 가능

### 4. 밸런싱 체크
- 가격 대비 효과 적절성
- 희귀도 대비 효과 적절성
- NPC별 선물 효과 균형

---

*문서 작성일: 2026-03-25*
*버전: 1.0*
