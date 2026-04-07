using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DessertKingdom.Controllers
{
    /// <summary>
    /// 타이틀 씬 컨트롤러
    /// 게임 시작 버튼 등 타이틀 화면의 상호작용 처리
    /// </summary>
    public class TitleSceneController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitGameButton;

        [Header("Settings")]
        [SerializeField] private string mainSceneName = "Main";

        void Start()
        {
            SetupButtons();
        }

        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtons()
        {
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
            else
            {
                Debug.LogWarning("[TitleSceneController] 시작 버튼이 연결되지 않았습니다.");
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.AddListener(OnQuitGameClicked);
            }
        }

        /// <summary>
        /// 게임 시작 버튼 클릭 시 호출
        /// </summary>
        public void OnStartGameClicked()
        {
            Debug.Log("[TitleSceneController] 게임 시작 버튼 클릭");
            LoadMainScene();
        }

        /// <summary>
        /// 메인 씬으로 전환
        /// </summary>
        private void LoadMainScene()
        {
            Debug.Log($"[TitleSceneController] 메인 씬 로드: {mainSceneName}");
            SceneManager.LoadScene(mainSceneName);
        }

        /// <summary>
        /// 게임 종료 버튼 클릭 시 호출
        /// </summary>
        public void OnQuitGameClicked()
        {
            Debug.Log("[TitleSceneController] 게임 종료 버튼 클릭");
            QuitGame();
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        private void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        void OnDestroy()
        {
            // 버튼 이벤트 해제 (메모리 누수 방지)
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveListener(OnStartGameClicked);
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.RemoveListener(OnQuitGameClicked);
            }
        }
    }
}
