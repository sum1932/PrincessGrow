using System.Collections.Generic;
using UnityEngine;

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
        public Dictionary<string, object> Requirements { get; }
        public string Season { get; }
        public int Month { get; }

        public Activity(string id, string name, ActivityType type, int cost, int income, int minAge, 
                       Dictionary<StatType, int> statEffects, int stressChange, string description,
                       Dictionary<string, object> requirements = null, string season = null, int month = 0)
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
            Requirements = requirements ?? new Dictionary<string, object>();
            Season = season ?? "All";
            Month = month;
        }

        public bool IsAvailable(int currentAge, CharacterStats stats)
        {
            // 나이 체크
            if (currentAge < MinAge)
            {
                Debug.Log($"[Activity:{Name}] 나이 불만족: 현재 {currentAge} < 필요 {MinAge}");
                return false;
            }

            // 스탯 요구사항 체크
            if (stats != null && Requirements != null && Requirements.Count > 0)
            {
                Debug.Log($"[Activity:{Name}] 스탯 체크 시작 - Requirements 수: {Requirements.Count}");
                
                foreach (var req in Requirements)
                {
                    if (req.Key.StartsWith("stat_"))
                    {
                        string statName = req.Key.Substring(5);
                        string enumName = ConvertToEnumStatName(statName);
                        
                        if (System.Enum.TryParse<StatType>(enumName, true, out var statType))
                        {
                            int requiredValue = GetIntValue(req.Value);
                            int currentValue = stats.GetStat(statType);
                            
                            if (currentValue < requiredValue)
                            {
                                Debug.Log($"[Activity:{Name}] 스탯 불만족: {statType}");
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool IsAvailable(int currentAge, CharacterStats stats, int currentMonth)
        {
            // 기본 나이/스탯 체크
            if (!IsAvailable(currentAge, stats))
                return false;

            // 계절/월 체크
            if (!IsAvailableInMonth(currentMonth))
            {
                Debug.Log($"[Activity:{Name}] 계절/월 불만족: 현재 {currentMonth}월, Season={Season}, Month={Month}");
                return false;
            }

            return true;
        }

        private bool IsAvailableInMonth(int currentMonth)
        {
            // Season이 비어있거나 "All"이면 항상 사용 가능
            if (string.IsNullOrEmpty(Season) || Season == "All")
                return true;
            
            // 특정 월이 지정된 경우 (Month > 0)
            if (Month > 0)
            {
                return Month == currentMonth;
            }
            
            // 계절별 체크
            return Season.ToLower() switch
            {
                "spring" => currentMonth >= 3 && currentMonth <= 5,
                "summer" => currentMonth >= 6 && currentMonth <= 8,
                "autumn" => currentMonth >= 9 && currentMonth <= 11,
                "winter" => currentMonth == 12 || currentMonth <= 2,
                _ => true
            };
        }

        public bool CanAfford(int currentMoney)
        {
            return currentMoney >= Cost;
        }

        public string GetRequirementText()
        {
            var texts = new List<string>();
            
            if (MinAge > 0)
                texts.Add($"{MinAge}세 이상");
            
            foreach (var req in Requirements)
            {
                if (req.Key.StartsWith("stat_"))
                {
                    string statName = req.Key.Substring(5);
                    string statKoreanName = GetStatKoreanName(statName);
                    texts.Add($"{statKoreanName} {req.Value} 이상");
                }
            }
            
            // 계절/월 요구사항 표시
            if (!string.IsNullOrEmpty(Season) && Season != "All")
            {
                if (Month > 0)
                {
                    texts.Add($"{Month}월");
                }
                else
                {
                    string seasonKorean = Season.ToLower() switch
                    {
                        "spring" => "봄",
                        "summer" => "여름",
                        "autumn" => "가을",
                        "winter" => "겨울",
                        _ => Season
                    };
                    texts.Add($"{seasonKorean}");
                }
            }
            
            return string.Join(", ", texts);
        }

        private string GetStatKoreanName(string statName)
        {
            return statName.ToLower() switch
            {
                "hp" => "체력",
                "charm" => "매력",
                "intelligence" => "지능",
                "art" => "예술",
                "morality" => "도덕",
                "stress" => "스트레스",
                _ => statName
            };
        }

        private int GetIntValue(object value)
        {
            return System.Convert.ToInt32(value);
        }

        /// <summary>
        /// CSV 스탯 이름을 Enum 스탯 이름으로 변환
        /// CSV: Int -> Enum: Intelligence
        /// </summary>
        private string ConvertToEnumStatName(string statName)
        {
            return statName.ToLower().Trim() switch
            {
                "int" => "Intelligence",
                "hp" => "HP",
                "charm" => "Charm",
                "art" => "Art",
                "morality" => "Morality",
                "stress" => "Stress",
                _ => statName
            };
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
