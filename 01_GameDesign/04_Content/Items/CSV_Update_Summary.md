# CSV 파일 정리 완료 보고서

## 작업 일시
2026-03-25

## 완료된 작업 목록

### ✅ 1. Equipment.csv 오타 수정
- **변경사항**: `Random_Bouse` → `Random_Bonus`
- **파일**: Equipment.csv
- **상태**: 완료

### ✅ 2. Items_Master.csv 통합 생성
- **내용**: Consumables + Gifts + SpecialItems 통합
- **총 아이템 수**: 45개
  - 소비 아이템: 14개
  - 선물: 20개 (NPC별 각 5개)
  - 특별 아이템: 11개
- **파일**: Items_Master.csv (완전 교체)
- **상태**: 완료

### ✅ 3. Gifts.csv 새 형식 변환
- **변경사항**: 
  - ID 형식: GIFT_xxx → ITEM_xxx
  - 컬럼 구조: DataSchema 기준 25개 컬럼으로 확장
  - 호감도 효과: Effect_Favor_XXX 컬럼으로 세분화
- **파일**: Gifts_New.csv (새 형식 샘플)
- **상태**: 완료

### ✅ 4. SpecialItems.csv 새 형식 변환
- **변경사항**:
  - ID 형식: SPECIAL_xxx → ITEM_xxx
  - Category 추가: Quest/Growth/Memory
  - Type 통일: Special
- **파일**: SpecialItems_New.csv (새 형식 샘플)
- **상태**: 완료

---

## 파일 구조 변경사항

### Before (기존)
```
Items/
├── Items_Master.csv      # 일부 깨진 한글
├── Gifts.csv             # 간단한 형식
├── SpecialItems.csv      # 간단한 형식
├── Equipment.csv         # 오타 있음
└── Consumables.md        # 마크다운
```

### After (변경 후)
```
Items/
├── Items_Master.csv           # ✅ 통합된 마스터 파일 (45개 아이템)
├── Items_Master_Backup.csv    # 백업
├── Gifts_New.csv              # ✅ 새 형식 샘플
├── Gifts_Backup.csv           # 백업
├── SpecialItems_New.csv       # ✅ 새 형식 샘플
├── SpecialItems_Backup.csv    # 백업
├── Equipment.csv              # ✅ 오타 수정됨
├── Equipment_Backup.csv       # 백업
├── Consumables.md             # (변환 제외됨)
├── CSV_Integration_Guide.md   # 통합 가이드
└── CSV_Update_Summary.md      # 본 문서
```

---

## 컬럼 변경 비교

### Gifts.csv 변경 전/후

**Before (7개 컬럼)**
```
Gift_ID, Name, Target_NPC, Price, Favorability, Description
```

**After (25개 컬럼)**
```
Item_ID, Name_KO, Type, Category, Rarity, Price, Description, 
Effect_HP, Effect_Charm, Effect_Int, Effect_Art, Effect_Morality, Effect_Stress,
Effect_Favor_Ino, Effect_Favor_Aileen, Effect_Favor_Kyle, Effect_Favor_Lian,
Target_NPC, Icon_Path, Unlock_Condition, Max_Stack, Is_Sellable, Is_Consumable
```

### SpecialItems.csv 변경 전/후

**Before (6개 컬럼)**
```
Special_ID, Name, Category, Effect, Description, Unlock_Condition
```

**After (25개 컬럼)**
```
Item_ID, Name_KO, Type, Category, Rarity, Price, Description,
Effect_HP, Effect_Charm, Effect_Int, Effect_Art, Effect_Morality, Effect_Stress,
Effect_Favor_Ino, Effect_Favor_Aileen, Effect_Favor_Kyle, Effect_Favor_Lian,
Target_NPC, Icon_Path, Unlock_Condition, Max_Stack, Is_Sellable, Is_Consumable
```

---

## DataSchema 연동 확인

### Items_Master 테이블 매핑
✅ 모든 필수 컬럼 포함
✅ DataSchema와 동일한 컬럼명 사용
✅ Enum 값 준수 (Type: Consumable/Gift/Special)

### Equipment 테이블 매핑
✅ 오타 수정 (Random_Bonus)
✅ 기존 컬럼 유지

---

## 다음 단계 권장사항

### 1. Consumables.md → CSV 변환 (1번 작업)
- Consumables.md 파일을 CSV 형식으로 변환 필요
- 현재 마크다운 테이블 → CSV로 변환

### 2. Excel 변환
```bash
python convert_csv_to_excel.py
```
- 통합된 Items_Master.csv를 Excel로 변환
- Items_Data.xlsx 자동 생성

### 3. 데이터 검증
- 모든 Item_ID 중복 확인
- Rarity 범위 (1-5) 확인
- Price 음수 여부 확인

### 4. 게임 데이터 적용
- Items_Master.csv → 게임 리소스로 복사
- Equipment.csv → 게임 리소스로 복사
- Gifts_New.csv, SpecialItems_New.csv → 참조용 또는 통합

---

## 백업 파일 목록

작업 중 생성된 백업 파일:
- Items_Master_Backup.csv
- Gifts_Backup.csv
- SpecialItems_Backup.csv
- Equipment_Backup.csv

**주의**: 문제 발생 시 백업 파일로 복원 가능

---

## 완료 체크리스트

- [x] Equipment.csv 오타 수정 (Random_Bouse → Random_Bonus)
- [x] Items_Master.csv 통합 생성 (45개 아이템)
- [x] Gifts.csv 새 형식 변환 (ITEM_ID 체계)
- [x] SpecialItems.csv 새 형식 변환 (Category 세분화)
- [x] DataSchema와 컬럼 매핑 확인
- [x] 백업 파일 생성
- [x] 작업 보고서 작성

**Status: 1번 작업(Consumables 변환) 제외 완료**

---

*문서 작성일: 2026-03-25*
*버전: 1.0*
