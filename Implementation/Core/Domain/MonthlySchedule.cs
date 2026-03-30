namespace DessertKingdom.Core.Domain
{
    public class MonthlySchedule
    {
        public Activity Activity1 { get; private set; }
        public Activity Activity2 { get; private set; }
        public bool IsComplete => Activity1 != null && Activity2 != null;

        public bool SetActivity(Activity activity, int slot)
        {
            if (slot != 1 && slot != 2)
                return false;

            if (slot == 1)
                Activity1 = activity;
            else
                Activity2 = activity;

            return true;
        }

        public void Clear()
        {
            Activity1 = null;
            Activity2 = null;
        }

        public int GetTotalCost()
        {
            int cost = 0;
            if (Activity1 != null) cost += Activity1.Cost;
            if (Activity2 != null) cost += Activity2.Cost;
            return cost;
        }

        public int GetTotalIncome()
        {
            int income = 0;
            if (Activity1 != null) income += Activity1.Income;
            if (Activity2 != null) income += Activity2.Income;
            return income;
        }

        public void ApplyEffects(CharacterStats stats)
        {
            double efficiency = stats.GetStressEfficiency();

            if (Activity1 != null)
            {
                ApplyActivityEffects(Activity1, stats, efficiency);
            }

            if (Activity2 != null)
            {
                ApplyActivityEffects(Activity2, stats, efficiency);
            }
        }

        private void ApplyActivityEffects(Activity activity, CharacterStats stats, double efficiency)
        {
            foreach (var effect in activity.StatEffects)
            {
                int modifiedAmount = (int)(effect.Value * efficiency);
                stats.ModifyStat(effect.Key, modifiedAmount);
            }
            
            stats.ModifyStat(StatType.Stress, activity.StressChange);
        }

        public int GetTotalStressChange()
        {
            int stress = 0;
            if (Activity1 != null) stress += Activity1.StressChange;
            if (Activity2 != null) stress += Activity2.StressChange;
            return stress;
        }

        public override string ToString()
        {
            string a1 = Activity1?.Name ?? "미선택";
            string a2 = Activity2?.Name ?? "미선택";
            return $"활동1: {a1}, 활동2: {a2}";
        }
    }
}
