using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 활동 선택 패널 View
    /// 활동 버튼 목록을 스크롤 뷰로 표시
    /// </summary>
    public class ActivityPanelView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject activityButtonPrefab;

        [Header("Filter Settings")]
        [SerializeField] private ToggleGroup activityTypeToggleGroup;
        [SerializeField] private Toggle allToggle;
        [SerializeField] private Toggle lessonToggle;
        [SerializeField] private Toggle partTimeToggle;
        [SerializeField] private Toggle restToggle;
        [SerializeField] private Toggle outingToggle;
        [SerializeField] private Toggle specialToggle;

        [Header("Settings")]
        [SerializeField] private int maxActivitiesPerRow = 3;
        [SerializeField] private float buttonSpacing = 10f;
        [SerializeField] private bool showCost = true;
        [SerializeField] private bool filterByAge = true;
        [SerializeField] private bool hideUnavailableActivities = true; // ★ 조건 불만족 활동 숨김 여부

        [Header("Colors")]
        [SerializeField] private Color lessonColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color partTimeColor = new Color(0.3f, 1f, 0.4f);
        [SerializeField] private Color restColor = new Color(1f, 0.8f, 0.3f);
        [SerializeField] private Color outingColor = new Color(1f, 0.4f, 0.6f);
        [SerializeField] private Color specialColor = new Color(0.8f, 0.4f, 1f);
        [SerializeField] private Color disabledColor = Color.gray;

        public event Action<Activity, int> OnActivitySelected;

        private List<ActivityButton> activityButtons = new List<ActivityButton>();
        private List<Activity> currentActivities = new List<Activity>();
        private GameState currentGameState; // ★ GameState 저장
        private int currentSlot = 0;
        private ActivityType currentFilter = ActivityType.Lesson;

        private void Awake()
        {
            SetupToggles();
        }

        private void SetupToggles()
        {
            if (allToggle != null)
                allToggle.onValueChanged.AddListener(isOn => { if (isOn) FilterActivities(ActivityType.Lesson, true); });
            if (lessonToggle != null)
                lessonToggle.onValueChanged.AddListener(isOn => { if (isOn) FilterActivities(ActivityType.Lesson); });
            if (partTimeToggle != null)
                partTimeToggle.onValueChanged.AddListener(isOn => { if (isOn) FilterActivities(ActivityType.PartTime); });
            if (restToggle != null)
                restToggle.onValueChanged.AddListener(isOn => { if (isOn) FilterActivities(ActivityType.Rest); });
            if (outingToggle != null)
                outingToggle.onValueChanged.AddListener(isOn => { if (isOn) FilterActivities(ActivityType.Outing); });
            if (specialToggle != null)
                specialToggle.onValueChanged.AddListener(isOn => { if (isOn) FilterActivities(ActivityType.Special); });
        }

        /// <summary>
        /// 활동 목록을 표시 (GameState 버전) ★수정
        /// </summary>
        public void ShowActivities(List<Activity> activities, GameState gameState, int slot = 0)
        {
            this.currentActivities = activities ?? new List<Activity>();
            this.currentGameState = gameState; // ★ GameState 저장
            this.currentSlot = slot;

            ClearButtons();
            CreateActivityButtons();
            FilterActivities(currentFilter, allToggle?.isOn ?? true);
        }

        /// <summary>
        /// 이전 버전 호환용 (나이만 받는 경우) ★유지
        /// </summary>
        public void ShowActivities(List<Activity> activities, int currentAge, int slot = 0)
        {
            // GameController에서 GameState 가져오기 시도
            if (DessertKingdom.Controllers.GameController.Instance != null)
            {
                currentGameState = DessertKingdom.Controllers.GameController.Instance.GetGameState();
            }
            
            ShowActivities(activities, currentGameState, slot);
        }

        private void ClearButtons()
        {
            foreach (var button in activityButtons)
            {
                if (button?.gameObject != null)
                    Destroy(button.gameObject);
            }
            activityButtons.Clear();
        }

        private void CreateActivityButtons()
        {
            if (activityButtonPrefab == null || contentParent == null)
            {
                Debug.LogError("[ActivityPanelView] Prefab or ContentParent is not assigned");
                return;
            }

            foreach (var activity in currentActivities)
            {
                if (activity == null) continue;

                var buttonObj = Instantiate(activityButtonPrefab, contentParent);
                var activityButton = buttonObj.GetComponent<ActivityButton>() ?? buttonObj.AddComponent<ActivityButton>();

                activityButton.Initialize(activity, currentGameState?.Turn?.CurrentAge ?? 6, OnActivityButtonClicked);
                activityButtons.Add(activityButton);
            }
        }

        private void FilterActivities(ActivityType type, bool showAll = false)
        {
            currentFilter = type;

            // 디버깅: GameState 확인
            if (currentGameState == null)
            {
                Debug.LogError("[ActivityPanelView] GameState is NULL! 필터링 불가");
                return;
            }

            int currentAge = currentGameState.Turn.CurrentAge;
            int currentMonth = currentGameState.Turn.CurrentMonth;
            var stats = currentGameState.Character;
            int currentMoney = currentGameState.Economy.CurrentMoney;

            Debug.Log($"[ActivityPanelView] 필터링 시작 - 나이: {currentAge}, 월: {currentMonth}, 자금: {currentMoney}, 활동 수: {activityButtons.Count}");
            Debug.Log($"[ActivityPanelView] 스탯 - HP:{stats.HP}, 매력:{stats.Charm}, 지능:{stats.Intelligence}");

            foreach (var button in activityButtons)
            {
                if (button == null || button.gameObject == null) continue;

                // Activity가 null이면 숨김
                if (button.Activity == null)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                // 조건 체크
                bool isAgeAvailable = button.Activity.MinAge <= currentAge;
                bool isStatAvailable = button.Activity.IsAvailable(currentAge, stats, currentMonth);
                bool isAffordable = button.Activity.CanAfford(currentMoney);
                bool isAvailable = isAgeAvailable && isStatAvailable && isAffordable;

                // 디버깅: Special이나 Outing 타입은 모두 로그
                if (button.Activity.Type == ActivityType.Special || button.Activity.Type == ActivityType.Outing)
                {
                    Debug.Log($"[ActivityPanelView] {button.Activity.Name} (타입:{button.Activity.Type}): " +
                             $"MinAge={button.Activity.MinAge}, 현재나이={currentAge}, " +
                             $"나이조건({isAgeAvailable}), 스탯조건({isStatAvailable}), 자금조건({isAffordable}) = {isAvailable}");
                }
                // 디버깅: 첫 5개만 로그
                else if (activityButtons.IndexOf(button) < 5)
                {
                    Debug.Log($"[ActivityPanelView] {button.Activity.Name}: 나이({isAgeAvailable}), 스탯({isStatAvailable}), 자금({isAffordable}) = {isAvailable}");
                }

                // 표시 여부 결정
                bool shouldShow = showAll || button.Activity.Type == type;
                
                // 숨김 모드일 때 조건 불만족 활동 숨김
                if (hideUnavailableActivities && !isAvailable)
                {
                    shouldShow = false;
                }

                button.gameObject.SetActive(shouldShow);
                
                // 표시는 되지만 비활성화된 상태인 경우
                if (shouldShow && !isAvailable)
                {
                    button.SetInteractable(false);
                    button.ShowUnavailableReason(!isAgeAvailable, !isStatAvailable, !isAffordable);
                }
                else if (shouldShow && isAvailable)
                {
                    button.SetInteractable(true);
                }
            }

            // 스크롤 위치 리셋
            if (scrollRect != null)
                scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        private void OnActivityButtonClicked(Activity activity)
        {
            if (activity == null) return;

            Debug.Log($"[ActivityPanelView] Activity selected: {activity.Name} (Slot: {currentSlot})");
            OnActivitySelected?.Invoke(activity, currentSlot);
        }

        /// <summary>
        /// UnityGamePresenter에서 호출되는 이벤트 핸들러
        /// </summary>
        public void OnScheduleSelectionRequested()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 특정 슬롯의 활동 선택 UI를 표시
        /// </summary>
        public void ShowForSlot(int slot)
        {
            currentSlot = slot;
            gameObject.SetActive(true);
            
            // 해당 슬롯이 이미 선택된 활동이 있다면 필터링 유지
            FilterActivities(currentFilter, allToggle?.isOn ?? true);
        }

        /// <summary>
        /// 현재 선택된 슬롯 번호 반환
        /// </summary>
        public int GetCurrentSlot()
        {
            return currentSlot;
        }

        /// <summary>
        /// 패널 전체 초기화
        /// </summary>
        public void ResetPanel()
        {
            currentSlot = 0;
            currentGameState = null;
            ClearButtons();
            if (allToggle != null)
                allToggle.isOn = true;
        }
    }

    /// <summary>
    /// 개별 활동 버튼 컴포넌트
    /// </summary>
    public class ActivityButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI requirementText; // ★ 요구사항 텍스트 추가
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button button;
        [SerializeField] private GameObject unavailableOverlay; // ★ 비활성화 오버레이

        public Activity Activity { get; private set; }

        private Action<Activity> onClickCallback;

        public void Initialize(Activity activity, int currentAge, Action<Activity> callback)
        {
            Activity = activity;
            onClickCallback = callback;

            FindComponents();
            UpdateVisuals();

            if (button != null)
                button.onClick.AddListener(() => onClickCallback?.Invoke(Activity));
        }

        private void FindComponents()
        {
            if (nameText == null)
                nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (costText == null)
                costText = transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            if (descriptionText == null)
                descriptionText = transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            if (requirementText == null)
                requirementText = transform.Find("RequirementText")?.GetComponent<TextMeshProUGUI>();
            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            if (button == null)
                button = GetComponent<Button>();
            if (unavailableOverlay == null)
                unavailableOverlay = transform.Find("UnavailableOverlay")?.gameObject;
        }

        private void UpdateVisuals()
        {
            if (Activity == null) return;

            if (nameText != null)
                nameText.text = Activity.Name;

            if (costText != null)
            {
                if (Activity.Cost > 0)
                    costText.text = $"-{Activity.Cost}";
                else if (Activity.Income > 0)
                    costText.text = $"+{Activity.Income}";
                else
                    costText.text = "무료";
            }

            if (descriptionText != null)
                descriptionText.text = Activity.Description;

            if (iconImage != null)
                iconImage.color = GetActivityColor(Activity.Type);

            // 요구사항 표시 ★추가
            if (requirementText != null)
            {
                string reqText = Activity.GetRequirementText();
                if (!string.IsNullOrEmpty(reqText))
                {
                    requirementText.text = $"[{reqText}]";
                    requirementText.gameObject.SetActive(true);
                }
                else
                {
                    requirementText.gameObject.SetActive(false);
                }
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
                button.interactable = interactable;

            if (backgroundImage != null)
                backgroundImage.color = interactable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

            if (unavailableOverlay != null)
                unavailableOverlay.SetActive(!interactable);
        }

        /// <summary>
        /// 불가능한 이유 표시 ★추가
        /// </summary>
        public void ShowUnavailableReason(bool ageIssue, bool statIssue, bool moneyIssue)
        {
            if (requirementText != null)
            {
                string reason = "";
                if (moneyIssue)
                    reason = "자금 부족";
                else if (ageIssue)
                    reason = $"{Activity.MinAge}세 필요";
                else if (statIssue)
                    reason = "스탯 부족";
                
                requirementText.text = $"[{reason}]";
                requirementText.color = Color.red;
                requirementText.gameObject.SetActive(true);
            }
        }

        private Color GetActivityColor(ActivityType type)
        {
            return type switch
            {
                ActivityType.Lesson => new Color(0.3f, 0.6f, 1f),
                ActivityType.PartTime => new Color(0.3f, 1f, 0.4f),
                ActivityType.Rest => new Color(1f, 0.8f, 0.3f),
                ActivityType.Outing => new Color(1f, 0.4f, 0.6f),
                ActivityType.Special => new Color(0.8f, 0.4f, 1f),
                _ => Color.white
            };
        }
    }
}
