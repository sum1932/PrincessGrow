# 🎮 디저트 왕국 시뮬레이션 프로그램

CSV 데이터를 기반으로 한 게임 시뮬레이션 프로그램입니다.

## 📁 프로젝트 구조

```
Simulation/
├── main.py                      # 메인 실행 스크립트
├── requirements.txt             # 의존성 목록
├── README.md                    # 이 파일
├── src/
│   ├── data/
│   │   └── csv_parser.py        # CSV 데이터 파서
│   ├── models/
│   │   └── game_models.py       # 게임 로직 모델
│   └── simulation/
│       ├── simulator.py         # 시뮬레이션 엔진
│       └── report_generator.py  # 결과 보고서 생성
├── results/                     # 결과 저장 디렉토리 (생성됨)
└── docs/                        # 문서 (선택적)
```

## 🚀 실행 방법

### 기본 실행

```bash
python main.py
```

### 옵션 사용

```bash
# 200회 시뮬레이션
python main.py --count 200

# 균형 잡힌 전략 사용
python main.py --strategy balanced

# 배치 크기 변경
python main.py --batch 20

# 모든 옵션 조합
python main.py --count 100 --batch 10 --strategy random --output ./my_results
```

### 옵션 설명

| 옵션 | 설명 | 기본값 |
|------|------|--------|
| `--count N` | 총 시뮬레이션 횟수 | 100 |
| `--batch N` | 배치 크기 (10회마다 기록) | 10 |
| `--strategy TYPE` | 전략 타입 (random/balanced/stress_aware) | random |
| `--output DIR` | 결과 저장 디렉토리 | ./results |
| `--data-path PATH` | CSV 데이터 경로 | ../01_GameDesign/Data |

## 📊 출력 결과

### 1. Markdown 보고서
- 파일명: `simulation_report_YYYYMMDD_HHMMSS.md`
- 전체 시뮬레이션 결과 요약
- 배치별 상세 결과
- 엔딩 분포 분석
- 개별 시뮬레이션 상세 정보

### 2. CSV 요약 파일
- 파일명: `simulation_summary_YYYYMMDD_HHMMSS.csv`
- 모든 시뮬레이션의 수치 데이터
- Excel/분석 도구로 활용 가능

## 🎲 시뮬레이션 전략

### 1. Random (기본)
- 완전 무작위로 활동 선택
- 자연스러운 분포 확인용

### 2. Balanced
- 낮은 스탯을 우선적으로 올리는 전략
- 균형 잡힌 성장 확인용

### 3. Stress Aware
- 스트레스가 높을 때 휴식 우선
- 스트레스 관리 전략 테스트

## 📈 결과 보고서 내용

### 전체 통계
- 성공적인 엔딩 수 및 성공률
- 평균 총 스탯
- 평균 스트레스 및 소지금
- 최고/최저 스탯 기록

### 엔딩 분포
- 각 엔딩별 달성 횟수
- 비율 및 순위

### 배치별 상세
- 10회 단위 통계
- 개별 시뮬레이션 결과
- NPC 호감도 변화
- 발생 이벤트 목록

## 🔧 데이터 연동

CSV 파일 경로: `../01_GameDesign/Data/`

### 연동 데이터 파일
- `Characters.csv` - 캐릭터 정보
- `Character_Stats.csv` - 스탯 정의
- `Events.csv` - 이벤트 데이터
- `Quests.csv` - 퀘스트 데이터
- `Ending_Conditions.csv` - 엔딩 조건

## 📝 예시 출력

```
======================================================================
🎮  디저트 왕국 육성 시뮬레이션
======================================================================

⚙️  설정:
  - 시뮬레이션 횟수: 100회
  - 배치 크기: 10회
  ..............................

📂 데이터 로드 중...
  ✅ 캐릭터: 5개
  ✅ 스탯: 6개
  ✅ 이벤트: 45개
  ✅ 엔딩: 12개

🎲 100회 시뮬레이션 시작...
======================================================================

[배치 1] 시뮬레이션 1 ~ 10 실행 중...
  - 시뮬레이션 5/100 완료
  - 시뮬레이션 10/100 완료
[배치 1] 완료 - 평균 스탯: 385.2
...

📊 결과 요약:
  - 실행 시간: 5.23초
  - 평균 총 스탯: 412.5
  - 엔딩 성공률: 78.0%

🏆 엔딩 분포:
  - 왕국 회복: 25회 (25.0%)
  - 디저트 마스터: 18회 (18.0%)
  ...

✅ 보고서 저장 완료: ./results/simulation_report_20260330_143022.md
✅ CSV 요약 저장 완료: ./results/simulation_summary_20260330_143022.csv
```

## ⚠️ 주의사항

1. **Python 버전**: Python 3.8 이상 필요
2. **CSV 인코딩**: UTF-8 인코딩 사용
3. **메모리**: 대량 시뮬레이션 시 메모리 사용량 주의
4. **데이터 경로**: 상대 경로 기준으로 실행

## 🔍 문제 해결

### ImportError 발생 시
```bash
# 프로젝트 루트에서 실행
python main.py

# 또는 PYTHONPATH 설정
set PYTHONPATH=%CD% && python main.py
```

### CSV 파일을 찾을 수 없을 때
```bash
# 데이터 경로 명시적 지정
python main.py --data-path "C:\\경로\\01_GameDesign\\Data"
```

## 📧 문의

문제 발생 시 프로젝트 관리자에게 문의해주세요.

---

*마지막 업데이트: 2026-03-30*
