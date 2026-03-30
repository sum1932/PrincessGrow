namespace DessertKingdom.Core.Domain
{
    public class CharacterStats
    {
        // 6개 기본 스탯
        public int HP { get; private set; }
        public int Charm { get; private set; }
        public int Intelligence { get; private set; }
        public int Art { get; private set; }
        public int Morality { get; private set; }
        public int Stress { get; private set; }

        private const int MinStatValue = 0;
        private const int MaxStatValue = 999;
        private const int MinStressValue = 0;
        private const int MaxStressValue = 100;

        public CharacterStats()
        {
            HP = 50;
            Charm = 50;
            Intelligence = 50;
            Art = 50;
            Morality = 50;
            Stress = 0;
        }

        public int GetStat(StatType type)
        {
            return type switch
            {
                StatType.HP => HP,
                StatType.Charm => Charm,
                StatType.Intelligence => Intelligence,
                StatType.Art => Art,
                StatType.Morality => Morality,
                StatType.Stress => Stress,
                _ => 0
            };
        }

        public void ModifyStat(StatType type, int amount)
        {
            switch (type)
            {
                case StatType.HP:
                    HP = Clamp(HP + amount, MinStatValue, MaxStatValue);
                    break;
                case StatType.Charm:
                    Charm = Clamp(Charm + amount, MinStatValue, MaxStatValue);
                    break;
                case StatType.Intelligence:
                    Intelligence = Clamp(Intelligence + amount, MinStatValue, MaxStatValue);
                    break;
                case StatType.Art:
                    Art = Clamp(Art + amount, MinStatValue, MaxStatValue);
                    break;
                case StatType.Morality:
                    Morality = Clamp(Morality + amount, MinStatValue, MaxStatValue);
                    break;
                case StatType.Stress:
                    Stress = Clamp(Stress + amount, MinStressValue, MaxStressValue);
                    break;
            }
        }

        public StatGrade GetGrade(StatType type)
        {
            int value = GetStat(type);
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

        public PersonalityType GetPersonality()
        {
            int threshold = 20; // 20% 이상 차이
            int baseValue = (HP + Charm + Intelligence + Art + Morality) / 5;

            if (HP > baseValue + threshold) return PersonalityType.Energetic;
            if (Intelligence > baseValue + threshold) return PersonalityType.Intellectual;
            if (Art > baseValue + threshold) return PersonalityType.Artistic;
            if (Charm > baseValue + threshold) return PersonalityType.Charming;
            if (Morality > baseValue + threshold) return PersonalityType.Virtuous;
            
            return PersonalityType.Balanced;
        }

        public StressLevel GetStressLevel()
        {
            return Stress switch
            {
                > 100 => StressLevel.Dangerous,
                > 90 => StressLevel.Critical,
                > 70 => StressLevel.Overworked,
                > 50 => StressLevel.Tired,
                > 30 => StressLevel.Good,
                _ => StressLevel.Normal
            };
        }

        public MoralityLevel GetMoralityLevel()
        {
            return Morality switch
            {
                < 200 => MoralityLevel.Evil,
                < 400 => MoralityLevel.Normal,
                _ => MoralityLevel.Good
            };
        }

        public double GetStressEfficiency()
        {
            return GetStressLevel() switch
            {
                StressLevel.Normal => 1.0,
                StressLevel.Good => 1.0,
                StressLevel.Tired => 0.9,
                StressLevel.Overworked => 0.75,
                StressLevel.Critical => 0.5,
                StressLevel.Dangerous => 0.3,
                _ => 1.0
            };
        }

        public override string ToString()
        {
            return $"체력:{HP} 매력:{Charm} 지능:{Intelligence} 예술:{Art} 도덕:{Morality} 스트레스:{Stress}";
        }

        private int Clamp(int value, int min, int max)
        {
            return System.Math.Clamp(value, min, max);
        }
    }
}
