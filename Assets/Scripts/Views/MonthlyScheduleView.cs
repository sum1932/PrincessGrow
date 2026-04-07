using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 월간 스케줄 표시 View
    /// 1번/2번 활동 슬롯과 예상 결과를 표시
    /// </summary>
    public class MonthlyScheduleView : MonoBehaviour
    {
        [Header("Activity Slots")]
        [SerializeField] private Button activity1Slot;  // 1번 활동 슬롯
        [SerializeField] private Button activity2Slot;  // 2번 활동 슬롯
        [SerializeField] private TextMeshProUGUI activity1Text; // 1번 활동 이름 텍스트
        [SerializeField] private TextMeshProUGUI activity2Text; // 2번 활동 이름 텍스트
        [SerializeField] private Image activity1Icon;   // 1번 활동 아이콘 (색상으로 구분) => 이후에 이미지로 변경 가능
        [SerializeField] private Image activity2Icon;   // 2번 활동 아이콘 (색상으로 구분)  => 이후에 이미지로 변경 가능

        [Header("Preview Panel")]
        [SerializeField] private TextMeshProUGUI expectedMoneyText; // 예상 재화 변화 텍스트
        [SerializeField] private TextMeshProUGUI expectedStatsText; // 예상 능력치 변화 텍스트
        [SerializeField] private TextMeshProUGUI expectedEventsText;    // 예상 고정 이벤트 텍스트 (고정 이벤트는 GameController에서 제공 필요)

        [Header("Control Buttons")]
        [SerializeField] private Button proceedButton;  // 다음 단계로 진행 버튼
        [SerializeField] private Button resetButton;    // 초기화 버튼

        [Header("Visual Feedback")]
        [SerializeField] private Color selectedSlotColor = new Color(0.3f, 0.8f, 1f);   // 선택된 슬롯 색상 => 이후 UI 디자인에 맞게 조정 가능
        [SerializeField] private Color emptySlotColor = new Color(0.8f, 0.8f, 0.8f);    // 빈 슬롯 색상 => 이후 UI 디자인에 맞게 조정 가능

        public event Action<int> OnSlotSelected;  // 1 또는 2
        public event Action OnProceedClicked;   // 선택 완료 후 다음 단계로 진행
        public event Action OnResetClicked; // 선택 초기화

        private int currentSelectedSlot = 0;    // 0: 없음, 1: 첫 번째 슬롯, 2: 두 번째 슬롯
        private Activity selectedActivity1; // 1번 슬롯에 배정된 활동
        private Activity selectedActivity2; // 2번 슬롯에 배정된 활동

        private void Awake()
        {
            FindComponents();
            SetupButtons();
        }

        private void FindComponents()
        {
            // 자식 오브젝트 자동 찾기
            if (activity1Slot != null && activity1Text == null)
                activity1Text = activity1Slot.GetComponentInChildren<TextMeshProUGUI>();
            if (activity1Slot != null && activity1Icon == null)
                activity1Icon = activity1Slot.transform.Find("Icon")?.GetComponent<Image>();
            
            if (activity2Slot != null && activity2Text == null)
                activity2Text = activity2Slot.GetComponentInChildren<TextMeshProUGUI>();
            if (activity2Slot != null && activity2Icon == null)
                activity2Icon = activity2Slot.transform.Find("Icon")?.GetComponent<Image>();

            // 프리뷰 텍스트 찾기
            if (expectedMoneyText == null)
                expectedMoneyText = transform.Find("PreviewPanel/ExpectedMoneyText")?.GetComponent<TextMeshProUGUI>();
            if (expectedStatsText == null)
                expectedStatsText = transform.Find("PreviewPanel/ExpectedStatsText")?.GetComponent<TextMeshProUGUI>();

            // 디버그 로그
            Debug.Log($"[MonthlyScheduleView] Components found: " +
                $"Slot1Text={activity1Text != null}, Slot2Text={activity2Text != null}, " +
                $"Slot1Icon={activity1Icon != null}, Slot2Icon={activity2Icon != null}");
        }

        private void SetupButtons()
        {
            if (activity1Slot != null)
                activity1Slot.onClick.AddListener(() => SelectSlot(1));
            
            if (activity2Slot != null)
                activity2Slot.onClick.AddListener(() => SelectSlot(2));
            
            if (proceedButton != null)
                proceedButton.onClick.AddListener(() => OnProceedClicked?.Invoke());
            
            if (resetButton != null)
                resetButton.onClick.AddListener(() => OnResetClicked?.Invoke());
        }

        /// <summary>
        /// 슬롯 선택 (1번 또는 2번)
        /// </summary>
        public void SelectSlot(int slot)
        {
            currentSelectedSlot = slot;
            UpdateSlotVisuals();
            OnSlotSelected?.Invoke(slot);
        }

        /// <summary>
        /// 활동을 슬롯에 배정
        /// </summary>
        public void SetActivity(int slot, Activity activity)
        {
            Debug.Log($"[MonthlyScheduleView] SetActivity called: Slot={slot}, Activity={activity?.Name ?? "NULL"}");
            
            if (slot == 1)
            {
                selectedActivity1 = activity;
                Debug.Log($"  - activity1Text is null: {activity1Text == null}");
                UpdateSlotDisplay(activity1Text, activity1Icon, activity);
            }
            else if (slot == 2)
            {
                selectedActivity2 = activity;
                Debug.Log($"  - activity2Text is null: {activity2Text == null}");
                UpdateSlotDisplay(activity2Text, activity2Icon, activity);
            }

            UpdatePreview();
        }

        /// <summary>
        /// 선택된 활동들 반환
        /// </summary>
        public (Activity activity1, Activity activity2) GetSelectedActivities()
        {
            return (selectedActivity1, selectedActivity2);
        }

        /// <summary>
        /// 모든 선택 초기화
        /// </summary>
        public void ResetSchedule()
        {
            selectedActivity1 = null;
            selectedActivity2 = null;
            currentSelectedSlot = 0;
            
            UpdateSlotDisplay(activity1Text, activity1Icon, null);
            UpdateSlotDisplay(activity2Text, activity2Icon, null);
            UpdateSlotVisuals();
            UpdatePreview();
        }

        private void UpdateSlotDisplay(TextMeshProUGUI text, Image icon, Activity activity)
        {
            if (activity != null)
            {
                if (text != null)
                    text.text = activity.Name;
                if (icon != null)
                    icon.color = GetActivityColor(activity.Type);
            }
            else
            {
                if (text != null)
                    text.text = "활동 선택";
                if (icon != null)
                    icon.color = Color.gray;
            }
        }

        private void UpdateSlotVisuals()
        {
            if (activity1Slot != null)
            {
                var img = activity1Slot.GetComponent<Image>();
                if (img != null)
                    img.color = currentSelectedSlot == 1 ? selectedSlotColor : emptySlotColor;
            }

            if (activity2Slot != null)
            {
                var img = activity2Slot.GetComponent<Image>();
                if (img != null)
                    img.color = currentSelectedSlot == 2 ? selectedSlotColor : emptySlotColor;
            }
        }

        /// <summary>
        /// 예상 결과 업데이트
        /// </summary>
        private void UpdatePreview()
        {
            if (expectedMoneyText != null)
            {
                int totalCost = (selectedActivity1?.Cost ?? 0) + (selectedActivity2?.Cost ?? 0);
                int totalIncome = (selectedActivity1?.Income ?? 0) + (selectedActivity2?.Income ?? 0);
                int net = totalIncome - totalCost;
                expectedMoneyText.text = $"예상 재화 변화: {net:+0;-0;0} 스위트";
            }

            if (expectedStatsText != null)
            {
                var statsPreview = CalculateStatsPreview();
                expectedStatsText.text = string.IsNullOrEmpty(statsPreview) ? "능력치 변화 없음" : statsPreview;
            }

            if (expectedEventsText != null)
            {
                // 고정 이벤트 체크 (GameController에서 제공 필요)
                expectedEventsText.text = "고정 이벤트: 확인 중...";
            }
        }

        private string CalculateStatsPreview()
        {
            // 모든 스탯 효과 합산
            var combinedEffects = new Dictionary<StatType, int>();
            
            void AddActivityStats(Activity activity)
            {
                if (activity?.StatEffects == null) return;
                foreach (var effect in activity.StatEffects)
                {
                    if (combinedEffects.ContainsKey(effect.Key))
                        combinedEffects[effect.Key] += effect.Value;
                    else
                        combinedEffects[effect.Key] = effect.Value;
                }
            }

            AddActivityStats(selectedActivity1);
            AddActivityStats(selectedActivity2);

            // 합산된 결과를 문자열로 변환
            var preview = new System.Text.StringBuilder();
            foreach (var effect in combinedEffects)
            {
                if (preview.Length > 0) preview.Append(", ");
                
                string sign = effect.Value > 0 ? "+" : "";
                string statName = GetStatName(effect.Key);
                preview.Append($"{statName}: {sign}{effect.Value}");
            }

            return preview.ToString();
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
