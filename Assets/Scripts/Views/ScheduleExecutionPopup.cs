using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 스케줄 실행 팝업 뷰
    /// 첫 번째/두 번째 스케줄을 15일 동안 진행하는 것을 보여줌
    /// </summary>
    public class ScheduleExecutionPopup : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private GameObject phase1Panel;

        [Header("Phase Display")]
        [SerializeField] private Image phaseActivityIcon;
        [SerializeField] private TextMeshProUGUI phaseActivityName;
        [SerializeField] private TextMeshProUGUI phaseActivityDesc;
        [SerializeField] private TextMeshProUGUI phaseDayText;
        [SerializeField] private Slider phaseProgressBar;
        [SerializeField] private Image phaseCharacterImage;

        [Header("Event Notification")]
        [SerializeField] private GameObject eventNotificationPanel;
        [SerializeField] private TextMeshProUGUI eventNotificationText;
        [SerializeField] private float eventNotificationDuration = 2f;

        [Header("Settings")]
        [SerializeField] private float dayDuration = 0.3f; // 하루 진행 시간 (초)
        [SerializeField] private int totalDays = 15;
        [SerializeField] private bool pauseGameWhenShown = true;

        // 이벤트
        public event Action OnPhase1Completed;
        public event Action OnPhase2Completed;
        public event Action OnScheduleExecutionCompleted;
        public event Action<GameEvent> OnEventTriggered;

        // 상태
        private Activity activity1;
        private Activity activity2;
        private int currentPhase = 0;
        private int currentDay = 0;
        private bool isExecuting = false;
        private Coroutine executionCoroutine;

        void Awake()
        {
            InitializePanel();
        }

        private void InitializePanel()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);
            if (eventNotificationPanel != null)
                eventNotificationPanel.SetActive(false);
        }

        /// <summary>
        /// 스케줄 실행 시작
        /// </summary>
        public void StartScheduleExecution(Activity act1, Activity act2)
        {
            activity1 = act1;
            activity2 = act2;
            currentPhase = 0;
            currentDay = 0;

            if (popupPanel != null)
                popupPanel.SetActive(true);

            if (pauseGameWhenShown)
                Time.timeScale = 0f;

            // 첫 번째 페이즈 시작
            StartPhase1();
        }

        /// <summary>
        /// 첫 번째 스케줄 실행 (15일)
        /// </summary>
        private void StartPhase1()
        {
            currentPhase = 1;
            currentDay = 0;

            UpdatePhaseDisplay(activity1);

            // 실행 시작
            if (executionCoroutine != null)
                StopCoroutine(executionCoroutine);
            executionCoroutine = StartCoroutine(ExecutePhase(activity1));
        }

        /// <summary>
        /// 두 번째 스케줄 실행 (15일)
        /// </summary>
        private void StartPhase2()
        {
            currentPhase = 2;
            currentDay = 0;

            UpdatePhaseDisplay(activity2);

            // 실행 시작
            if (executionCoroutine != null)
                StopCoroutine(executionCoroutine);
            executionCoroutine = StartCoroutine(ExecutePhase(activity2));
        }

        /// <summary>
        /// 페이즈 실행 코루틴
        /// </summary>
        private IEnumerator ExecutePhase(Activity activity)
        {
            isExecuting = true;

            for (int day = 1; day <= totalDays; day++)
            {
                currentDay = day;
                UpdateDayDisplay(day);

                // 일정 확률로 이벤트 발생 (예: 10%)
                if (UnityEngine.Random.value < 0.1f)
                {
                    yield return new WaitForSecondsRealtime(dayDuration * 0.5f);
                    ShowRandomEventNotification();
                    yield return new WaitForSecondsRealtime(dayDuration * 0.5f);
                }
                else
                {
                    yield return new WaitForSecondsRealtime(dayDuration);
                }
            }

            isExecuting = false;

            // 페이즈 완료
            if (currentPhase == 1)
            {
                OnPhase1Completed?.Invoke();

                // 두 번째 페이즈 시작
                yield return new WaitForSecondsRealtime(0.5f);
                StartPhase2();
            }
            else if (currentPhase == 2)
            {
                OnPhase2Completed?.Invoke();

                // 모든 스케줄 완료
                yield return new WaitForSecondsRealtime(0.5f);
                CompleteScheduleExecution();
            }
        }

        /// <summary>
        /// 페이즈 UI 업데이트
        /// </summary>
        private void UpdatePhaseDisplay(Activity activity)
        {
            if (activity == null) return;

            if (phaseActivityName != null)
                phaseActivityName.text = activity.Name;

            if (phaseActivityDesc != null)
                phaseActivityDesc.text = GetActivityDescription(activity);

            if (phaseActivityIcon != null)
                phaseActivityIcon.color = GetActivityColor(activity.Type);

            if (phaseProgressBar != null)
            {
                phaseProgressBar.value = 0f;
                phaseProgressBar.maxValue = totalDays;
            }
        }

        /// <summary>
        /// 날짜 표시 업데이트
        /// </summary>
        private void UpdateDayDisplay(int day)
        {
            if (phaseDayText != null)
                phaseDayText.text = $"{day} / {totalDays}일";

            if (phaseProgressBar != null)
                phaseProgressBar.value = day;
        }

        /// <summary>
        /// 랜덤 이벤트 알림 표시
        /// </summary>
        private void ShowRandomEventNotification()
        {
            if (eventNotificationPanel != null)
            {
                eventNotificationPanel.SetActive(true);

                if (eventNotificationText != null)
                {
                    string[] eventMessages = new string[]
                    {
                        "특별한 일이 발생했습니다!",
                        "뜻밖의 만남이 있었습니다.",
                        "행운이 찾아왔습니다!",
                        "새로운 발견을 했습니다."
                    };
                    eventNotificationText.text = eventMessages[UnityEngine.Random.Range(0, eventMessages.Length)];
                }

                StartCoroutine(HideEventNotification());
            }
        }

        private IEnumerator HideEventNotification()
        {
            yield return new WaitForSecondsRealtime(eventNotificationDuration);

            if (eventNotificationPanel != null)
                eventNotificationPanel.SetActive(false);
        }

        /// <summary>
        /// 스케줄 실행 완료
        /// </summary>
        private void CompleteScheduleExecution()
        {
            OnScheduleExecutionCompleted?.Invoke();
            ClosePopup();
        }

        /// <summary>
        /// 강제 종료
        /// </summary>
        public void ForceComplete()
        {
            if (executionCoroutine != null)
                StopCoroutine(executionCoroutine);

            CompleteScheduleExecution();
        }

        private void ClosePopup()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            if (eventNotificationPanel != null)
                eventNotificationPanel.SetActive(false);

            if (pauseGameWhenShown)
                Time.timeScale = 1f;

            isExecuting = false;
        }

        /// <summary>
        /// 활동 설명 생성
        /// </summary>
        private string GetActivityDescription(Activity activity)
        {
            if (activity == null) return "";

            var desc = activity.Description;
            if (string.IsNullOrEmpty(desc))
            {
                desc = activity.Type switch
                {
                    ActivityType.Lesson => "열심히 수업을 듣고 있습니다...",
                    ActivityType.PartTime => "일하며 경험을 쌓고 있습니다...",
                    ActivityType.Rest => "휴식을 취하며 재충전 중입니다...",
                    ActivityType.Outing => "외출하며 새로운 경험을 하고 있습니다...",
                    ActivityType.Special => "특별한 활동을 하고 있습니다...",
                    _ => "활동 중입니다..."
                };
            }
            return desc;
        }

        /// <summary>
        /// 활동 타입별 색상
        /// </summary>
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

        /// <summary>
        /// 현재 실행 중인지 확인
        /// </summary>
        public bool IsExecuting => isExecuting;

        /// <summary>
        /// 현재 페이즈 반환 (0: 없음, 1: 첫 번째, 2: 두 번째)
        /// </summary>
        public int CurrentPhase => currentPhase;
    }
}
