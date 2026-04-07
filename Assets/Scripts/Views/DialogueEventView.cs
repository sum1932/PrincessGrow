using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;
using DessertKingdom.Controllers;
using System.Collections;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 대사 기반 이벤트 뷰
    /// 클릭으로 대사 넘기기, 스탠드 일러스트 표시, 스탯 텍스트 표시
    /// </summary>
    public class DialogueEventView : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject standIllustrationPanel;

        [Header("Character Display")]
        [SerializeField] private Image characterPortrait;
        [SerializeField] private Image standIllustration;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private GameObject standIllustrationObject;

        [Header("Dialogue Display")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject clickHintObject;
        [SerializeField] private float textTypingSpeed = 0.05f;

        [Header("Stat Text")]
        [SerializeField] private GameObject statTextPanel;
        [SerializeField] private TextMeshProUGUI statChangeText;
        [SerializeField] private float statTextDisplayDuration = 2f;

        [Header("Choice Panel")]
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private Transform choiceButtonParent;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Settings")]
        [SerializeField] private bool pauseGameWhenShown = true;
        [SerializeField] private bool autoHideStatText = true;
        [SerializeField] private KeyCode advanceKey = KeyCode.Space;
        [SerializeField] private KeyCode advanceKeyMouse = KeyCode.Mouse0;

        public event Action<GameEvent, EventChoice> OnChoiceSelected;
        public event Action OnDialogueCompleted;

        private GameEvent currentEvent;
        private EventDialogue currentDialogue;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private bool isWaitingForClick = false;
        private bool dialogueCompleted = false;
        private Coroutine typingCoroutine;
        private List<ChoiceButton> choiceButtons = new List<ChoiceButton>();
        private Dictionary<string, Sprite> characterPortraitCache = new Dictionary<string, Sprite>();
        private Dictionary<string, Sprite> standIllustrationCache = new Dictionary<string, Sprite>();

        void Awake()
        {
            InitializePanel();
        }

        void Update()
        {
            if (isWaitingForClick && !isTyping && !choicePanel.activeInHierarchy)
            {
                if (Input.GetKeyDown(advanceKey) || Input.GetKeyDown(advanceKeyMouse))
                {
                    AdvanceDialogue();
                }
            }

            if (isTyping && (Input.GetKeyDown(advanceKey) || Input.GetKeyDown(advanceKeyMouse)))
            {
                SkipTyping();
            }
        }

        private void InitializePanel()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (statTextPanel != null)
                statTextPanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (standIllustrationObject != null)
                standIllustrationObject.SetActive(false);
        }

        public void ShowEventDialogue(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                Debug.LogWarning("[DialogueEventView] GameEvent is null");
                return;
            }

            currentEvent = gameEvent;
            currentDialogue = gameEvent.Dialogue;
            currentLineIndex = 0;
            dialogueCompleted = false;

            if (currentDialogue == null || currentDialogue.Lines.Count == 0)
            {
                ShowChoices();
                return;
            }

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            if (pauseGameWhenShown)
                Time.timeScale = 0f;

            DisplayCurrentLine();
        }

        private void DisplayCurrentLine()
        {
            if (currentDialogue == null || currentLineIndex >= currentDialogue.Lines.Count)
            {
                if (currentEvent != null && currentEvent.Choices.Count > 0)
                {
                    ShowChoices();
                }
                else
                {
                    CompleteDialogue();
                }
                return;
            }

            var line = currentDialogue.Lines[currentLineIndex];
            UpdateCharacterDisplay();

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogueText(line.Text));

            if (clickHintObject != null)
                clickHintObject.SetActive(true);

            isWaitingForClick = true;
        }

        private IEnumerator TypeDialogueText(string text)
        {
            isTyping = true;
            isWaitingForClick = false;
            string processedText = ReplaceNamePlaceholders(text);

            if (dialogueText != null)
            {
                dialogueText.text = "";
                foreach (char c in processedText)
                {
                    dialogueText.text += c;
                    yield return new WaitForSecondsRealtime(textTypingSpeed);
                }
            }

            isTyping = false;
            isWaitingForClick = true;
        }

        private void UpdateCharacterDisplay()
        {
            if (currentDialogue == null) return;

            if (characterNameText != null)
            {
                string charName = string.IsNullOrEmpty(currentDialogue.CharacterName)
                    ? CharacterDefinitions.GetCharacterName(currentDialogue.CharacterId)
                    : currentDialogue.CharacterName;
                charName = ReplaceNamePlaceholders(charName);
                characterNameText.text = charName;
            }

            bool hasStand = currentDialogue.ShowStandIllustration &&
                          CharacterDefinitions.HasStandIllustration(currentDialogue.CharacterId);

            if (standIllustrationObject != null)
                standIllustrationObject.SetActive(hasStand);

            if (hasStand && standIllustration != null)
            {
                var standSprite = LoadStandIllustration(currentDialogue.CharacterId);
                if (standSprite != null)
                    standIllustration.sprite = standSprite;
            }

            if (characterPortrait != null)
            {
                var portrait = LoadPortrait(currentDialogue.CharacterId);
                if (portrait != null)
                    characterPortrait.sprite = portrait;
            }
        }

        private void SkipTyping()
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (currentDialogue != null && currentLineIndex < currentDialogue.Lines.Count)
            {
                string originalText = currentDialogue.Lines[currentLineIndex].Text;
                dialogueText.text = ReplaceNamePlaceholders(originalText);
            }

            isTyping = false;
            isWaitingForClick = true;
        }

        private void AdvanceDialogue()
        {
            if (isTyping || !isWaitingForClick) return;

            if (currentDialogue != null && currentLineIndex < currentDialogue.Lines.Count)
            {
                var line = currentDialogue.Lines[currentLineIndex];
                if (line.Effect != null)
                {
                    ApplyDialogueEffect(line.Effect);
                }
            }

            currentLineIndex++;
            DisplayCurrentLine();
        }

        private void ApplyDialogueEffect(DialogueEffect effect)
        {
            if (effect == null) return;

            bool hasChanges = false;
            var statTextBuilder = new System.Text.StringBuilder();

            foreach (var statChange in effect.StatChanges)
            {
                if (GameController.Instance?.GetGameState()?.Character != null)
                {
                    GameController.Instance.GetGameState().Character.ModifyStat(statChange.Key, statChange.Value);
                }

                if (effect.ShowStatText)
                {
                    string sign = statChange.Value > 0 ? "+" : "";
                    string statName = GetStatName(statChange.Key);
                    if (statTextBuilder.Length > 0) statTextBuilder.Append(", ");
                    statTextBuilder.Append($"{statName} {sign}{statChange.Value}");
                    hasChanges = true;
                }
            }

            foreach (var favorChange in effect.FavorChanges)
            {
                if (GameController.Instance?.GetGameState() != null)
                {
                    var npc = GameController.Instance.GetGameState().GetNPC(favorChange.Key);
                    if (npc != null)
                    {
                        npc.ChangeFavorability(favorChange.Value);
                    }
                }

                if (effect.ShowStatText)
                {
                    string sign = favorChange.Value > 0 ? "+" : "";
                    string npcName = CharacterDefinitions.GetCharacterName(favorChange.Key);
                    if (statTextBuilder.Length > 0) statTextBuilder.Append(", ");
                    statTextBuilder.Append($"{npcName} 호감도 {sign}{favorChange.Value}");
                    hasChanges = true;
                }
            }

            if (hasChanges && effect.ShowStatText)
            {
                ShowStatText(statTextBuilder.ToString());
            }
        }

        private void ShowStatText(string text)
        {
            if (statChangeText != null)
                statChangeText.text = text;

            if (statTextPanel != null)
            {
                statTextPanel.SetActive(true);
                if (autoHideStatText)
                {
                    StartCoroutine(HideStatTextAfterDelay());
                }
            }
        }

        private IEnumerator HideStatTextAfterDelay()
        {
            yield return new WaitForSecondsRealtime(statTextDisplayDuration);
            if (statTextPanel != null)
                statTextPanel.SetActive(false);
        }

        private void ShowChoices()
        {
            if (currentEvent == null || currentEvent.Choices.Count == 0)
            {
                CompleteDialogue();
                return;
            }

            if (choicePanel != null)
                choicePanel.SetActive(true);

            if (clickHintObject != null)
                clickHintObject.SetActive(false);

            ClearChoiceButtons();

            foreach (var choice in currentEvent.Choices)
            {
                if (choiceButtonPrefab == null || choiceButtonParent == null) continue;

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
            OnChoiceSelected?.Invoke(currentEvent, choice);
            CloseDialoguePanel();
        }

        private void CompleteDialogue()
        {
            if (dialogueCompleted) return;
            dialogueCompleted = true;
            OnDialogueCompleted?.Invoke();
            CloseDialoguePanel();
        }

        private void CloseDialoguePanel()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (statTextPanel != null)
                statTextPanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (standIllustrationObject != null)
                standIllustrationObject.SetActive(false);

            ClearChoiceButtons();
            currentEvent = null;
            currentDialogue = null;

            if (pauseGameWhenShown)
                Time.timeScale = 1f;
        }

        private string GetStatName(StatType stat)
        {
            return stat switch
            {
                StatType.HP => "체력",
                StatType.Charm => "매력",
                StatType.Intelligence => "지능",
                StatType.Art => "예술",
                StatType.Morality => "도덕",
                StatType.Stress => "스트레스",
                _ => stat.ToString()
            };
        }

        private Sprite LoadPortrait(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;
            if (characterPortraitCache.TryGetValue(characterId, out var cached))
                return cached;

            string path = CharacterDefinitions.GetPortraitPath(characterId);
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
                characterPortraitCache[characterId] = sprite;

            return sprite;
        }

        private Sprite LoadStandIllustration(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;
            if (standIllustrationCache.TryGetValue(characterId, out var cached))
                return cached;

            string path = CharacterDefinitions.GetStandIllustrationPath(characterId);
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
                standIllustrationCache[characterId] = sprite;

            return sprite;
        }

        public static string ReplaceNamePlaceholdersStatic(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string playerName = "이노";
            string targetName = "루아";

            // GameController를 찾아서 이름 가져오기
            var gameController = GameController.Instance;
            if (gameController == null)
            {
                // Instance가 null이면 씬에서 찾기
                gameController = UnityEngine.Object.FindObjectOfType<GameController>();
            }
            
            if (gameController != null)
            {
                playerName = gameController.GetPlayerName();
                targetName = gameController.GetTargetName();
            }

            text = text.Replace("{PLAYER_NAME}", playerName);
            text = text.Replace("{TARGET_NAME}", targetName);

            return text;
        }

        private string ReplaceNamePlaceholders(string text)
        {
            return ReplaceNamePlaceholdersStatic(text);
        }
    }
}
