using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 경제/자산 정보 표시 View
    /// 보유 자산, 수입, 지출 정보를 표시
    /// </summary>
    public class EconomyView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI incomeText;
        [SerializeField] private TextMeshProUGUI expenseText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image moneyIcon;

        [Header("Format Strings")]
        [SerializeField] private string moneyFormat = "{0} 스위트";
        [SerializeField] private string incomeFormat = "총 수입: +{0}";
        [SerializeField] private string expenseFormat = "총 지출: -{0}";
        [SerializeField] private string statusFormat = "({0})";

        [Header("Colors")]
        [SerializeField] private Color positiveColor = new Color(0.3f, 1f, 0.4f);
        [SerializeField] private Color negativeColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color neutralColor = Color.white;
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color dangerColor = new Color(1f, 0.3f, 0.3f);

        private int currentMoney = 0;

        private void Awake()
        {
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (moneyText == null)
                Debug.LogWarning("[EconomyView] moneyText is not assigned");
        }

        /// <summary>
        /// 경제 데이터를 표시
        /// </summary>
        public void Show(Economy economy)
        {
            if (economy == null)
            {
                Debug.LogWarning("[EconomyView] Economy is null");
                return;
            }

            currentMoney = economy.CurrentMoney;

            UpdateMoneyText(economy.CurrentMoney);
            UpdateIncomeText(economy.TotalIncome);
            UpdateExpenseText(economy.TotalExpense);
            UpdateStatusText(economy.GetFinancialStatus());
            UpdateMoneyIconColor(economy.CurrentMoney);
        }

        /// <summary>
        /// 단순 금액 표시 (UnityGamePresenter용)
        /// </summary>
        public void Show(int money)
        {
            currentMoney = money;
            UpdateMoneyText(money);
            UpdateMoneyIconColor(money);

            // 다른 텍스트는 비활성화
            if (incomeText != null)
                incomeText.gameObject.SetActive(false);
            if (expenseText != null)
                expenseText.gameObject.SetActive(false);
            if (statusText != null)
                statusText.gameObject.SetActive(false);
        }

        private void UpdateMoneyText(int money)
        {
            if (moneyText != null)
            {
                moneyText.text = string.Format(moneyFormat, money);
                moneyText.color = money >= 0 ? positiveColor : negativeColor;
            }
        }

        private void UpdateIncomeText(int income)
        {
            if (incomeText != null)
            {
                incomeText.text = string.Format(incomeFormat, income);
                incomeText.color = positiveColor;
                incomeText.gameObject.SetActive(true);
            }
        }

        private void UpdateExpenseText(int expense)
        {
            if (expenseText != null)
            {
                expenseText.text = string.Format(expenseFormat, expense);
                expenseText.color = negativeColor;
                expenseText.gameObject.SetActive(true);
            }
        }

        private void UpdateStatusText(string status)
        {
            if (statusText != null)
            {
                statusText.text = string.Format(statusFormat, status);
                statusText.color = GetStatusColor(status);
                statusText.gameObject.SetActive(true);
            }
        }

        private void UpdateMoneyIconColor(int money)
        {
            if (moneyIcon != null)
            {
                moneyIcon.color = GetStatusColor(GetFinancialStatus(money));
            }
        }

        private string GetFinancialStatus(int money)
        {
            return money switch
            {
                >= 2000 => "풍족",
                >= 1000 => "안정",
                >= 500 => "보통",
                >= 200 => "빠듯",
                _ => "위기"
            };
        }

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "풍족" => positiveColor,
                "안정" => neutralColor,
                "보통" => neutralColor,
                "빠듯" => warningColor,
                "위기" => dangerColor,
                _ => neutralColor
            };
        }

        /// <summary>
        /// UnityGamePresenter에서 호출되는 이벤트 핸들러
        /// </summary>
        public void OnEconomyUpdated(int money)
        {
            Show(money);
        }
    }
}
