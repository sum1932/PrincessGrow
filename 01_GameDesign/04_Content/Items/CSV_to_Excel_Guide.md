# CSV to Excel 변환 가이드

## 방법 1: Python 스크립트 사용

```python
import pandas as pd

# CSV 파일 경로
csv_files = {
    'Items_Master': 'Items_Master.csv',
    'Equipment': 'Equipment.csv', 
    'Gifts': 'Gifts.csv'
}

# Excel 파일 생성
with pd.ExcelWriter('Items_Data.xlsx', engine='openpyxl') as writer:
    for sheet_name, csv_file in csv_files.items():
        df = pd.read_csv(csv_file, encoding='utf-8')
        df.to_excel(writer, sheet_name=sheet_name, index=False)
        print(f'{sheet_name} 변환 완료')
```

## 방법 2: 수동 변환

### Excel에서 CSV 열기
1. Excel 실행
2. **데이터** → **텍스트/CSV 가져오기**
3. CSV 파일 선택
4. **파일 원본**: UTF-8 선택
5. **불러오기**
6. **파일** → **다른 이름으로 저장** → .xlsx 선택

### CSV 파일 목록
- `Items_Master.csv` - 기본 아이템 데이터
- `Equipment.csv` - 장비/의상 데이터
- `Gifts.csv` - NPC 선물 데이터

## 방법 3: Google Sheets 사용

1. Google Sheets 열기
2. **파일** → **가져오기** → **업로드**
3. CSV 파일 선택
4. 인코딩: UTF-8 선택
5. **파일** → **다운로드** → Microsoft Excel (.xlsx)

## 데이터 시트 구조

### Items_Master 시트
| 컬럼 | 설명 |
|------|------|
| Item_ID | 아이템 고유 ID |
| Name | 아이템 이름 |
| Category | 카테고리 |
| Price | 가격 |
| Effect | 효과 |
| Description | 설명 |
| Rarity | 희귀도 |

### Equipment 시트
| 컬럼 | 설명 |
|------|------|
| Equipment_ID | 장비 ID |
| Name | 장비 이름 |
| Type | 타입 |
| Price | 가격 |
| Charm_Bonus | 매력 보너스 |
| HP_Bonus | 체력 보너스 |
| Int_Bonus | 지능 보너스 |
| Art_Bonus | 예술 보너스 |
| Morality_Bonus | 도덕성 보너스 |
| Description | 설명 |
| Unlock_Condition | 해금 조건 |
| Rarity | 희귀도 |

### Gifts 시트
| 컬럼 | 설명 |
|------|------|
| Gift_ID | 선물 ID |
| Name | 선물 이름 |
| Target_NPC | 대상 NPC |
| Price | 가격 |
| Favorability | 호감도 |
| Description | 설명 |
