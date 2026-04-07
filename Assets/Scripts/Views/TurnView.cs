using TMPro;
using UnityEngine;
using DessertKingdom.Adapters.Interfaces;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 턴 정보 표시 View
    /// 상단 패널의 나이/월/턴 정보를 표시
    /// </summary>
    public class TurnView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI monthText;
        [SerializeField] private TextMeshProUGUI chapterText;
        [SerializeField] private TextMeshProUGUI seasonText;

        [Header("Format Strings")]
        [SerializeField] private string turnFormat = "턴 {0}/144";
        [SerializeField] private string ageFormat = "{0}세";
        [SerializeField] private string monthFormat = "{0}월";
        [SerializeField] private string chapterFormat = "챕터 {0}";
        [SerializeField] private string seasonFormat = "{0}";

        [Header("Season Colors")]
        [SerializeField] private Color springColor = new Color(0.4f, 1f, 0.4f);
        [SerializeField] private Color summerColor = new Color(1f, 0.4f, 0.4f);
        [SerializeField] private Color autumnColor = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private Color winterColor = new Color(0.4f, 0.8f, 1f);

        private void Awake()
        {
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (turnText == null)
                Debug.LogWarning("[TurnView] turnText is not assigned");
            if (ageText == null)
                Debug.LogWarning("[TurnView] ageText is not assigned");
            if (monthText == null)
                Debug.LogWarning("[TurnView] monthText is not assigned");
            if (chapterText == null)
                Debug.LogWarning("[TurnView] chapterText is not assigned");
            if (seasonText == null)
                Debug.LogWarning("[TurnView] seasonText is not assigned");
        }

        /// <summary>
        /// 턴 데이터를 표시
        /// </summary>
        public void Show(TurnDisplayData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[TurnView] TurnDisplayData is null");
                return;
            }

            UpdateTurnText(data.Turn);
            UpdateAgeText(data.Age);
            UpdateMonthText(data.Month);
            UpdateChapterText(data.Chapter);
            UpdateSeasonText(data.Season);
        }

        private void UpdateTurnText(int turn)
        {
            if (turnText != null)
                turnText.text = string.Format(turnFormat, turn);
        }

        private void UpdateAgeText(int age)
        {
            if (ageText != null)
                ageText.text = string.Format(ageFormat, age);
        }

        private void UpdateMonthText(int month)
        {
            if (monthText != null)
                monthText.text = string.Format(monthFormat, month);
        }

        private void UpdateChapterText(int chapter)
        {
            if (chapterText != null)
                chapterText.text = string.Format(chapterFormat, chapter);
        }

        private void UpdateSeasonText(string season)
        {
            if (seasonText != null)
            {
                seasonText.text = string.Format(seasonFormat, season);
                seasonText.color = GetSeasonColor(season);
            }
        }

        private Color GetSeasonColor(string season)
        {
            return season?.ToLower() switch
            {
                "spring" => springColor,
                "summer" => summerColor,
                "autumn" => autumnColor,
                "winter" => winterColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// UnityGamePresenter에서 호출되는 이벤트 핸들러
        /// </summary>
        public void OnTurnUpdated(TurnDisplayData data)
        {
            Show(data);
        }
    }
}
