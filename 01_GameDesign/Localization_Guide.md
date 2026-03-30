# 다국어 지원 가이드 (Localization Guide)

## 개요

게임의 대사 및 텍스트 데이터는 Excel에서 관리되며, 다국어 지원을 위한 구조로 설계되었습니다.

## 파일 구조

```
01_GameDesign/Data/
├── NPC_Dialogues_Events.csv    # 대사 이벤트 기본 정보
├── NPC_Dialogues_Text.csv      # 다국어 텍스트 데이터
└── GameData_*.xlsx             # 통합 Excel 파일
```

## 테이블 구조

### 1. NPC_Dialogues_Events

이벤트의 기본 정보를 관리하는 테이블입니다.

| 컬럼 | 설명 | 예시 |
|------|------|------|
| Dialogue_ID | 대사 이벤트 고유 ID | D_Ino_L1_001 |
| NPC_ID | NPC ID | NPC_Ino |
| NPC_Name | NPC 이름 | 이노 |
| Favor_Level | 호감도 레벨 | 1 |
| Event_Order | 이벤트 순서 | 1 |
| Event_Name | 이벤트명 | 인사이동 첫날 |
| Condition_Type | 발생 조건 타입 | Story/Turn/Random/Age/Date |
| Condition_Value | 조건 값 | GameStart/Age=6/Date=1224 |
| CG_Variation | CG 변형 | CG_Ino_Normal |
| BGM_ID | 배경음악 ID | BGM_Prologue |
| Next_Dialogue_ID | 다음 대사 ID | D_Ino_L1_002 |
| Chapter_Note | 비고 | 첫 만남 시 자동 발생 |

### 2. NPC_Dialogues_Text

실제 대사 텍스트를 다국어로 관리하는 테이블입니다.

| 컬럼 | 설명 | 예시 |
|------|------|------|
| Text_ID | 텍스트 고유 ID | T_Ino_L1_001_001_KO |
| Dialogue_ID | 연결된 이벤트 ID | D_Ino_L1_001 |
| Language | 언어 코드 | KO/EN/JP/CN |
| Text_Type | 텍스트 타입 | Dialogue/Choice/Result |
| Sequence | 순서 | 1, 2, 3... |
| Text_Content | 실제 텍스트 | "안녕하세요?" |
| Voice_Path | 음성 파일 경로 | Voice/Ino/L1_001_001.wav |
| Note | 비고 | 첫 인사 |

## 언어 코드

| 코드 | 언어 | 파일명 접미사 |
|------|------|--------------|
| KO | 한국어 | _ko |
| EN | English | _en |
| JP | 日本語 | _jp |
| CN | 简体中文 | _cn |
| TW | 繁體中文 | _tw |
| FR | Français | _fr |
| DE | Deutsch | _de |
| ES | Español | _es |

## Text_Type 분류

| 타입 | 설명 | 예시 |
|------|------|------|
| Dialogue | NPC/캐릭터 대사 | "어, 어... 안녕하세요?" |
| Choice | 플레이어 선택지 | "고마워요..." / "...정말 괜찮으세요?" |
| Result | 선택 결과 설명 | "호감도 +3, 스트레스 -5" |

## Excel에서 데이터 관리 방법

### 1. 새로운 언어 추가하기

1. `NPC_Dialogues_Text` 시트 열기
2. 기존 언어(KO) 행을 복사
3. Language 컬럼을 새 언어 코드로 변경 (예: JP)
4. Text_Content를 일본어로 번역
5. Voice_Path는 해당 언어 음성 파일 경로로 설정

### 2. 새로운 대사 이벤트 추가하기

**Step 1: Events 테이블에 이벤트 등록**
```
Dialogue_ID: D_Ino_L2_005
NPC_ID: NPC_Ino
Favor_Level: 2
Event_Order: 5
Event_Name: 새로운 이벤트
Condition_Type: Age
Condition_Value: Age=11
```

**Step 2: Text 테이블에 다국어 텍스트 등록**
```
Text_ID: T_Ino_L2_005_001_KO
Dialogue_ID: D_Ino_L2_005
Language: KO
Text_Type: Dialogue
Sequence: 1
Text_Content: "새로운 대사입니다"
```

### 3. 필터링하여 작업하기

Excel에서 Language 컬럼으로 필터링하여 특정 언어만 작업:
```
Language = "KO"  (한국어만 표시)
Language = "EN"  (영어만 표시)
```

## 게임 엔진 연동

### 데이터 로드 예시 (Unity C#)

```csharp
public class DialogueManager
{
    // Dialogue Events 로드
    public Dictionary<string, DialogueEvent> LoadDialogueEvents()
    {
        var events = new Dictionary<string, DialogueEvent>();
        // CSV/Excel 파싱
        return events;
    }
    
    // 특정 언어의 대사 로드
    public string GetDialogueText(string dialogueId, string language, int sequence)
    {
        // SELECT Text_Content FROM NPC_Dialogues_Text 
        // WHERE Dialogue_ID = dialogueId 
        // AND Language = language 
        // AND Sequence = sequence
        return text;
    }
    
    // 선택지 로드
    public List<Choice> GetChoices(string dialogueId, string language)
    {
        // SELECT * FROM NPC_Dialogues_Text 
        // WHERE Dialogue_ID = dialogueId 
        // AND Language = language 
        // AND Text_Type = 'Choice'
        return choices;
    }
}
```

### JSON 변환 예시

Excel → JSON 변환 시 구조:
```json
{
  "dialogues": [
    {
      "dialogue_id": "D_Ino_L1_001",
      "npc_id": "NPC_Ino",
      "favor_level": 1,
      "texts": {
        "KO": {
          "dialogues": ["어, 어... 안녕하세요?", "저는 이노라고 합니다."],
          "choices": [
            {"text": "고마워요...", "result": "호감도 +3"},
            {"text": "...정말 괜찮으세요?", "result": "호감도 +5"}
          ]
        },
        "EN": {
          "dialogues": ["Um, um... Hello?", "I'm Ino."],
          "choices": [
            {"text": "Thank you...", "result": "Favor +3"},
            {"text": "...Are you really okay?", "result": "Favor +5"}
          ]
        }
      }
    }
  ]
}
```

## 작업 체크리스트

### 번역 작업 순서

- [ ] 1. 한국어(KO) 원문 작성 완료
- [ ] 2. 영어(EN) 번역
- [ ] 3. 일본어(JP) 번역
- [ ] 4. 중국어(CN/TW) 번역
- [ ] 5. 음성 녹음 (해당 언어)
- [ ] 6. 게임 내 테스트
- [ ] 7. 폰트 및 UI 레이아웃 확인

### 품질 관리

- [ ] 모든 언어의 길이 확인 (UI 벗어남 방지)
- [ ] 음성 파일 경로 확인
- [ ] 특수문자/이모지 지원 확인
- [ ] RTL(Right-to-Left) 언어 지원 (아랍어 등)

## 주의사항

1. **Text_ID 규칙**: T_{NPC}_{레벨}_{이벤트번호}_{시퀀스}_{언어}
   - 예: T_Ino_L1_001_001_KO

2. **CSV 인코딩**: UTF-8 with BOM 사용

3. **줄바꿈**: Excel 셀 내 줄바꿈은 Alt+Enter, CSV에서는 \n 사용

4. **특수문자": JSON 호환을 위해 큰따옴표는 "{ }" 형태로 이스케이프

## 버전 관리

| 버전 | 날짜 | 변경사항 | 작업자 |
|------|------|----------|--------|
| 1.0 | 2026-03-25 | 초기 한국어/영어 데이터 생성 | - |
| 1.1 | - | 일본어 추가 | - |
| 1.2 | - | 중국어 추가 | - |

---

*문서 버전: 1.0*
*업데이트: 2026-03-25*
