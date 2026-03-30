using System.Collections.Generic;

namespace DessertKingdom.Core.Domain
{
    public class Activity
    {
        public string Id { get; }
        public string Name { get; }
        public ActivityType Type { get; }
        public int Cost { get; }
        public int Income { get; }
        public int MinAge { get; }
        public Dictionary<StatType, int> StatEffects { get; }
        public int StressChange { get; }
        public string Description { get; }

        public Activity(string id, string name, ActivityType type, int cost, int income, int minAge, 
                       Dictionary<StatType, int> statEffects, int stressChange, string description)
        {
            Id = id;
            Name = name;
            Type = type;
            Cost = cost;
            Income = income;
            MinAge = minAge;
            StatEffects = statEffects ?? new Dictionary<StatType, int>();
            StressChange = stressChange;
            Description = description;
        }

        public bool IsAvailable(int currentAge, CharacterStats stats)
        {
            if (currentAge < MinAge)
                return false;

            return true;
        }

        public int GetNetCost()
        {
            return Cost - Income;
        }

        public override string ToString()
        {
            return $"{Name} (비용: {Cost}, 수입: {Income})";
        }
    }
}
