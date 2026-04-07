using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 메시지/알림 패널 View
    /// 팝업 메시지와 토스트 알림을 표시
    /// </summary>
    public class MessagePanelView : MonoBehaviour
    {
        [Header("Popup UI")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI popupTitleText;
        [SerializeField] private TextMeshProUGUI popupMessageText;
        [SerializeField] private Button popupConfirmButton;
        [SerializeField] private Button popupCancelButton;

        [Header("Toast UI")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private TextMeshProUGUI toastMessageText;
        [SerializeField] private CanvasGroup toastCanvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float toastShowDuration = 0.3f;
        [SerializeField] private float toastDisplayDuration = 2f;
        [SerializeField] private float toastHideDuration = 0.3f;
        [SerializeField] private AnimationCurve toastAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Popup Settings")]
        [SerializeField] private bool pauseTimeWhenPopupOpen = true;
        [SerializeField] private bool allowMultiplePopups = false;

        private Coroutine currentToastCoroutine;
        private bool isPopupOpen = false;

        private void Awake()
        {
            InitializePanels();
        }

        private void InitializePanels()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            if (toastPanel != null)
            {
                toastPanel.SetActive(false);
                if (toastCanvasGroup == null)
                    toastCanvasGroup = toastPanel.GetComponent<CanvasGroup>();
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (popupConfirmButton != null)
                popupConfirmButton.onClick.AddListener(OnPopupConfirm);

            if (popupCancelButton != null)
                popupCancelButton.onClick.AddListener(OnPopupCancel);
        }

        #region Popup Methods

        /// <summary>
        /// 확인/취소 버튼이 있는 팝업 메시지 표시
        /// </summary>
        public void ShowPopup(string title, string message, System.Action onConfirm = null, System.Action onCancel = null)
        {
            if (isPopupOpen && !allowMultiplePopups)
            {
                Debug.LogWarning("[MessagePanelView] Popup is already open");
                return;
            }

            if (popupPanel == null)
            {
                Debug.LogError("[MessagePanelView] PopupPanel is not assigned");
                return;
            }

            onPopupConfirmCallback = onConfirm;
            onPopupCancelCallback = onCancel;

            if (popupTitleText != null)
                popupTitleText.text = title ?? "알림";

            if (popupMessageText != null)
                popupMessageText.text = message;

            popupPanel.SetActive(true);
            isPopupOpen = true;

            if (pauseTimeWhenPopupOpen)
                Time.timeScale = 0f;
        }

        /// <summary>
        /// 간단한 확인 팝업 표시
        /// </summary>
        public void ShowConfirmPopup(string message, System.Action onConfirm = null)
        {
            ShowPopup("확인", message, onConfirm, null);

            // 취소 버튼 숨김
            if (popupCancelButton != null)
                popupCancelButton.gameObject.SetActive(false);
        }

        private System.Action onPopupConfirmCallback;
        private System.Action onPopupCancelCallback;

        private void OnPopupConfirm()
        {
            ClosePopup();
            onPopupConfirmCallback?.Invoke();
        }

        private void OnPopupCancel()
        {
            ClosePopup();
            onPopupCancelCallback?.Invoke();
        }

        private void ClosePopup()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            isPopupOpen = false;

            if (pauseTimeWhenPopupOpen)
                Time.timeScale = 1f;

            // 취소 버튼 다시 활성화
            if (popupCancelButton != null)
                popupCancelButton.gameObject.SetActive(true);
        }

        #endregion

        #region Toast Methods

        /// <summary>
        /// 토스트 메시지 표시 (자동 사라짐)
        /// </summary>
        public void ShowToast(string message, float? customDuration = null)
        {
            if (toastPanel == null)
            {
                Debug.LogError("[MessagePanelView] ToastPanel is not assigned");
                return;
            }

            if (currentToastCoroutine != null)
                StopCoroutine(currentToastCoroutine);

            currentToastCoroutine = StartCoroutine(ToastCoroutine(message, customDuration ?? toastDisplayDuration));
        }

        /// <summary>
        /// 간단한 메시지 표시 (UnityGamePresenter용)
        /// </summary>
        public void Show(string message)
        {
            ShowToast(message);
        }

        private IEnumerator ToastCoroutine(string message, float duration)
        {
            // 메시지 설정
            if (toastMessageText != null)
                toastMessageText.text = message;

            // 패널 활성화
            toastPanel.SetActive(true);

            // 페이드 인
            if (toastCanvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < toastShowDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / toastShowDuration;
                    toastCanvasGroup.alpha = toastAnimationCurve.Evaluate(t);
                    yield return null;
                }
                toastCanvasGroup.alpha = 1f;
            }

            // 표시 대기
            yield return new WaitForSecondsRealtime(duration);

            // 페이드 아웃
            if (toastCanvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < toastHideDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = 1f - (elapsed / toastHideDuration);
                    toastCanvasGroup.alpha = toastAnimationCurve.Evaluate(t);
                    yield return null;
                }
                toastCanvasGroup.alpha = 0f;
            }

            // 패널 비활성화
            toastPanel.SetActive(false);
            currentToastCoroutine = null;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 모든 메시지 패널 숨김
        /// </summary>
        public void HideAll()
        {
            ClosePopup();

            if (currentToastCoroutine != null)
            {
                StopCoroutine(currentToastCoroutine);
                currentToastCoroutine = null;
            }

            if (toastPanel != null)
                toastPanel.SetActive(false);
        }

        /// <summary>
        /// UnityGamePresenter에서 호출되는 이벤트 핸들러
        /// </summary>
        public void OnMessageShown(string message)
        {
            Show(message);
        }

        #endregion
    }
}
