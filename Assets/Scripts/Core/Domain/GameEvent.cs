using System.Collections.Generic;

namespace DessertKingdom.Core.Domain
{
    public class GameEvent
    {
        public string Id { get; }
        public string Name { get; }
        public EventType Type { get; }
        public int Priority { get; }
        public string Description { get; }
        public Dictionary<string, object> Conditions { get; }
        public List<EventChoice> Choices { get; }
        public Dictionary<StatType, int> StatEffects { get; }
        public Dictionary<string, int> FavorEffects { get; }
        public bool IsOneTime { get; }
        public bool IsHidden { get; }

        public GameEvent(string id, string name, EventType type, int priority, string description,
                        Dictionary<string, object> conditions, List<EventChoice> choices,
                        Dictionary<StatType, int> statEffects, Dictionary<string, int> favorEffects,
                        bool isOneTime, bool isHidden)
        {
            Id = id;
            Name = name;
            Type = type;
            Priority = priority;
            Description = description;
            Conditions = conditions ?? new Dictionary<string, object>();
            Choices = choices ?? new List<EventChoice>();
            StatEffects = statEffects ?? new Dictionary<StatType, int>();
            FavorEffects = favorEffects ?? new Dictionary<string, int>();
            IsOneTime = isOneTime;
            IsHidden = isHidden;
        }

        public bool CanTrigger(GameState state)
        {
            // 연령 체크
            if (Conditions.ContainsKey("minAge"))
            {
                int minAge = GetIntValue(Conditions["minAge"]);
                if (state.Turn.CurrentAge < minAge) return false;
            }

            if (Conditions.ContainsKey("maxAge"))
            {
                int maxAge = GetIntValue(Conditions["maxAge"]);
                if (state.Turn.CurrentAge > maxAge) return false;
            }

            // 월/일 체크
            if (Conditions.ContainsKey("month"))
            {
                int month = GetIntValue(Conditions["month"]);
                if (state.Turn.CurrentMonth != month) return false;
            }

            // 스탯 체크
            foreach (var condition in Conditions)
            {
                if (condition.Key.StartsWith("stat_"))
                {
                    string statName = condition.Key.Substring(5);
                    if (!System.Enum.TryParse<StatType>(statName, true, out var statType))
                        continue;

                    int requiredValue = GetIntValue(condition.Value);
                    if (state.Character.GetStat(statType) < requiredValue)
                        return false;
                }

                if (condition.Key.StartsWith("favor_"))
                {
                    string npcId = condition.Key.Substring(6);
                    var npc = state.GetNPC(npcId);
                    if (npc == null || npc.Favorability < GetIntValue(condition.Value))
                        return false;
                }
            }

            // 이미 완료된 이벤트 체크
            if (IsOneTime && state.CompletedEvents.Contains(Id))
                return false;

            return true;
        }

        public override string ToString()
        {
            return $"[{Type}] {Name} (우선순위: {Priority})";
        }

        private int GetIntValue(object value)
        {
            return System.Convert.ToInt32(value);
        }
    }

    public class EventChoice
    {
        public string Id { get; }
        public string Text { get; }
        public Dictionary<StatType, int> StatEffects { get; }
        public Dictionary<string, int> FavorEffects { get; }
        public Dictionary<string, object> Requirements { get; }

        public EventChoice(string id, string text, Dictionary<StatType, int> statEffects,
                          Dictionary<string, int> favorEffects, Dictionary<string, object> requirements)
        {
            Id = id;
            Text = text;
            StatEffects = statEffects ?? new Dictionary<StatType, int>();
            FavorEffects = favorEffects ?? new Dictionary<string, int>();
            Requirements = requirements ?? new Dictionary<string, object>();
        }

        public bool IsAvailable(GameState state)
        {
            // 선택지 요구사항 체크
            foreach (var req in Requirements)
            {
                if (req.Key.StartsWith("stat_"))
                {
                    string statName = req.Key.Substring(5);
                    if (!System.Enum.TryParse<StatType>(statName, true, out var statType))
                        continue;

                    int requiredValue = GetIntValue(req.Value);
                    if (state.Character.GetStat(statType) < requiredValue)
                        return false;
                }
            }
            return true;
        }

        private int GetIntValue(object value)
        {
            return System.Convert.ToInt32(value);
        }
    }
}
