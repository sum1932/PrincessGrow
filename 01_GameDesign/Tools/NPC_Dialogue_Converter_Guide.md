# NPC 대사 변환 도구 사용 가이드

## 개요

Markdown 형식의 NPC 대사 스크립트를 게임 엔진에서 사용 가능한 CSV 형식으로 변환하는 도구입니다.

## 파일 구조

```
GameProject/
├── 01_GameDesign/
│   ├── 02_Narrative/
│   │   └── NPC_Dialogues.md          # 원본 Markdown 파일
│   ├── Data/
│   │   ├── NPC_Dialogues_KO.csv      # 변환된 CSV 파일
│   │   └── NPC_Dialogues_KO_Updated.csv  # 개선된 CSV 샘플
│   └── Tools/
│       └── npc_dialogue_converter.py # 변환 스크립트
```

## 설치 방법

1. Python 3.7 이상 설치 필요
2. 추가 패키지 불필요 (표준 라이브러리만 사용)

## 사용 방법

### 기본 사용법

```bash
python npc_dialogue_converter.py <input_md_file> <output_csv_file>
```

### 예시

```bash
# Windows
python Tools/npc_dialogue_converter.py 02_Narrative/NPC_Dialogues.md Data/NPC_Dialogues_KO.csv

# Linux/Mac
python3 Tools/npc_dialogue_converter.py 02_Narrative/NPC_Dialogues.md Data/NPC_Dialogues_KO.csv
```

## 입력 파일 형식 (Markdown)

### 지원되는 Markdown 구조

```markdown
## 1. 이노 (Ino)

### 레벨 1 (0~20): 첫 만남

#### 이벤트 1: 인사이동 첫날
**조건:** 게임 시작 후 첫 이벤트

**이노:**
"어, 어... 안녕하세요? 저는 이노라고 해요."
"갑자기 이런 일이 생겨서... 정말 놀라셨죠?"

**선택지:**
- 선택지 1: "고마워..." → 호감도 +3, 스트레스 -5
- 선택지 2: "...정말 괜찮아?" → 호감도 +5, 스트레스 -3

**결과:**
- 호감도 변화: +3~+5
- 스트레스 변화: -3~-5
```

### 필수 요소

1. **NPC 이름**: `## 1. 이노 (Ino)` 형식
2. **레벨**: `### 레벨 1 (0~20): 설명` 형식
3. **이벤트**: `#### 이벤트 1: 제목` 형식
4. **대사**: `"대사 내용"` 형식 (큰따옴표 필수)
5. **선택지**: `- 선택지 N: "텍스트" → 결과` 형식
6. **결과**: `**결과:**` 섹션

## 출력 파일 형식 (CSV)

### 컬럼 구조

| 컬럼명 | 설명 | 예시 |
|--------|------|------|
| Dialogue_ID | 고유 대사 ID | D_Ino_L1_001_001 |
| Event_ID | 이벤트 그룹 ID | E_Ino_L1_001 |
| NPC_Name | NPC 이름 | 이노 |
| Level | 호감도 레벨 | 1 |
| Event_Number | 이벤트 번호 | 1 |
| Text_Type | 텍스트 유형 | Dialogue/Choice/Result |
| Sequence | 순서 번호 | 1, 2, 3... |
| Text_Content | 대사/선택지 내용 | "안녕하세요" |
| Choice_ID | 선택지 ID | C_Ino_L1_001_01 |
| Condition | 발생 조건 | Stress>30 |
| Target_NPC | 대상 NPC | 이노, 아이린 |
| Stat_Type | 변경 스탯 | Favor, Stress, Health |
| Stat_Change | 스탯 변화량 | +5, -10 |
| Flag_Set | 설정할 플래그 | Special_Ending |
| Next_Event | 다음 이벤트 ID | E_Ino_L2_001 |

### ID 네이밍 규칙

- **Dialogue_ID**: `D_{NPC코드}_L{레벨}_{이벤트번호}_{순서}`
  - 예: D_Ino_L1_001_001
- **Event_ID**: `E_{NPC코드}_{레벨}_{이벤트번호}`
  - 예: E_Ino_L1_001
- **Choice_ID**: `C_{NPC코드}_L{레벨}_{이벤트번호}_{선택지번호}`
  - 예: C_Ino_L1_001_01

### NPC 코드

| NPC 이름 | 코드 |
|----------|------|
| 이노 | Ino |
| 아이린 | Aileen |
| 카일 | Kyle |
| 리안 | Lian |

## 스탯 타입

현재 변환기가 인식하는 스탯:

- **Favor** (호감도)
- **Stress** (스트레스)
- **Health** (체력)
- **Morality** (도덕성)
- **Charm** (매력)
- **Intelligence** (지능)
- **Humor** (유머)

## 변환 예시

### 입력 (Markdown)

```markdown
#### 이벤트 1: 첫 만남
**이노:**
"안녕하세요?"

**선택지:**
- 선택지 1: "안녕" → 호감도 +5
- 선택지 2: "..." → 호감도 +3

**결과:**
- 선택지 1: 호감도 +5
- 선택지 2: 호감도 +3
```

### 출력 (CSV)

```csv
Dialogue_ID,Event_ID,NPC_Name,Level,Event_Number,Text_Type,Sequence,Text_Content,Choice_ID,Condition,Target_NPC,Stat_Type,Stat_Change,Flag_Set,Next_Event
D_Ino_L1_001_001,E_Ino_L1_001,이노,1,1,Dialogue,1,"안녕하세요?",,,,,,,
D_Ino_L1_001_C01,E_Ino_L1_001,이노,1,1,Choice,1,"안녕",C_Ino_L1_001_01,,,,,,
D_Ino_L1_001_C02,E_Ino_L1_001,이노,1,1,Choice,2,"...",C_Ino_L1_001_02,,,,,,
D_Ino_L1_001_R01,E_Ino_L1_001,이노,1,1,Result,1,,C_Ino_L1_001_01,,이노,Favor,5,,,
D_Ino_L1_001_R02,E_Ino_L1_001,이노,1,1,Result,2,,C_Ino_L1_001_02,,이노,Favor,3,,,
```

## 고급 사용법

### 수동 수정이 필요한 경우

변환기가 자동으로 처리하지 못하는 경우, CSV 파일을 직접 수정해야 합니다:

1. **다른 NPC 호감도**: `"아이린 호감도 +5"` → Target_NPC: `아이린`, Stat_Type: `Favor`
2. **특별 플래그**: `Flag_Set` 컬럼에 플래그 이름 입력
3. **복잡한 조건**: `Condition` 컬럼에 수동 입력
4. **다음 이벤트 지정**: `Next_Event` 컬럼에 Event_ID 입력

### 일괄 변환 스크립트

여러 파일을 한 번에 변환하려면:

```bash
# Windows batch
for %f in (*.md) do python npc_dialogue_converter.py %f %~nf.csv

# Linux/Mac bash
for f in *.md; do python3 npc_dialogue_converter.py "$f" "${f%.md}.csv"; done
```

## 문제 해결

### 자주 발생하는 문제

#### 1. "입력 파일을 찾을 수 없습니다"
- 파일 경로 확인
- 한글 경로 문제 시 영문 경로로 복사 후 시도

#### 2. 변환 결과가 비어있음
- Markdown 형식이 올바른지 확인
- 대사가 큰따옴표(")로 감싸져 있는지 확인
- 선택지 형식이 올바른지 확인

#### 3. 한글 깨짐
- 파일 인코딩이 UTF-8인지 확인
- CSV 파일을 Excel로 열 때 UTF-8 인코딩 선택

#### 4. 결과가 제대로 파싱되지 않음
- 결과 형식이 `선택지 N: 설명` 형식인지 확인
- 쉼표(,) 대신 다른 구분자를 사용했는지 확인

### 디버깅 모드

변환기를 수정하여 디버깅 정보를 출력하도록 할 수 있습니다:

```python
# npc_dialogue_converter.py의 parse_markdown 메서드에 추가
print(f"처리 중: {line}")  # 각 라인 출력
```

## CSV 파일 Excel에서 열기

1. Excel 실행
2. 데이터 → 외부 데이터 가져오기 → 텍스트/CSV
3. 파일 선택
4. 파일 원본: **65001 : 유니코드(UTF-8)** 선택
5. 구분 기호: **쉼표** 선택
6. 로드

## 향후 개선 사항

- [ ] YAML/JSON 출력 형식 지원
- [ ] Unity Addressables 자동 생성
- [ ] 데이터 검증 기능 (스탯 범위 체크 등)
- [ ] 다국어 자동 변환 지원
- [ ] GUI 버전 개발

---

*문서 버전: 1.0*  
*업데이트: 2026-03-25*  
*작성자: AI Assistant*
