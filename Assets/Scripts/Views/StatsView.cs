using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;
using DessertKingdom.Adapters.Interfaces;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 캐릭터 스탯 표시 View
    /// 6개 기본 스탯을 슬라이더와 텍스트로 표시
    /// </summary>
    public class StatsView : MonoBehaviour
    {
        [Header("HP")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpValueText;
        [SerializeField] private TextMeshProUGUI hpGradeText;

        [Header("Charm")]
        [SerializeField] private Slider charmSlider;
        [SerializeField] private TextMeshProUGUI charmValueText;
        [SerializeField] private TextMeshProUGUI charmGradeText;

        [Header("Intelligence")]
        [SerializeField] private Slider intelligenceSlider;
        [SerializeField] private TextMeshProUGUI intelligenceValueText;
        [SerializeField] private TextMeshProUGUI intelligenceGradeText;

        [Header("Art")]
        [SerializeField] private Slider artSlider;
        [SerializeField] private TextMeshProUGUI artValueText;
        [SerializeField] private TextMeshProUGUI artGradeText;

        [Header("Morality")]
        [SerializeField] private Slider moralitySlider;
        [SerializeField] private TextMeshProUGUI moralityValueText;
        [SerializeField] private TextMeshProUGUI moralityGradeText;

        [Header("Stress")]
        [SerializeField] private Slider stressSlider;
        [SerializeField] private TextMeshProUGUI stressValueText;
        [SerializeField] private TextMeshProUGUI stressGradeText;

        [Header("Personality")]
        [SerializeField] private TextMeshProUGUI personalityText;

        [Header("Settings")]
        [SerializeField] private int maxStatValue = 999;
        [SerializeField] private bool useGradeColors = true;

        [Header("Grade Colors")]
        [SerializeField] private Color gradeFColor = Color.gray;
        [SerializeField] private Color gradeEColor = new Color(0.6f, 1f, 0.6f);
        [SerializeField] private Color gradeDColor = new Color(0.4f, 1f, 0.4f);
        [SerializeField] private Color gradeCColor = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color gradeBColor = new Color(0.2f, 0.8f, 1f);
        [SerializeField] private Color gradeAColor = new Color(0.4f, 0.4f, 1f);
        [SerializeField] private Color gradeSColor = new Color(0.8f, 0.4f, 1f);
        [SerializeField] private Color gradeSSColor = new Color(1f, 0.4f, 0.8f);

        private void Awake()
        {
            InitializeSliders();
        }

        private void InitializeSliders()
        {
            SetupSlider(hpSlider);
            SetupSlider(charmSlider);
            SetupSlider(intelligenceSlider);
            SetupSlider(artSlider);
            SetupSlider(moralitySlider);
            SetupSlider(stressSlider);
        }

        private void SetupSlider(Slider slider)
        {
            if (slider != null)
            {
                slider.minValue = 0;
                slider.maxValue = maxStatValue;
            }
        }

        /// <summary>
        /// 스탯 데이터를 표시
        /// </summary>
        public void Show(CharacterStats stats)
        {
            if (stats == null)
            {
                Debug.LogWarning("[StatsView] CharacterStats is null");
                return;
            }

            UpdateStat(StatType.HP, stats.GetStat(StatType.HP), stats.GetGrade(StatType.HP),
                      hpSlider, hpValueText, hpGradeText);
            UpdateStat(StatType.Charm, stats.GetStat(StatType.Charm), stats.GetGrade(StatType.Charm),
                      charmSlider, charmValueText, charmGradeText);
            UpdateStat(StatType.Intelligence, stats.GetStat(StatType.Intelligence), stats.GetGrade(StatType.Intelligence),
                      intelligenceSlider, intelligenceValueText, intelligenceGradeText);
            UpdateStat(StatType.Art, stats.GetStat(StatType.Art), stats.GetGrade(StatType.Art),
                      artSlider, artValueText, artGradeText);
            UpdateStat(StatType.Morality, stats.GetStat(StatType.Morality), stats.GetGrade(StatType.Morality),
                      moralitySlider, moralityValueText, moralityGradeText);
            UpdateStat(StatType.Stress, stats.GetStat(StatType.Stress), stats.GetGrade(StatType.Stress),
                      stressSlider, stressValueText, stressGradeText);

            UpdatePersonality(stats.GetPersonality());
        }

        /// <summary>
        /// StatsDisplayData로부터 스탯 표시
        /// </summary>
        public void Show(StatsDisplayData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[StatsView] StatsDisplayData is null");
                return;
            }

            UpdateStat(StatType.HP, data.HP, CalculateGrade(data.HP), hpSlider, hpValueText, hpGradeText);
            UpdateStat(StatType.Charm, data.Charm, CalculateGrade(data.Charm), charmSlider, charmValueText, charmGradeText);
            UpdateStat(StatType.Intelligence, data.Intelligence, CalculateGrade(data.Intelligence), intelligenceSlider, intelligenceValueText, intelligenceGradeText);
            UpdateStat(StatType.Art, data.Art, CalculateGrade(data.Art), artSlider, artValueText, artGradeText);
            UpdateStat(StatType.Morality, data.Morality, CalculateGrade(data.Morality), moralitySlider, moralityValueText, moralityGradeText);
            UpdateStat(StatType.Stress, data.Stress, CalculateGrade(data.Stress), stressSlider, stressValueText, stressGradeText);

            if (personalityText != null && !string.IsNullOrEmpty(data.Personality))
                personalityText.text = GetPersonalityDisplayName(data.Personality);
        }

        private void UpdateStat(StatType type, int value, StatGrade grade,
                               Slider slider, TextMeshProUGUI valueText, TextMeshProUGUI gradeText)
        {
            if (slider != null)
                slider.value = value;

            if (valueText != null)
                valueText.text = value.ToString();

            if (gradeText != null)
            {
                gradeText.text = grade.ToString();
                if (useGradeColors)
                    gradeText.color = GetGradeColor(grade);
            }
        }

        private void UpdatePersonality(PersonalityType personality)
        {
            if (personalityText != null)
                personalityText.text = GetPersonalityDisplayName(personality.ToString());
        }

        private StatGrade CalculateGrade(int value)
        {
            return value switch
            {
                >= 700 => StatGrade.SS,
                >= 600 => StatGrade.S,
                >= 500 => StatGrade.A,
                >= 400 => StatGrade.B,
                >= 300 => StatGrade.C,
                >= 200 => StatGrade.D,
                >= 100 => StatGrade.E,
                _ => StatGrade.F
            };
        }

        private Color GetGradeColor(StatGrade grade)
        {
            return grade switch
            {
                StatGrade.F => gradeFColor,
                StatGrade.E => gradeEColor,
                StatGrade.D => gradeDColor,
                StatGrade.C => gradeCColor,
                StatGrade.B => gradeBColor,
                StatGrade.A => gradeAColor,
                StatGrade.S => gradeSColor,
                StatGrade.SS => gradeSSColor,
                _ => Color.white
            };
        }

        private string GetPersonalityDisplayName(string personality)
        {
            return personality?.ToLower() switch
            {
                "energetic" => "활발한",
                "intellectual" => "지적인",
                "artistic" => "예술적인",
                "charming" => "매력적인",
                "virtuous" => "품격있는",
                "balanced" => "균형잡힌",
                _ => personality
            };
        }

        /// <summary>
        /// UnityGamePresenter에서 호출되는 이벤트 핸들러
        /// </summary>
        public void OnStatsUpdated(StatsDisplayData data)
        {
            Show(data);
        }
    }
}
