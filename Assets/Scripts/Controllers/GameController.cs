using System;
using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using DessertKingdom.Core.Services;
using DessertKingdom.Core.Data;
using GameData.ScriptableObjects;
using DessertKingdom.Adapters.Interfaces;
using DessertKingdom.Adapters.Unity;
using DessertKingdom.Views;
using DG.Tweening;
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

        [Header("Panel References")]
        [SerializeField] private GameObject monthlySchedulePanel;
        [SerializeField] private GameObject activityPanel;
        [SerializeField] private GameObject statsPanel;  // 스탯 패널
        [SerializeField] private GameObject inventoryPanel;  // 인벤토리 패널
        [SerializeField] private GameObject shopPanel;  // 상점 패널

        [Header("Event System")]
        [SerializeField] private DialogueEventView dialogueEventView;
        [SerializeField] private ScheduleExecutionPopup scheduleExecutionPopup;
        [SerializeField] private PrologueController prologueController;
        [SerializeField] private NameSettingView nameSettingView;

        [Header("Data")]
        [SerializeField] private bool useDatabaseData = true;
        [SerializeField] private ActionDatabase actionDatabase;
        [SerializeField] private EventDatabase eventDatabase;
        [SerializeField] private EndingDatabase endingDatabase;
        
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
            Debug.Log("[GameController] InitializeCore 시작");
            Debug.Log($"[GameController] actionDatabase: {(actionDatabase != null ? actionDatabase.name : "NULL")}");
            Debug.Log($"[GameController] eventDatabase: {(eventDatabase != null ? eventDatabase.name : "NULL")}");
            Debug.Log($"[GameController] endingDatabase: {(endingDatabase != null ? endingDatabase.name : "NULL")}");
            Debug.Log($"[GameController] useDatabaseData: {useDatabaseData}");
            
            // Database가 Inspector에서 연결되지 않은 경우 Resources에서 로드 시도
            if (useDatabaseData && (actionDatabase == null || eventDatabase == null || endingDatabase == null))
            {
                Debug.Log("[GameController] Inspector에서 Database를 찾을 수 없어 Resources에서 로드 시도");
                
                if (actionDatabase == null)
                    actionDatabase = Resources.Load<ActionDatabase>("GameData/ActionDatabase");
                if (eventDatabase == null)
                    eventDatabase = Resources.Load<EventDatabase>("GameData/EventDatabase");
                if (endingDatabase == null)
                    endingDatabase = Resources.Load<EndingDatabase>("GameData/EndingDatabase");
                
                Debug.Log($"[GameController] Resources 로드 후 - actionDatabase: {(actionDatabase != null ? "OK" : "NULL")}");
                Debug.Log($"[GameController] Resources 로드 후 - eventDatabase: {(eventDatabase != null ? "OK" : "NULL")}");
                Debug.Log($"[GameController] Resources 로드 후 - endingDatabase: {(endingDatabase != null ? "OK" : "NULL")}");
            }
            
            // Repository 초기화
            if (useDatabaseData && actionDatabase != null && eventDatabase != null && endingDatabase != null)
            {
                // Database ScriptableObject 사용
                _activityRepo = new DatabaseActionRepository(actionDatabase);
                _eventRepo = new DatabaseEventRepository(eventDatabase);
                _endingRepo = new DatabaseEndingRepository(endingDatabase);
                
                Debug.Log("[GameController] Database ScriptableObject 데이터 로드 완료");
                Debug.Log($"[GameController] 활동 수: {_activityRepo.GetAll().Count}");
                Debug.Log($"[GameController] 이벤트 수: {_eventRepo.GetAll().Count}");
                Debug.Log($"[GameController] 엔딩 수: {_endingRepo.GetAll().Count}");
            }
            else if (useDatabaseData)
            {
                Debug.LogWarning("[GameController] Database가 연결되지 않았습니다. Inspector에서 Database를 연결하세요.");
                // 샘플 데이터 사용
                var (activities, events, endingConditions) = LoadSampleData();
                _activityRepo = new ListActivityRepository(activities);
                _eventRepo = new ListEventRepository(events);
                _endingRepo = new ListEndingRepository(endingConditions);
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
            
            // PrologueController 초기화
            if (prologueController == null)
            {
                prologueController = GetComponent<PrologueController>();
                if (prologueController == null)
                {
                    prologueController = gameObject.AddComponent<PrologueController>();
                }
            }
            prologueController.Initialize(_gameState, _eventRepo);
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

            // MonthlyScheduleView 이벤트 구독
            if (gamePresenter?.monthlyScheduleView != null)
            {
                gamePresenter.monthlyScheduleView.OnSlotSelected += OnScheduleSlotSelected;
                gamePresenter.monthlyScheduleView.OnProceedClicked += OnScheduleProceed;
                gamePresenter.monthlyScheduleView.OnResetClicked += OnScheduleReset;
            }

            // ActivityPanelView 이벤트 구독 ★추가
            if (gamePresenter?.activityPanelView != null)
            {
                gamePresenter.activityPanelView.OnActivitySelected += OnActivityPanelActivitySelected;
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

            // MonthlyScheduleView 이벤트 구독 해제
            if (gamePresenter?.monthlyScheduleView != null)
            {
                gamePresenter.monthlyScheduleView.OnSlotSelected -= OnScheduleSlotSelected;
                gamePresenter.monthlyScheduleView.OnProceedClicked -= OnScheduleProceed;
                gamePresenter.monthlyScheduleView.OnResetClicked -= OnScheduleReset;
            }

            // ActivityPanelView 이벤트 구독 해제 ★추가
            if (gamePresenter?.activityPanelView != null)
            {
                gamePresenter.activityPanelView.OnActivitySelected -= OnActivityPanelActivitySelected;
            }
        }

        #endregion

        #region 게임 흐름

        private void StartNewGame()
        {
            // ★★★ 중요: _gameState를 새로 생성하면 TurnManager와 다른 인스턴스가 됨
            // 그래서 TurnManager도 새로 생성해야 함
            _gameState = new GameState();
            
            // ★ TurnManager도 새로 생성하여 같은 _gameState 참조하도록 설정
            // 기존 이벤트 구독 해제 (중복 방지)
            UnsubscribeFromEvents();
            _turnManager = new TurnManager(_gameState, _eventManager);
            
            // 이벤트 다시 구독 (새로운 _turnManager에 대해)
            SubscribeToEvents();
            
            // 이름 설정 단계로 시작
            _gameState.SetPhase(GamePhase.NameSetting);

            // UI 업데이트
            UpdateAllUI();
            
            // PrologueController 재초기화 (새로운 _gameState 사용)
            prologueController.Initialize(_gameState, _eventRepo);
            
            // ★ 이름 설정 시작
            StartCoroutine(ExecuteNameSettingSequence());
        }

        /// <summary>
        /// 이름 설정 시퀀스 실행
        /// </summary>
        private System.Collections.IEnumerator ExecuteNameSettingSequence()
        {
            Debug.Log("[GameController] 이름 설정 단계 시작");

            if (nameSettingView != null)
            {
                bool nameSettingComplete = false;
                string playerName = null;
                string targetName = null;

                // 이름 설정 완료 콜백
                System.Action<string, string> onNamesConfirmed = (pName, tName) =>
                {
                    playerName = pName;
                    targetName = tName;
                    nameSettingComplete = true;
                };

                // 건너뛰기 콜백
                System.Action onNamesSkipped = () =>
                {
                    nameSettingComplete = true;
                };

                nameSettingView.OnNamesConfirmed += onNamesConfirmed;
                nameSettingView.OnNamesSkipped += onNamesSkipped;

                // 이름 설정 UI 표시
                nameSettingView.Show();

                // 이름 설정 완료 대기
                yield return new WaitUntil(() => nameSettingComplete);

                // 콜백 해제
                nameSettingView.OnNamesConfirmed -= onNamesConfirmed;
                nameSettingView.OnNamesSkipped -= onNamesSkipped;

                // 이름 저장
                if (!string.IsNullOrEmpty(playerName))
                {
                    _gameState.PlayerName = playerName;
                    Debug.Log($"[GameController] 플레이어 이름 설정: {playerName}");
                }

                if (!string.IsNullOrEmpty(targetName))
                {
                    _gameState.TargetName = targetName;
                    Debug.Log($"[GameController] 육성대상 이름 설정: {targetName}");
                }
            }
            else
            {
                Debug.LogWarning("[GameController] NameSettingView가 연결되지 않았습니다. 기본 이름을 사용합니다.");
            }

            // 프로로그로 진행
            Debug.Log("[GameController] 프로로그 단계로 진행");
            StartCoroutine(ExecutePrologueSequence());
        }

        /// <summary>
        /// 프로로그 시퀀스 실행 (PrologueController 사용)
        /// </summary>
        private System.Collections.IEnumerator ExecutePrologueSequence()
        {
            // PrologueController에 콜백 등록
            bool prologueCompleted = false;
            System.Action onPrologueComplete = () => prologueCompleted = true;
            prologueController.OnPrologueCompleted += onPrologueComplete;
            
            // 프로로그 실행 시작
            yield return StartCoroutine(prologueController.CheckAndExecutePrologue());
            
            // 프로로그 완료 대기 (이미 완료되었을 수도 있음)
            if (!prologueCompleted)
            {
                yield return new WaitUntil(() => prologueCompleted);
            }
            
            // 콜백 해제
            prologueController.OnPrologueCompleted -= onPrologueComplete;
            
            // 프로로그 완료 후 스케줄 선택 화면으로
            _gameState.SetPhase(GamePhase.ScheduleSelection);
            _presenter?.DisplayScheduleSelection();
            
            // MonthlyScheduleView 초기화 (비활성화 상태로 시작)
            if (gamePresenter?.monthlyScheduleView != null)
            {
                gamePresenter.monthlyScheduleView.ResetSchedule();
                gamePresenter.monthlyScheduleView.gameObject.SetActive(false);
            }

            Debug.Log("게임 시작! 월간 스케줄 설정 버튼을 눌러주세요.");
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

        /// <summary>
        /// 월간 스케줄 설정 패널 토글 (UI 버튼에서 호출)
        /// MonthlyScheduleView와 ActivityPanelView를 함께 활성화/비활성화
        /// </summary>
        public void ToggleMonthlySchedule()
        {
            bool isCurrentlyActive = monthlySchedulePanel?.activeSelf ?? false;
            
            if (isCurrentlyActive)
            {
                // 패널들이 활성화되어 있으면 비활성화
                CloseMonthlySchedule();
                Debug.Log("[GameController] 월간 스케줄 설정 패널 닫힘");
            }
            else
            {
                // 패널들이 비활성화되어 있으면 활성화
                OpenMonthlySchedule();
            }
        }

        /// <summary>
        /// 월간 스케줄 설정 패널 열기 (UI 버튼에서 호출)
        /// DoTween Scale Pop 애니메이션 적용
        /// </summary>
        public void OpenMonthlySchedule()
        {
            if (monthlySchedulePanel != null)
            {
                monthlySchedulePanel.SetActive(true);
                
                // 초기 상태 설정 (작은 크기, 투명)
                RectTransform panelRect = monthlySchedulePanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = monthlySchedulePanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = monthlySchedulePanel.AddComponent<CanvasGroup>();
                
                panelRect.localScale = Vector3.one * 0.7f;
                panelCanvas.alpha = 0f;
                
                // Scale Pop 애니메이션
                panelRect.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack, 0.5f);
                panelCanvas.DOFade(1f, 0.25f)
                    .SetEase(Ease.OutQuad);
                
                Debug.Log("[GameController] 월간 스케줄 설정 패널 열림");
            }
            else
            {
                Debug.LogError("[GameController] MonthlySchedulePanel이 연결되지 않았습니다!");
            }

            if (activityPanel != null)
            {
                var activities = GetAvailableActivities();
                gamePresenter?.activityPanelView?.ShowActivities(activities, _gameState, 0);
                activityPanel.SetActive(true);
                
                // ActivityPanel도 같은 애니메이션 적용
                RectTransform activityRect = activityPanel.GetComponent<RectTransform>();
                CanvasGroup activityCanvas = activityPanel.GetComponent<CanvasGroup>();
                
                if (activityCanvas == null)
                    activityCanvas = activityPanel.AddComponent<CanvasGroup>();
                
                activityRect.localScale = Vector3.one * 0.7f;
                activityCanvas.alpha = 0f;
                
                activityRect.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack, 0.5f)
                    .SetDelay(0.05f);
                activityCanvas.DOFade(1f, 0.25f)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(0.05f);
                
                Debug.Log("[GameController] ActivityPanel 활성화");
            }
            else
            {
                Debug.LogError("[GameController] ActivityPanel이 연결되지 않았습니다!");
            }
        }

        /// <summary>
        /// 월간 스케줄 설정 패널 닫기
        /// DoTween Scale Shrink 애니메이션 적용
        /// </summary>
        public void CloseMonthlySchedule()
        {
            if (monthlySchedulePanel != null)
            {
                RectTransform panelRect = monthlySchedulePanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = monthlySchedulePanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = monthlySchedulePanel.AddComponent<CanvasGroup>();
                
                // Scale Shrink + Fade Out 애니메이션
                Sequence closeSequence = DOTween.Sequence();
                closeSequence.Append(panelRect.DOScale(Vector3.one * 0.7f, 0.2f)
                    .SetEase(Ease.InBack));
                closeSequence.Join(panelCanvas.DOFade(0f, 0.2f)
                    .SetEase(Ease.InQuad));
                closeSequence.OnComplete(() => monthlySchedulePanel.SetActive(false));
            }

            if (activityPanel != null)
            {
                RectTransform activityRect = activityPanel.GetComponent<RectTransform>();
                CanvasGroup activityCanvas = activityPanel.GetComponent<CanvasGroup>();
                
                if (activityCanvas == null)
                    activityCanvas = activityPanel.AddComponent<CanvasGroup>();
                
                // ActivityPanel도 같은 애니메이션 적용
                Sequence closeSequence = DOTween.Sequence();
                closeSequence.Append(activityRect.DOScale(Vector3.one * 0.7f, 0.2f)
                    .SetEase(Ease.InBack)
                    .SetDelay(0.02f));
                closeSequence.Join(activityCanvas.DOFade(0f, 0.2f)
                    .SetEase(Ease.InQuad)
                    .SetDelay(0.02f));
                closeSequence.OnComplete(() => activityPanel.SetActive(false));
                
                Debug.Log("[GameController] ActivityPanel 비활성화");
            }
        }

        #region 스탯 패널 제어

        /// <summary>
        /// 스탯 패널 토글 (UI 버튼에서 호출)
        /// </summary>
        public void ToggleStats()
        {
            bool isCurrentlyActive = statsPanel?.activeSelf ?? false;
            
            if (isCurrentlyActive)
            {
                CloseStats();
                Debug.Log("[GameController] 스탯 패널 닫힘");
            }
            else
            {
                OpenStats();
            }
        }

        /// <summary>
        /// 스탯 패널 열기 (DoTween Scale Pop 애니메이션)
        /// </summary>
        public void OpenStats()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
                
                // 스탯 데이터 갱신
                if (_gameState != null)
                {
                    _presenter?.DisplayStats(_gameState.Character);
                }
                
                // 초기 상태 설정 (작은 크기, 투명)
                RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = statsPanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = statsPanel.AddComponent<CanvasGroup>();
                
                panelRect.localScale = Vector3.one * 0.7f;
                panelCanvas.alpha = 0f;
                
                // Scale Pop 애니메이션
                panelRect.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack, 0.5f);
                panelCanvas.DOFade(1f, 0.25f)
                    .SetEase(Ease.OutQuad);
                
                Debug.Log("[GameController] 스탯 패널 열림");
            }
            else
            {
                Debug.LogError("[GameController] StatsPanel이 연결되지 않았습니다!");
            }
        }

        /// <summary>
        /// 스탯 패널 닫기 (DoTween Scale Shrink 애니메이션)
        /// </summary>
        public void CloseStats()
        {
            if (statsPanel != null)
            {
                RectTransform panelRect = statsPanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = statsPanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = statsPanel.AddComponent<CanvasGroup>();
                
                // Scale Shrink + Fade Out 애니메이션
                Sequence closeSequence = DOTween.Sequence();
                closeSequence.Append(panelRect.DOScale(Vector3.one * 0.7f, 0.2f)
                    .SetEase(Ease.InBack));
                closeSequence.Join(panelCanvas.DOFade(0f, 0.2f)
                    .SetEase(Ease.InQuad));
                closeSequence.OnComplete(() => statsPanel.SetActive(false));
            }
        }

        #endregion

        #region 인벤토리 패널 제어

        /// <summary>
        /// 인벤토리 패널 토글 (UI 버튼에서 호출)
        /// </summary>
        public void ToggleInventory()
        {
            bool isCurrentlyActive = inventoryPanel?.activeSelf ?? false;
            
            if (isCurrentlyActive)
            {
                CloseInventory();
                Debug.Log("[GameController] 인벤토리 패널 닫힘");
            }
            else
            {
                OpenInventory();
            }
        }

        /// <summary>
        /// 인벤토리 패널 열기 (DoTween Scale Pop 애니메이션)
        /// </summary>
        public void OpenInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
                
                // 인벤토리 데이터 갱신
                if (_gameState != null && gamePresenter?.activityPanelView != null)
                {
                    // InventoryPanelView 초기화 호출 (있는 경우)
                    var inventoryView = inventoryPanel.GetComponent<DessertKingdom.Views.InventoryPanelView>();
                    if (inventoryView != null)
                    {
                        // ItemDatabase 필요 - GameController에 필드로 추가 필요
                        // inventoryView.Initialize(_gameState.Inventory, itemDatabase, _gameState.Economy);
                    }
                }
                
                // 초기 상태 설정 (작은 크기, 투명)
                RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = inventoryPanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = inventoryPanel.AddComponent<CanvasGroup>();
                
                panelRect.localScale = Vector3.one * 0.7f;
                panelCanvas.alpha = 0f;
                
                // Scale Pop 애니메이션
                panelRect.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack, 0.5f);
                panelCanvas.DOFade(1f, 0.25f)
                    .SetEase(Ease.OutQuad);
                
                Debug.Log("[GameController] 인벤토리 패널 열림");
            }
            else
            {
                Debug.LogError("[GameController] InventoryPanel이 연결되지 않았습니다!");
            }
        }

        /// <summary>
        /// 인벤토리 패널 닫기 (DoTween Scale Shrink 애니메이션)
        /// </summary>
        public void CloseInventory()
        {
            if (inventoryPanel != null)
            {
                RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = inventoryPanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = inventoryPanel.AddComponent<CanvasGroup>();
                
                // Scale Shrink + Fade Out 애니메이션
                Sequence closeSequence = DOTween.Sequence();
                closeSequence.Append(panelRect.DOScale(Vector3.one * 0.7f, 0.2f)
                    .SetEase(Ease.InBack));
                closeSequence.Join(panelCanvas.DOFade(0f, 0.2f)
                    .SetEase(Ease.InQuad));
                closeSequence.OnComplete(() => inventoryPanel.SetActive(false));
            }
        }

        #endregion

        #region 상점 패널 제어

        /// <summary>
        /// 상점 패널 토글 (UI 버튼에서 호출)
        /// </summary>
        public void ToggleShop()
        {
            bool isCurrentlyActive = shopPanel?.activeSelf ?? false;
            
            if (isCurrentlyActive)
            {
                CloseShop();
                Debug.Log("[GameController] 상점 패널 닫힘");
            }
            else
            {
                OpenShop();
            }
        }

        /// <summary>
        /// 상점 패널 열기 (DoTween Scale Pop 애니메이션)
        /// </summary>
        public void OpenShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                
                // 상점 데이터 갱신
                if (_gameState != null)
                {
                    var shopView = shopPanel.GetComponent<DessertKingdom.Views.ShopPanelView>();
                    if (shopView != null)
                    {
                        // ItemDatabase 필요
                        // var availableItems = GetShopItems(); // 상점에 표시할 아이템 목록
                        // shopView.Initialize(availableItems, _gameState.Economy, "상점");
                    }
                }
                
                // 초기 상태 설정 (작은 크기, 투명)
                RectTransform panelRect = shopPanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = shopPanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = shopPanel.AddComponent<CanvasGroup>();
                
                panelRect.localScale = Vector3.one * 0.7f;
                panelCanvas.alpha = 0f;
                
                // Scale Pop 애니메이션
                panelRect.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack, 0.5f);
                panelCanvas.DOFade(1f, 0.25f)
                    .SetEase(Ease.OutQuad);
                
                Debug.Log("[GameController] 상점 패널 열림");
            }
            else
            {
                Debug.LogError("[GameController] ShopPanel이 연결되지 않았습니다!");
            }
        }

        /// <summary>
        /// 상점 패널 닫기 (DoTween Scale Shrink 애니메이션)
        /// </summary>
        public void CloseShop()
        {
            if (shopPanel != null)
            {
                RectTransform panelRect = shopPanel.GetComponent<RectTransform>();
                CanvasGroup panelCanvas = shopPanel.GetComponent<CanvasGroup>();
                
                if (panelCanvas == null)
                    panelCanvas = shopPanel.AddComponent<CanvasGroup>();
                
                // Scale Shrink + Fade Out 애니메이션
                Sequence closeSequence = DOTween.Sequence();
                closeSequence.Append(panelRect.DOScale(Vector3.one * 0.7f, 0.2f)
                    .SetEase(Ease.InBack));
                closeSequence.Join(panelCanvas.DOFade(0f, 0.2f)
                    .SetEase(Ease.InQuad));
                closeSequence.OnComplete(() => shopPanel.SetActive(false));
            }
        }

        #endregion

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

        // ★★★ 현재 처리 중인 이벤트 큐
        private Queue<GameEvent> _pendingEvents = new Queue<GameEvent>();
        private bool _isProcessingEvents = false;

        private void OnTurnProcessed(TurnResult result)
        {
            Debug.Log($"[GameController] OnTurnProcessed 호출됨 - IsEnding: {result.IsEnding}, 이벤트 수: {result.TriggeredEvents?.Count ?? 0}");
            
            if (result.Success)
            {
                UpdateAllUI();
                
                // 엔딩 체크 (이벤트보다 먼저)
                if (result.IsEnding)
                {
                    Debug.Log("[GameController] ★★★ 엔딩 조건 충족! 엔딩 표시");
                    ShowEnding();
                    return;
                }
                
                // ★ 이벤트를 큐에 넣고 순차적으로 처리
                if (result.TriggeredEvents != null && result.TriggeredEvents.Count > 0)
                {
                    Debug.Log($"[GameController] ★ 이벤트 {result.TriggeredEvents.Count}개 발생 (순차 처리 예정)");
                    foreach (var evt in result.TriggeredEvents)
                    {
                        _pendingEvents.Enqueue(evt);
                        Debug.Log($"  - 이벤트 대기열에 추가: {evt.Name} (Priority: {evt.Priority})");
                    }
                    
                    // 첫 번째 이벤트 처리 시작
                    ProcessNextEvent();
                }
                else
                {
                    Debug.Log("[GameController] 발생한 이벤트 없음");
                    // 이벤트 없으면 다음 턴 준비
                    PrepareNextTurn();
                }
            }
            else
            {
                Debug.Log($"[GameController] 턴 처리 실패: {result.Message}");
            }
        }
        
        /// <summary>
        /// 다음 이벤트 처리
        /// </summary>
        private void ProcessNextEvent()
        {
            if (_pendingEvents.Count == 0)
            {
                Debug.Log("[GameController] 모든 이벤트 처리 완료");
                _isProcessingEvents = false;
                // 다음 턴 준비
                PrepareNextTurn();
                return;
            }
            
            _isProcessingEvents = true;
            var evt = _pendingEvents.Dequeue();
            Debug.Log($"[GameController] 이벤트 처리 중: {evt.Name} (남은 이벤트: {_pendingEvents.Count})");
            
            // 이벤트 표시
            _presenter?.DisplayEvent(evt);
            
            // 이벤트 완료 처리 (이벤트 선택 후 ProcessNextEvent() 다시 호출 필요)
            // EventView에서 선택 완료 시 OnEventCompleted 호출하도록 연결 필요
        }
        
        /// <summary>
        /// 이벤트 선택 완료 시 호출 (EventView에서 호출 필요)
        /// </summary>
        public void OnEventCompleted()
        {
            Debug.Log("[GameController] 이벤트 선택 완료, 다음 이벤트 처리");
            ProcessNextEvent();
        }
        
        /// <summary>
        /// 다음 턴 준비
        /// </summary>
        private void PrepareNextTurn()
        {
            Debug.Log("[GameController] 다음 턴 준비 중...");
            
            // 다음 턴을 위해 스케줄 초기화
            _gameState.CurrentSchedule.Clear();
            
            // UI 업데이트
            UpdateAllUI();
            
            // 새로운 턴에서 월간 스케줄 패널 다시 열 준비
            if (gamePresenter?.monthlyScheduleView != null)
            {
                gamePresenter.monthlyScheduleView.ResetSchedule();
            }
            
            // ★★★ 모든 UI 강제 활성화
            ForceActivateAllUI();
            
            Debug.Log("[GameController] 다음 턴 준비 완료. 월간 스케줄 설정 버튼을 눌러주세요.");
        }

        private void OnActivitySelected(ActivitySelectedEvent evt)
        {
            if (evt.Activity == null) return;

            // 현재 슬롯에 활동 설정 (기존 활동 덮어쓰기 가능)
            if (evt.Slot == 1)
            {
                _gameState.CurrentSchedule.SetActivity(evt.Activity, 1);
                // MonthlyScheduleView 업데이트
                gamePresenter?.monthlyScheduleView?.SetActivity(1, evt.Activity);
            }
            else if (evt.Slot == 2)
            {
                _gameState.CurrentSchedule.SetActivity(evt.Activity, 2);
                // MonthlyScheduleView 업데이트
                gamePresenter?.monthlyScheduleView?.SetActivity(2, evt.Activity);
            }

            Debug.Log($"활동 선택됨: {evt.Activity.Name} (슬롯 {evt.Slot})");
            UpdateAllUI();
        }

        /// <summary>
        /// ActivityPanelView에서 활동 선택 시 호출 ★추가
        /// </summary>
        private void OnActivityPanelActivitySelected(Activity activity, int slot)
        {
            Debug.Log($"[GameController] ActivityPanelView에서 활동 선택: {activity?.Name} (슬롯 {slot})");
            
            if (activity == null) return;

            // 현재 슬롯에 활동 설정
            if (slot == 1)
            {
                _gameState.CurrentSchedule.SetActivity(activity, 1);
                gamePresenter?.monthlyScheduleView?.SetActivity(1, activity);
            }
            else if (slot == 2)
            {
                _gameState.CurrentSchedule.SetActivity(activity, 2);
                gamePresenter?.monthlyScheduleView?.SetActivity(2, activity);
            }

            UpdateAllUI();
            
            // ActivityPanelView 닫기 (선택사항)
            if (gamePresenter?.activityPanelView != null)
            {
                gamePresenter.activityPanelView.gameObject.SetActive(false);
                Debug.Log("[GameController] ActivityPanelView 닫힘");
            }
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

        #region 스케줄 선택 핸들러

        private void OnScheduleSlotSelected(int slot)
        {
            Debug.Log($"[GameController] 스케줄 슬롯 {slot} 선택됨");
            
            // ActivityPanelView를 해당 슬롯으로 열기
            if (gamePresenter?.activityPanelView != null)
            {
                var activities = GetAvailableActivities();
                
                // ★ GameState를 전달하여 완전한 필터링 (나이, 스탯, 자금)
                gamePresenter.activityPanelView.ShowActivities(activities, _gameState, slot);
                
                // ActivityPanelView 활성화
                gamePresenter.activityPanelView.gameObject.SetActive(true);
                
                Debug.Log($"[GameController] ActivityPanelView 활성화 완료 (Slot: {slot})");
            }
        }

        private void OnScheduleProceed()
        {
            Debug.Log("[GameController] 스케줄 진행 버튼 클릭");
            
            // 두 슬롯 모두 선택되었는지 확인
            if (_gameState.CurrentSchedule.Activity1 == null || _gameState.CurrentSchedule.Activity2 == null)
            {
                Debug.LogError("[GameController] 활동이 선택되지 않음!");
                _presenter?.ShowMessage("두 개의 활동을 모두 선택해주세요.");
                return;
            }
            
            // MonthlyScheduleView 닫기
            if (gamePresenter?.monthlyScheduleView != null)
            {
                gamePresenter.monthlyScheduleView.gameObject.SetActive(false);
                Debug.Log("[GameController] MonthlyScheduleView 닫힘");
            }
            
            // ActivityPanelView도 닫기
            if (gamePresenter?.activityPanelView != null)
            {
                gamePresenter.activityPanelView.gameObject.SetActive(false);
            }

            // ★★★ 새로운 이벤트 시스템 플로우 시작
            StartCoroutine(ExecuteTurnFlow());
        }

        /// <summary>
        /// 턴 실행 플로우 (스케줄 실행 + 이벤트 처리)
        /// </summary>
        private System.Collections.IEnumerator ExecuteTurnFlow()
        {
            var act1 = _gameState.CurrentSchedule.Activity1;
            var act2 = _gameState.CurrentSchedule.Activity2;

            // 1. 스케줄 실행 팝업 표시 (첫 번째 15일)
            if (scheduleExecutionPopup != null)
            {
                scheduleExecutionPopup.StartScheduleExecution(act1, act2);
                
                // 첫 번째 페이즈 완료 대기
                bool phase1Complete = false;
                System.Action onPhase1 = () => phase1Complete = true;
                scheduleExecutionPopup.OnPhase1Completed += onPhase1;
                
                yield return new WaitUntil(() => phase1Complete);
                scheduleExecutionPopup.OnPhase1Completed -= onPhase1;

                // 2. 첫 번째 스케줄 중 이벤트 체크 및 실행
                var phase1Events = CheckEventsForActivity(act1);
                if (phase1Events.Count > 0)
                {
                    yield return StartCoroutine(ProcessEventsSequential(phase1Events));
                }

                // 3. 두 번째 페이즈 완료 대기
                bool phase2Complete = false;
                System.Action onPhase2 = () => phase2Complete = true;
                scheduleExecutionPopup.OnPhase2Completed += onPhase2;
                
                yield return new WaitUntil(() => phase2Complete);
                scheduleExecutionPopup.OnPhase2Completed -= onPhase2;

                // 4. 두 번째 스케줄 중 이벤트 체크 및 실행
                var phase2Events = CheckEventsForActivity(act2);
                if (phase2Events.Count > 0)
                {
                    yield return StartCoroutine(ProcessEventsSequential(phase2Events));
                }

                // 5. 스케줄 실행 완료 대기
                bool executionComplete = false;
                System.Action onComplete = () => executionComplete = true;
                scheduleExecutionPopup.OnScheduleExecutionCompleted += onComplete;
                
                yield return new WaitUntil(() => executionComplete);
                scheduleExecutionPopup.OnScheduleExecutionCompleted -= onComplete;
            }

            // 6. 실제 턴 처리 (스탯 적용 등)
            var result = _turnManager.EndTurn();
            
            if (result.Success)
            {
                // 7. 랜덤 이벤트 처리
                if (result.TriggeredEvents != null && result.TriggeredEvents.Count > 0)
                {
                    yield return StartCoroutine(ProcessEventsSequential(result.TriggeredEvents));
                }

                // 8. UI 업데이트
                UpdateAllUI();

                // 9. 다음 턴 준비 (스케줄 창 열기)
                PrepareNextTurn();
            }
        }

        /// <summary>
        /// 활동 관련 이벤트 체크
        /// </summary>
        private List<GameEvent> CheckEventsForActivity(Activity activity)
        {
            // 활동 타입이나 특정 조건에 따른 이벤트 체크 로직
            // 현재는 빈 리스트 반환 (필요시 구현)
            return new List<GameEvent>();
        }

        /// <summary>
        /// 이벤트 순차 처리
        /// </summary>
        private System.Collections.IEnumerator ProcessEventsSequential(List<GameEvent> events)
        {
            foreach (var evt in events)
            {
                bool eventComplete = false;
                
                if (dialogueEventView != null)
                {
                    dialogueEventView.ShowEventDialogue(evt);
                    
                    System.Action onComplete = () => eventComplete = true;
                    dialogueEventView.OnDialogueCompleted += onComplete;
                    dialogueEventView.OnChoiceSelected += (gameEvent, choice) =>
                    {
                        _eventManager.ProcessEventChoice(gameEvent, choice, _gameState);
                        eventComplete = true;
                    };
                    
                    yield return new WaitUntil(() => eventComplete);
                    
                    dialogueEventView.OnDialogueCompleted -= onComplete;
                }
                
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        /// <summary>
        /// 모든 UI 강제 활성화 (검은 화면 문제 해결)
        /// </summary>
        private void ForceActivateAllUI()
        {
            Debug.Log("[GameController] ForceActivateAllUI() 시작");
            
            // 1. Main Camera 활성화
            if (Camera.main != null)
            {
                Camera.main.gameObject.SetActive(true);
                Debug.Log("[GameController] Main Camera 활성화");
            }
            
            // 2. 모든 Canvas 활성화 및 설정 확인
            var canvases = FindObjectsOfType<Canvas>(true); // 비활성화된 것도 포함
            foreach (var canvas in canvases)
            {
                canvas.gameObject.SetActive(true);
                
                // ★ Canvas Render Mode 강제 설정
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogWarning($"[GameController] Canvas '{canvas.name}' Render Mode를 ScreenSpaceOverlay로 변경");
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
                
                // ★ Sorting Order 확인
                //sDebug.Log($"[GameController] Canvas '{canvas.name}' 활성화 (Render Mode: {canvas.renderMode}, Sorting Order: {canvas.sortingOrder})");
            }
            
            // 3. Canvas Group 확인 (Alpha가 0이면 안 보임) - 모두 1로 강제 설정
            var canvasGroups = FindObjectsOfType<CanvasGroup>(true);
            foreach (var group in canvasGroups)
            {
                if (group.alpha != 1f || !group.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"[GameController] CanvasGroup '{group.name}' Alpha: {group.alpha} → 1.0로 강제 설정");
                    group.alpha = 1f;
                    group.gameObject.SetActive(true);
                }
                // 상호작용 가능하도록 설정
                group.interactable = true;
                group.blocksRaycasts = true;
            }
            
            // 3. 게임 오브젝트 활성화
            if (gamePresenter?.turnView != null)
                gamePresenter.turnView.gameObject.SetActive(true);
            if (gamePresenter?.statsView != null)
                gamePresenter.statsView.gameObject.SetActive(true);
            if (gamePresenter?.economyView != null)
                gamePresenter.economyView.gameObject.SetActive(true);
            
            Debug.Log("[GameController] ForceActivateAllUI() 완료");
        }

        private void OnScheduleReset()
        {
            Debug.Log("[GameController] 스케줄 초기화");
            
            // 스케줄 초기화
            _gameState.CurrentSchedule.Clear();
            
            // UI 업데이트
            if (gamePresenter?.monthlyScheduleView != null)
            {
                gamePresenter.monthlyScheduleView.ResetSchedule();
            }
            
            UpdateAllUI();
        }

        #endregion

        #region UI 업데이트

        private void UpdateAllUI()
        {
            Debug.Log("[GameController] UpdateAllUI() 시작");
            
            if (_presenter == null) 
            {
                Debug.LogError("[GameController] _presenter가 null입니다!");
                return;
            }

            // ★★★ 현재 게임 상태 로그
            Debug.Log($"[GameController] 현재 턴: {_gameState.Turn.CurrentAge}세 {_gameState.Turn.CurrentMonth}월 (턴 {_gameState.Turn.CurrentTurn})");
            Debug.Log($"[GameController] 스탯 - HP:{_gameState.Character.HP}, 매력:{_gameState.Character.Charm}, 지능:{_gameState.Character.Intelligence}, 예술:{_gameState.Character.Art}, 도덕:{_gameState.Character.Morality}, 스트레스:{_gameState.Character.Stress}");
            Debug.Log($"[GameController] 자산: {_gameState.Economy.CurrentMoney} 스위트");

            _presenter.DisplayTurn(_gameState.Turn);
            _presenter.DisplayStats(_gameState.Character);
            _presenter.DisplayEconomy(_gameState.Economy);
            
            Debug.Log("[GameController] UpdateAllUI() 완료");
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
        
        /// <summary>
        /// 플레이어 이름 가져오기
        /// </summary>
        public string GetPlayerName() => _gameState?.PlayerName ?? "이노";
        
        /// <summary>
        /// 육성대상 이름 가져오기
        /// </summary>
        public string GetTargetName() => _gameState?.TargetName ?? "루아";

        #endregion
    }
}
