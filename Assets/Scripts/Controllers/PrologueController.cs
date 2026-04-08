using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Data;
using DessertKingdom.Core.Domain;
using DessertKingdom.Views;
using GameData.ScriptableObjects;
using UnityEngine;
using EventType = DessertKingdom.Core.Domain.EventType;

namespace DessertKingdom.Controllers
{
    /// <summary>
    /// 프로로그 시스템 컨트롤러
    /// 게임 시작 시 프로로그 이벤트를 관리
    /// </summary>
    public class PrologueController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueEventView dialogueEventView;
        [SerializeField] private DialogueDatabase dialogueDatabase;

        // 이벤트
        public event Action OnPrologueStarted;
        public event Action OnPrologueCompleted;
        public event Action OnPrologueEventTriggered;

        // 상태
        private GameState _gameState;
        private IEventRepository _eventRepo;
        private bool _isExecuting = false;

        /// <summary>
        /// 프로로그 컨트롤러 초기화
        /// </summary>
        public void Initialize(GameState gameState, IEventRepository eventRepo)
        {
            _gameState = gameState;
            _eventRepo = eventRepo;
            
            // DialogueDatabase 초기화
            if (dialogueDatabase == null)
            {
                dialogueDatabase = Resources.Load<DialogueDatabase>("GameData/DialogueDatabase");
                if (dialogueDatabase == null)
                {
                    Debug.LogWarning("[PrologueController] DialogueDatabase를 Resources에서 찾을 수 없습니다.");
                }
            }
            
            dialogueDatabase?.Initialize();
        }

        /// <summary>
        /// 프로로그 이벤트 체크 및 실행
        /// </summary>
        public IEnumerator CheckAndExecutePrologue()
        {
            if (_isExecuting)
            {
                Debug.LogWarning("[PrologueController] 프로로그가 이미 실행 중입니다.");
                yield break;
            }

            if (_gameState == null)
            {
                Debug.LogError("[PrologueController] GameState가 초기화되지 않았습니다.");
                yield break;
            }

            // 프로로그 이미 표시됨
            if (_gameState.PrologueShown)
            {
                Debug.Log("[PrologueController] 프로로그가 이미 표시되었습니다.");
                yield break;
            }

            // PrologueDatabase에서 프로로그 이벤트 로드
            List<GameEvent> prologueEvents = LoadPrologueEvents();

            if (prologueEvents.Count == 0)
            {
                Debug.LogWarning("[PrologueController] 프로로그 이벤트를 찾을 수 없습니다.");
                _gameState.PrologueShown = true;
                yield break;
            }

            _isExecuting = true;
            OnPrologueStarted?.Invoke();

            Debug.Log($"[PrologueController] 프로로그 이벤트 {prologueEvents.Count}개 발견");

            // 프로로그 이벤트 순차 실행
            foreach (var prologueEvent in prologueEvents)
            {
                if (prologueEvent.CanTrigger(_gameState))
                {
                    yield return StartCoroutine(ExecutePrologueEvent(prologueEvent));
                }
            }

            // 프로로그 완료 표시
            _gameState.PrologueShown = true;
            _isExecuting = false;

            Debug.Log("[PrologueController] 프로로그 실행 완료");
            OnPrologueCompleted?.Invoke();
        }

        /// <summary>
        /// 프로로그 이벤트 로드 (Prologue.csv 또는 EventRepository)
        /// </summary>
        private List<GameEvent> LoadPrologueEvents()
        {
            var events = new List<GameEvent>();
            
            // 1. 먼저 Prologue.csv에서 직접 로드
            var csvEvents = LoadPrologueFromCsv();
            if (csvEvents.Count > 0)
            {
                Debug.Log($"[PrologueController] Prologue.csv에서 {csvEvents.Count}개 이벤트 로드");
                events.AddRange(csvEvents);
            }
            
            // 2. EventRepository에서 Prologue 타입 이벤트도 검색 (추가 프로로그 이벤트)
            if (_eventRepo != null)
            {
                var repoEvents = _eventRepo.GetAll()
                    .Where(e => e.Type == EventType.Prologue)
                    .ToList();
                
                if (repoEvents.Count > 0)
                {
                    Debug.Log($"[PrologueController] EventRepository에서 {repoEvents.Count}개 Prologue 이벤트 로드");
                    events.AddRange(repoEvents);
                }
            }
            
            // 중복 제거 및 우선순위 정렬
            return events
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .OrderByDescending(e => e.Priority)
                .ToList();
        }
        
        /// <summary>
        /// Prologue.csv 파일에서 프로로그 이벤트 로드
        /// </summary>
        private List<GameEvent> LoadPrologueFromCsv()
        {
            var events = new List<GameEvent>();
            
            try
            {
                // Resources에서 Prologue.csv 로드
                TextAsset csvFile = Resources.Load<TextAsset>("Data/Prologue");
                
                if (csvFile == null)
                {
                    Debug.LogWarning("[PrologueController] Prologue.csv 파일을 찾을 수 없습니다. (Resources/Data/Prologue.csv)");
                    return events;
                }
                
                string[] lines = csvFile.text.Split('\n');
                if (lines.Length < 2)
                {
                    Debug.LogWarning("[PrologueController] Prologue.csv에 데이터가 없습니다.");
                    return events;
                }
                
                // 헤더 파싱
                string[] headers = lines[0].Trim().Split(',');
                
                // 데이터 행 파싱
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;
                    
                    string[] values = line.Split(',');
                    if (values.Length < 3)
                        continue;
                    
                    var gameEvent = ParsePrologueEvent(values, headers);
                    if (gameEvent != null)
                    {
                        events.Add(gameEvent);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PrologueController] Prologue.csv 로드 중 오류: {ex.Message}");
            }
            
            return events;
        }
        
        /// <summary>
        /// CSV 행을 GameEvent로 파싱 (JSON 대사 데이터 포함)
        /// </summary>
        private GameEvent ParsePrologueEvent(string[] values, string[] headers)
        {
            try
            {
                string eventId = values[0].Trim();
                string name = values[1].Trim();
                string typeStr = values[2].Trim();
                string scriptPath = values.Length > 12 ? values[12].Trim() : "";
                string description = values.Length > 19 ? values[19].Trim() : "";
                int priority = 0;
                if (values.Length > 18 && !string.IsNullOrEmpty(values[18]))
                    int.TryParse(values[18].Trim(), out priority);
                bool isRepeatable = values.Length > 16 && values[16].Trim().ToLower() == "true";
                bool isHidden = values.Length > 17 && values[17].Trim().ToLower() == "true";
                
                // EventType 파싱
                EventType eventType = ParseEventType(typeStr);
                
                // 조건 딕셔너리 생성
                var conditions = new Dictionary<string, object>();
                
                // 선택지, 효과는 비어있는 리스트/딕셔너리로 초기화
                var choices = new List<EventChoice>();
                var statEffects = new Dictionary<StatType, int>();
                var favorEffects = new Dictionary<string, int>();
                
                // DialogueDatabase에서 대사 데이터 로드
                EventDialogue dialogue = null;
                if (dialogueDatabase != null)
                {
                    var dialogueData = dialogueDatabase.Get(eventId);
                    if (dialogueData != null)
                    {
                        var result = ConvertDialogueData(dialogueData);
                        dialogue = result.dialogue;
                        
                        // DialogueDatabase에서 선택지도 로드
                        if (result.choices != null && result.choices.Count > 0)
                        {
                            choices = result.choices;
                        }
                        
                        Debug.Log($"[PrologueController] DialogueDatabase에서 대사 로드: {eventId} ({dialogue?.Lines?.Count ?? 0}줄)");
                    }
                    else
                    {
                        Debug.LogWarning($"[PrologueController] DialogueDatabase에서 {eventId}를 찾을 수 없습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("[PrologueController] DialogueDatabase가 연결되지 않았습니다.");
                }
                
                var gameEvent = new GameEvent(
                    eventId,
                    name,
                    eventType,
                    priority,
                    description,
                    conditions,
                    choices,
                    statEffects,
                    favorEffects,
                    !isRepeatable,  // IsOneTime = !IsRepeatable
                    isHidden,
                    null,           // RequiredPreviousEvent
                    dialogue        // ★ 대사 데이터 포함
                );
                
                Debug.Log($"[PrologueController] 프로로그 이벤트 파싱 완료: {eventId} - {name} (대사: {dialogue?.Lines?.Count ?? 0}줄)");
                
                return gameEvent;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PrologueController] 이벤트 파싱 중 오류: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// DialogueData를 EventDialogue와 EventChoice 리스트로 변환
        /// </summary>
        private (EventDialogue dialogue, List<EventChoice> choices) ConvertDialogueData(DialogueData data)
        {
            if (data == null) return (null, null);
            
            // 대사 라인 변환
            var lines = new List<EventDialogueLine>();
            if (data.Lines != null)
            {
                foreach (var lineData in data.Lines)
                {
                    var line = new EventDialogueLine(lineData.Text);
                    lines.Add(line);
                }
            }
            
            // 선택지 변환
            var choices = new List<EventChoice>();
            if (data.Choices != null)
            {
                for (int i = 0; i < data.Choices.Count; i++)
                {
                    var choiceData = data.Choices[i];
                    var choice = new EventChoice(
                        $"choice_{i}",
                        choiceData.Text,
                        null,  // statEffects
                        null,  // favorEffects
                        new Dictionary<string, object>()  // requirements
                    );
                    choices.Add(choice);
                }
            }
            
            // EventDialogue 생성 (첫 번째 라인의 릭터 정보 사용)
            string characterId = "Narrator";
            string characterName = "";
            string portraitPath = "";
            
            if (data.Lines != null && data.Lines.Count > 0)
            {
                var firstLine = data.Lines[0];
                characterId = firstLine.CharacterId ?? "Narrator";
                characterName = firstLine.CharacterName ?? "";
                portraitPath = firstLine.PortraitPath ?? "";
            }
            
            var dialogue = new EventDialogue(
                characterId,
                characterName,
                portraitPath,
                lines,
                false,
                ""
            );
            
            return (dialogue, choices);
        }
        
        /// <summary>
        /// 이벤트 타입 문자열을 EventType enum으로 변환
        /// </summary>
        private EventType ParseEventType(string typeStr)
        {
            if (string.IsNullOrEmpty(typeStr)) return EventType.Fixed;
            
            switch (typeStr.ToLower())
            {
                case "fixed": return EventType.Fixed;
                case "random": return EventType.Random;
                case "npc": return EventType.NPC;
                case "hidden": return EventType.Hidden;
                case "prologue": return EventType.Prologue;
                case "special": return EventType.Special;
                default: return EventType.Fixed;
            }
        }

        /// <summary>
        /// 프로로그 이벤트 실행
        /// </summary>
        private IEnumerator ExecutePrologueEvent(GameEvent prologueEvent)
        {
            Debug.Log($"[PrologueController] 프로로그 이벤트 실행: {prologueEvent.Name}");
            Debug.Log($"[PrologueController] DialogueEventView 연결 상태: {(dialogueEventView != null ? "OK" : "NULL")}");
            Debug.Log($"[PrologueController] 대사 라인 수: {prologueEvent.Dialogue?.Lines?.Count ?? 0}");

            OnPrologueEventTriggered?.Invoke();

            if (dialogueEventView != null)
            {
                dialogueEventView.ShowEventDialogue(prologueEvent);

                bool eventComplete = false;
                Action onComplete = () => eventComplete = true;
                dialogueEventView.OnDialogueCompleted += onComplete;

                yield return new WaitUntil(() => eventComplete);

                dialogueEventView.OnDialogueCompleted -= onComplete;

                // 이벤트 완료 처리
                _gameState.CompleteEvent(prologueEvent.Id);
            }
            else
            {
                Debug.LogWarning("[PrologueController] DialogueEventView가 연결되지 않았습니다.");
                // 1초 대기 후 종료
                yield return new WaitForSecondsRealtime(1f);
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }

        /// <summary>
        /// 프로로그 강제 완료
        /// </summary>
        public void ForceComplete()
        {
            if (_gameState != null)
            {
                _gameState.PrologueShown = true;
            }
            _isExecuting = false;
            OnPrologueCompleted?.Invoke();
        }

        /// <summary>
        /// 프로로그 실행 중인지 확인
        /// </summary>
        public bool IsExecuting => _isExecuting;

        /// <summary>
        /// 프로로그가 이미 표시되었는지 확인
        /// </summary>
        public bool HasShown => _gameState?.PrologueShown ?? false;
    }
}
