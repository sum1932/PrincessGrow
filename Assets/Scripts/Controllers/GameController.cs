using System;
using System.Collections.Generic;
using DessertKingdom.Core.Domain;
using DessertKingdom.Core.Services;
using DessertKingdom.Core.Data;
using DessertKingdom.Adapters.Interfaces;
using DessertKingdom.Adapters.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DessertKingdom.Controllers
{
    /// <summary>
    /// 게임의 메인 컨트롤러
    /// Core 로직과 Unity View를 연결
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityGamePresenter gamePresenter;
        [SerializeField] private UnityGameInput gameInput;

        [Header("Data")]
        [SerializeField] private bool useCsvData = true;
        
        // Repository
        private IActivityRepository _activityRepo;
        private IEventRepository _eventRepo;
        private IEndingRepository _endingRepo;

        // Core 의존성
        private GameState _gameState;
        private ITurnManager _turnManager;
        private IEventManager _eventManager;
        private IEndingJudge _endingJudge;

        // 어댑터
        private IGamePresenter _presenter;
        private IGameInput _input;

        public static GameController Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            InitializeCore();
            InitializeAdapters();
            SubscribeToEvents();
            StartNewGame();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region 초기화

        private void InitializeCore()
        {
            // Repository 초기화
            if (useCsvData)
            {
                _activityRepo = new CsvActivityRepository();
                _eventRepo = new CsvEventRepository();
                _endingRepo = new CsvEndingRepository();
                Debug.Log("CSV 데이터 로드 완료");
            }
            else
            {
                // 샘플 데이터 사용
                var (activities, events, endingConditions) = LoadSampleData();
                _activityRepo = new ListActivityRepository(activities);
                _eventRepo = new ListEventRepository(events);
                _endingRepo = new ListEndingRepository(endingConditions);
            }
            
            // 게임 상태 생성
            _gameState = new GameState();
            
            // 서비스 생성
            _eventManager = new EventManager(_eventRepo.GetAll());
            _turnManager = new TurnManager(_gameState, _eventManager);
            _endingJudge = new EndingJudge(_endingRepo.GetAll());
        }

        private void InitializeAdapters()
        {
            _presenter = gamePresenter ?? GetComponent<UnityGamePresenter>();
            _input = gameInput ?? GetComponent<UnityGameInput>();

            if (_presenter == null)
            {
                Debug.LogError("GamePresenter가 설정되지 않았습니다!");
            }
            if (_input == null)
            {
                Debug.LogError("GameInput이 설정되지 않았습니다!");
            }
        }

        private void SubscribeToEvents()
        {
            // Core 이벤트 구독
            if (_turnManager != null)
            {
                _turnManager.OnTurnStarted += OnTurnStarted;
                _turnManager.OnTurnEnded += OnTurnEnded;
                _turnManager.OnTurnProcessed += OnTurnProcessed;
            }

            // 입력 이벤트 구독
            if (_input != null)
            {
                _input.OnActivitySelected += OnActivitySelected;
                _input.OnChoiceSelected += OnChoiceSelected;
                _input.OnCommand += OnCommand;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_turnManager != null)
            {
                _turnManager.OnTurnStarted -= OnTurnStarted;
                _turnManager.OnTurnEnded -= OnTurnEnded;
                _turnManager.OnTurnProcessed -= OnTurnProcessed;
            }

            if (_input != null)
            {
                _input.OnActivitySelected -= OnActivitySelected;
                _input.OnChoiceSelected -= OnChoiceSelected;
                _input.OnCommand -= OnCommand;
            }
        }

        #endregion

        #region 게임 흐름

        private void StartNewGame()
        {
            _gameState = new GameState();
            _gameState.SetPhase(GamePhase.ScheduleSelection);

            // UI 업데이트
            UpdateAllUI();
            _presenter?.DisplayScheduleSelection();

            Debug.Log("게임 시작!");
        }

        public void EndTurn()
        {
            if (_turnManager == null) return;

            var result = _turnManager.EndTurn();
            
            if (result.IsEnding)
            {
                ShowEnding();
            }
            else if (!result.Success)
            {
                _presenter?.ShowMessage(result.Message);
            }
        }

        public bool CanEndTurn()
        {
            return _turnManager?.CanEndTurn() ?? false;
        }

        private void ShowEnding()
        {
            if (_endingJudge == null) return;

            var ending = _endingJudge.EvaluateEnding(_gameState);
            _presenter?.DisplayEnding(ending);
            _gameState.SetPhase(GamePhase.Ending);

            Debug.Log($"엔딩 달성: {ending.Name}");
        }

        #endregion

        #region 이벤트 핸들러

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            Debug.Log($"턴 시작: {evt.Age}세 {evt.Month}월");
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            Debug.Log($"턴 종료: {evt.Turn}턴");
        }

        private void OnTurnProcessed(TurnResult result)
        {
            if (result.Success)
            {
                UpdateAllUI();
                
                // 이벤트 발생
                if (result.TriggeredEvents != null && result.TriggeredEvents.Count > 0)
                {
                    foreach (var evt in result.TriggeredEvents)
                    {
                        _presenter?.DisplayEvent(evt);
                        Debug.Log($"이벤트 발생: {evt.Name}");
                    }
                }
            }
        }

        private void OnActivitySelected(ActivitySelectedEvent evt)
        {
            if (_turnManager == null) return;

            // 현재 슬롯에 활동 설정
            if (evt.Slot == 1)
            {
                // 첫 번째 활동
                if (_gameState.CurrentSchedule.Activity1 == null)
                {
                    _gameState.CurrentSchedule.SetActivity(evt.Activity, 1);
                }
            }
            else if (evt.Slot == 2)
            {
                // 두 번째 활동
                if (_gameState.CurrentSchedule.Activity2 == null)
                {
                    _gameState.CurrentSchedule.SetActivity(evt.Activity, 2);
                }
            }

            Debug.Log($"활동 선택됨: {evt.Activity.Name} (슬롯 {evt.Slot})");
            UpdateAllUI();
        }

        private void OnChoiceSelected(EventChoiceSelectedEvent evt)
        {
            if (_eventManager == null) return;

            _eventManager.ProcessEventChoice(evt.GameEvent, evt.Choice, _gameState);
            Debug.Log($"선택지 선택됨: {evt.Choice.Text}");
            
            UpdateAllUI();
        }

        private void OnCommand(CommandEvent evt)
        {
            switch (evt.Type)
            {
                case CommandType.Save:
                    SaveGame();
                    break;
                case CommandType.Load:
                    LoadGame();
                    break;
                case CommandType.NewGame:
                    StartNewGame();
                    break;
                case CommandType.Quit:
                    QuitGame();
                    break;
            }
        }

        #endregion

        #region UI 업데이트

        private void UpdateAllUI()
        {
            if (_presenter == null) return;

            _presenter.DisplayTurn(_gameState.Turn);
            _presenter.DisplayStats(_gameState.Character);
            _presenter.DisplayEconomy(_gameState.Economy);
        }

        #endregion

        #region 저장/로드

        private void SaveGame()
        {
            string data = _gameState.Serialize();
            PlayerPrefs.SetString("GameSave", data);
            PlayerPrefs.Save();
            Debug.Log("게임 저장 완료");
            _presenter?.ShowMessage("게임이 저장되었습니다.");
        }

        private void LoadGame()
        {
            if (PlayerPrefs.HasKey("GameSave"))
            {
                string data = PlayerPrefs.GetString("GameSave");
                _gameState = GameState.Deserialize(data);
                Debug.Log("게임 로드 완료");
                UpdateAllUI();
                _presenter?.ShowMessage("게임을 불러왔습니다.");
            }
            else
            {
                _presenter?.ShowMessage("저장된 데이터가 없습니다.");
            }
        }

        private void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        #endregion

        #region 데이터 로드

        private (List<Activity> activities, List<GameEvent> events, List<EndingCondition> endings) LoadSampleData()
        {
            var activities = new List<Activity>();
            var events = new List<GameEvent>();
            var endings = new List<EndingCondition>();

            // 샘플 활동 데이터
            activities.Add(new Activity(
                "hp_training", "체력 단련", ActivityType.Lesson,
                200, 0, 6,
                new Dictionary<StatType, int> { { StatType.HP, 15 }, { StatType.Charm, -3 } },
                10, "체력을 단련하는 수업"
            ));

            activities.Add(new Activity(
                "charm_lesson", "매력 수업", ActivityType.Lesson,
                300, 0, 6,
                new Dictionary<StatType, int> { { StatType.Charm, 20 }, { StatType.HP, -3 } },
                8, "외모와 태도를 가꾸는 수업"
            ));

            activities.Add(new Activity(
                "study", "학업 공부", ActivityType.Lesson,
                250, 0, 6,
                new Dictionary<StatType, int> { { StatType.Intelligence, 15 }, { StatType.Art, -3 } },
                12, "지식을 쌓는 수업"
            ));

            activities.Add(new Activity(
                "art_lesson", "예술 수업", ActivityType.Lesson,
                350, 0, 6,
                new Dictionary<StatType, int> { { StatType.Art, 15 }, { StatType.Intelligence, -3 } },
                10, "감성을 키우는 수업"
            ));

            activities.Add(new Activity(
                "convenience", "편의점 아르바이트", ActivityType.PartTime,
                0, 150, 10,
                new Dictionary<StatType, int> { { StatType.Intelligence, 5 }, { StatType.Morality, 2 } },
                5, "편의점에서 일합니다"
            ));

            activities.Add(new Activity(
                "cafe", "카페 아르바이트", ActivityType.PartTime,
                0, 180, 10,
                new Dictionary<StatType, int> { { StatType.Charm, 8 } },
                10, "카페에서 일합니다"
            ));

            activities.Add(new Activity(
                "rest_home", "집에서 쉬기", ActivityType.Rest,
                0, 0, 6,
                new Dictionary<StatType, int>(),
                -30, "집에서 편하게 쉽니다"
            ));

            activities.Add(new Activity(
                "outing_ino", "이노 만나기", ActivityType.Outing,
                50, 0, 6,
                new Dictionary<StatType, int>(),
                -10, "이노와 시간을 보냅니다"
            ));

            return (activities, events, endings);
        }

        #endregion

        #region 공개 API

        public GameState GetGameState() => _gameState;
        public List<Activity> GetAvailableActivities() => _activityRepo?.GetAll() ?? new List<Activity>();
        public List<Activity> GetAvailableActivitiesForAge(int age) => _activityRepo?.GetAvailable(age) ?? new List<Activity>();
        public ITurnManager GetTurnManager() => _turnManager;
        public IEventManager GetEventManager() => _eventManager;

        #endregion
    }
}
