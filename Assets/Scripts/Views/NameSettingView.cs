using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 게임 시작 시 플레이어와 육성대상 이름 설정 UI
    /// </summary>
    public class NameSettingView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_InputField targetNameInput;
        [SerializeField] private TextMeshProUGUI playerNamePlaceholder;
        [SerializeField] private TextMeshProUGUI targetNamePlaceholder;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Settings")]
        [SerializeField] private string defaultPlayerName = "이노";
        [SerializeField] private string defaultTargetName = "루아";
        [SerializeField] private int minNameLength = 1;
        [SerializeField] private int maxNameLength = 8;

        // 이벤트
        public event Action<string, string> OnNamesConfirmed;
        public event Action OnNamesSkipped;

        private void Awake()
        {
            InitializePanel();
            SetupButtons();
        }

        private void InitializePanel()
        {
            if (panel != null)
                panel.SetActive(false);

            // Placeholder 텍스트 설정
            if (playerNamePlaceholder != null)
                playerNamePlaceholder.text = $"기본: {defaultPlayerName}";
            
            if (targetNamePlaceholder != null)
                targetNamePlaceholder.text = $"기본: {defaultTargetName}";

            // 입력 제한 설정
            if (playerNameInput != null)
            {
                playerNameInput.characterLimit = maxNameLength;
                playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
            }

            if (targetNameInput != null)
            {
                targetNameInput.characterLimit = maxNameLength;
                targetNameInput.onValueChanged.AddListener(OnTargetNameChanged);
            }
        }

        private void SetupButtons()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
        }

        private void OnDestroy()
        {
            if (playerNameInput != null)
                playerNameInput.onValueChanged.RemoveListener(OnPlayerNameChanged);
            
            if (targetNameInput != null)
                targetNameInput.onValueChanged.RemoveListener(OnTargetNameChanged);

            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (skipButton != null)
                skipButton.onClick.RemoveListener(OnSkipClicked);
        }

        /// <summary>
        /// 이름 설정 패널 표시
        /// </summary>
        public void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
                
                // 입력 필드 초기화
                if (playerNameInput != null)
                {
                    playerNameInput.text = "";
                    playerNameInput.ActivateInputField();
                }
                
                if (targetNameInput != null)
                    targetNameInput.text = "";
            }

            UpdateConfirmButtonState();
        }

        /// <summary>
        /// 이름 설정 패널 숨김
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        /// <summary>
        /// 플레이어 이름 입력값 변경 시
        /// </summary>
        private void OnPlayerNameChanged(string value)
        {
            UpdateConfirmButtonState();
        }

        /// <summary>
        /// 육성대상 이름 입력값 변경 시
        /// </summary>
        private void OnTargetNameChanged(string value)
        {
            UpdateConfirmButtonState();
        }

        /// <summary>
        /// 확인 버튼 상태 업데이트
        /// </summary>
        private void UpdateConfirmButtonState()
        {
            if (confirmButton == null) return;

            string playerName = GetPlayerName();
            string targetName = GetTargetName();

            // 둘 다 비어있거나, 둘 다 입력되었을 때만 활성화
            bool canConfirm = (string.IsNullOrWhiteSpace(playerName) && string.IsNullOrWhiteSpace(targetName)) ||
                              (!string.IsNullOrWhiteSpace(playerName) && !string.IsNullOrWhiteSpace(targetName));

            confirmButton.interactable = canConfirm;
        }

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        private void OnConfirmClicked()
        {
            string playerName = GetPlayerName();
            string targetName = GetTargetName();

            // 빈 값이면 기본값 사용
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = defaultPlayerName;
            
            if (string.IsNullOrWhiteSpace(targetName))
                targetName = defaultTargetName;

            // 공백 제거
            playerName = playerName.Trim();
            targetName = targetName.Trim();

            Debug.Log($"[NameSettingView] 이름 설정 완료 - 플레이어: {playerName}, 육성대상: {targetName}");
            
            Hide();
            OnNamesConfirmed?.Invoke(playerName, targetName);
        }

        /// <summary>
        /// 건너뛰기 버튼 클릭
        /// </summary>
        private void OnSkipClicked()
        {
            Debug.Log($"[NameSettingView] 이름 설정 건너뛰기 - 기본값 사용");
            
            Hide();
            OnNamesSkipped?.Invoke();
        }

        /// <summary>
        /// 플레이어 이름 가져오기
        /// </summary>
        private string GetPlayerName()
        {
            return playerNameInput != null ? playerNameInput.text : "";
        }

        /// <summary>
        /// 육성대상 이름 가져오기
        /// </summary>
        private string GetTargetName()
        {
            return targetNameInput != null ? targetNameInput.text : "";
        }

        /// <summary>
        /// 패널이 활성화되어 있는지 확인
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;
    }
}
