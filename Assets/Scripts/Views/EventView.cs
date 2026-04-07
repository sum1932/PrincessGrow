using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;
using DessertKingdom.Controllers;  // ★ GameController 참조

namespace DessertKingdom.Views
{
    /// <summary>
    /// 이벤트 표시 View
    /// 이벤트 팝업과 선택지 버튼을 표시
    /// </summary>
    public class EventView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private Image eventBackgroundImage;
        [SerializeField] private Transform choiceButtonParent;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Event Type Colors")]
        [SerializeField] private Color fixedEventColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color randomEventColor = new Color(0.3f, 1f, 0.4f);
        [SerializeField] private Color npcEventColor = new Color(1f, 0.4f, 0.6f);
        [SerializeField] private Color specialEventColor = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private Color hiddenEventColor = new Color(0.6f, 0.3f, 1f);

        [Header("Settings")]
        [SerializeField] private bool pauseGameWhenShown = true;
        [SerializeField] private bool showUnavailableChoices = true;
        [SerializeField] private Color disabledChoiceColor = Color.gray;

        public event Action<GameEvent, EventChoice> OnChoiceSelected;

        private GameEvent currentEvent;
        private List<ChoiceButton> choiceButtons = new List<ChoiceButton>();

        private void Awake()
        {
            InitializePanel();
        }

        private void InitializePanel()
        {
            if (eventPanel != null)
                eventPanel.SetActive(false);
        }

        /// <summary>
        /// 이벤트를 표시
        /// </summary>
        public void Show(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                Debug.LogWarning("[EventView] GameEvent is null");
                return;
            }

            currentEvent = gameEvent;

            UpdateEventDisplay(gameEvent);
            CreateChoiceButtons(gameEvent);

            if (eventPanel != null)
                eventPanel.SetActive(true);

            if (pauseGameWhenShown)
                Time.timeScale = 0f;
        }

        private void UpdateEventDisplay(GameEvent gameEvent)
        {
            if (eventTitleText != null)
                eventTitleText.text = gameEvent.Name;

            if (eventDescriptionText != null)
                eventDescriptionText.text = gameEvent.Description;

            if (eventBackgroundImage != null)
                eventBackgroundImage.color = GetEventColor(gameEvent.Type);
        }

        private void CreateChoiceButtons(GameEvent gameEvent)
        {
            // 기존 버튼 정리
            ClearChoiceButtons();

            if (choiceButtonPrefab == null || choiceButtonParent == null)
            {
                Debug.LogError("[EventView] ChoiceButtonPrefab or ChoiceButtonParent is not assigned");
                return;
            }

            // 선택지 버튼 생성
            foreach (var choice in gameEvent.Choices)
            {
                var buttonObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
                var choiceButton = buttonObj.GetComponent<ChoiceButton>() ?? buttonObj.AddComponent<ChoiceButton>();

                choiceButton.Initialize(choice, OnChoiceButtonClicked);
                choiceButtons.Add(choiceButton);
            }
        }

        private void ClearChoiceButtons()
        {
            foreach (var button in choiceButtons)
            {
                if (button?.gameObject != null)
                    Destroy(button.gameObject);
            }
            choiceButtons.Clear();
        }

        private void OnChoiceButtonClicked(EventChoice choice)
        {
            if (choice == null || currentEvent == null) return;

            Debug.Log($"[EventView] Choice selected: {choice.Text} for event: {currentEvent.Name}");
            OnChoiceSelected?.Invoke(currentEvent, choice);

            CloseEventPanel();
            
            // ★★★ 다음 이벤트 처리 요청 (GameController에 연결)
            GameController.Instance?.OnEventCompleted();
        }

        private void CloseEventPanel()
        {
            if (eventPanel != null)
                eventPanel.SetActive(false);

            ClearChoiceButtons();
            currentEvent = null;

            if (pauseGameWhenShown)
                Time.timeScale = 1f;
        }

        private Color GetEventColor(Core.Domain.EventType type)
        {
            return type switch
            {
                Core.Domain.EventType.Fixed => fixedEventColor,
                Core.Domain.EventType.Random => randomEventColor,
                Core.Domain.EventType.NPC => npcEventColor,
                Core.Domain.EventType.Special => specialEventColor,
                Core.Domain.EventType.Hidden => hiddenEventColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// UnityGamePresenter에서 호출되는 이벤트 핸들러
        /// </summary>
        public void OnEventDisplayed(GameEvent gameEvent)
        {
            Show(gameEvent);
        }
    }

    /// <summary>
    /// 개별 선택지 버튼 컴포넌트
    /// </summary>
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI choiceText;
        [SerializeField] private TextMeshProUGUI requirementsText;
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;

        private EventChoice choice;
        private Action<EventChoice> onClickCallback;

        public void Initialize(EventChoice choice, Action<EventChoice> callback)
        {
            this.choice = choice;
            this.onClickCallback = callback;

            FindComponents();
            UpdateVisuals();

            if (button != null)
                button.onClick.AddListener(() => onClickCallback?.Invoke(choice));
        }

        private void FindComponents()
        {
            if (choiceText == null)
                choiceText = transform.Find("ChoiceText")?.GetComponent<TextMeshProUGUI>();
            if (requirementsText == null)
                requirementsText = transform.Find("RequirementsText")?.GetComponent<TextMeshProUGUI>();
            if (button == null)
                button = GetComponent<Button>();
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
        }

        private void UpdateVisuals()
        {
            if (choice == null) return;

            if (choiceText != null)
            {
                // 이름 플레이스홀더 치환
                string processedText = DialogueEventView.ReplaceNamePlaceholdersStatic(choice.Text);
                choiceText.text = processedText;
            }

            // 요구사항 표시 (선택적)
            if (requirementsText != null)
            {
                if (choice.Requirements.Count > 0)
                {
                    requirementsText.gameObject.SetActive(true);
                    requirementsText.text = GetRequirementsString(choice.Requirements);
                }
                else
                {
                    requirementsText.gameObject.SetActive(false);
                }
            }
        }

        private string GetRequirementsString(Dictionary<string, object> requirements)
        {
            var reqStrings = new List<string>();
            foreach (var req in requirements)
            {
                if (req.Key.StartsWith("stat_"))
                {
                    string statName = req.Key.Substring(5);
                    reqStrings.Add($"{statName} {req.Value} 필요");
                }
            }
            return string.Join(", ", reqStrings);
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
                button.interactable = interactable;

            if (backgroundImage != null)
                backgroundImage.color = interactable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
