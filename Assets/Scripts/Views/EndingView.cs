using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 엔딩 표시 View
    /// 엔딩 정보와 CG 이미지를 표시
    /// </summary>
    public class EndingView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TextMeshProUGUI endingTitleText;
        [SerializeField] private TextMeshProUGUI endingDescriptionText;
        [SerializeField] private Image endingImage;
        [SerializeField] private TextMeshProUGUI conditionText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button titleButton;

        [Header("Animation")]
        [SerializeField] private Animator endingAnimator;
        [SerializeField] private string showAnimationTrigger = "Show";
        [SerializeField] private float autoContinueDelay = 5f;

        [Header("Hidden Ending Settings")]
        [SerializeField] private string hiddenEndingTitle = "???";
        [SerializeField] private string hiddenEndingDescription = "숨겨진 엔딩을 발견했습니다!";
        [SerializeField] private Sprite hiddenEndingSprite;

        public event System.Action OnContinueClicked;
        public event System.Action OnTitleClicked;

        private EndingCondition currentEnding;

        private void Awake()
        {
            InitializePanel();
            SetupButtons();
        }

        private void InitializePanel()
        {
            if (endingPanel != null)
                endingPanel.SetActive(false);
        }

        private void SetupButtons()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueButtonClicked);

            if (titleButton != null)
                titleButton.onClick.AddListener(OnTitleButtonClicked);
        }

        /// <summary>
        /// 엔딩을 표시
        /// </summary>
        public void Show(EndingCondition ending)
        {
            if (ending == null)
            {
                Debug.LogWarning("[EndingView] EndingCondition is null");
                return;
            }

            currentEnding = ending;

            UpdateEndingDisplay(ending);

            if (endingPanel != null)
                endingPanel.SetActive(true);

            // 애니메이션 실행
            if (endingAnimator != null)
                endingAnimator.SetTrigger(showAnimationTrigger);

            // 자동 진행 (선택적)
            if (autoContinueDelay > 0)
                Invoke(nameof(TriggerAutoContinue), autoContinueDelay);
        }

        private void UpdateEndingDisplay(EndingCondition ending)
        {
            bool isHidden = ending.IsHidden;

            // 제목
            if (endingTitleText != null)
                endingTitleText.text = isHidden ? hiddenEndingTitle : ending.Name;

            // 설명
            if (endingDescriptionText != null)
                endingDescriptionText.text = isHidden ? hiddenEndingDescription : ending.Description;

            // 이미지
            if (endingImage != null)
            {
                if (isHidden && hiddenEndingSprite != null)
                    endingImage.sprite = hiddenEndingSprite;
                // TODO: 일반 엔딩 이미지 설정
            }

            // 조건 텍스트
            if (conditionText != null)
            {
                conditionText.text = GetConditionsString(ending);
            }
        }

        private string GetConditionsString(EndingCondition ending)
        {
            if (ending == null) return "";

            var conditions = new System.Collections.Generic.List<string>();

            // 스탯 조건
            foreach (var statReq in ending.RequiredStats)
            {
                conditions.Add($"{statReq.Key}: {statReq.Value}+");
            }

            // 호감도 조건
            foreach (var favorReq in ending.RequiredFavor)
            {
                conditions.Add($"{favorReq.Key} 호감도: {favorReq.Value}+");
            }

            // 이벤트 조건
            foreach (var eventId in ending.RequiredEvents)
            {
                conditions.Add($"이벤트 완료: {eventId}");
            }

            return conditions.Count > 0
                ? "달성 조건:\n" + string.Join("\n", conditions)
                : "특별한 조건 없이 달성된 엔딩";
        }

        private void TriggerAutoContinue()
        {
            if (gameObject.activeInHierarchy)
                OnContinueButtonClicked();
        }

        private void OnContinueButtonClicked()
        {
            CancelInvoke(nameof(TriggerAutoContinue));
            Hide();
            OnContinueClicked?.Invoke();
        }

        private void OnTitleButtonClicked()
        {
            CancelInvoke(nameof(TriggerAutoContinue));
            Hide();
            OnTitleClicked?.Invoke();
        }

        private void Hide()
        {
            if (endingPanel != null)
                endingPanel.SetActive(false);

            currentEnding = null;
        }

        /// <summary>
        /// UnityGamePresenter에서 호출되는 이벤트 핸들러
        /// </summary>
        public void OnEndingDisplayed(EndingCondition ending)
        {
            Show(ending);
        }
    }
}
