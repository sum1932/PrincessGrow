namespace DessertKingdom.Core.Domain
{
    public class Economy
    {
        public int CurrentMoney { get; private set; }
        public int TotalIncome { get; private set; }
        public int TotalExpense { get; private set; }

        private const int BaseLivingCost = 200; // 고정 생활비
        private const int BaseAllowance = 200;  // 기본 생활비 지원

        public Economy()
        {
            CurrentMoney = 500; // 시작 자금
            TotalIncome = 0;
            TotalExpense = 0;
        }

        public bool CanAfford(int amount)
        {
            return CurrentMoney >= amount;
        }

        public bool Spend(int amount, string reason)
        {
            if (!CanAfford(amount))
                return false;

            CurrentMoney -= amount;
            TotalExpense += amount;
            return true;
        }

        public void Earn(int amount, string reason)
        {
            CurrentMoney += amount;
            TotalIncome += amount;
        }

        public void ProcessMonthly()
        {
            // 기본 생활비 지원
            Earn(BaseAllowance, "기본 생활비");
            
            // 고정 생활비 지출
            Spend(BaseLivingCost, "생활비");
        }

        public int GetMonthlyBalance()
        {
            return BaseAllowance - BaseLivingCost;
        }

        public string GetFinancialStatus()
        {
            return CurrentMoney switch
            {
                >= 2000 => "풍족",
                >= 1000 => "안정",
                >= 500 => "보통",
                >= 200 => "빠듯",
                _ => "위기"
            };
        }

        public override string ToString()
        {
            return $"보유: {CurrentMoney} 수입: {TotalIncome} 지출: {TotalExpense}";
        }
    }
}
