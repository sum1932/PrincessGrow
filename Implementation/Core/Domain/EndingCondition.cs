using System.Collections.Generic;
using System.Linq;

namespace DessertKingdom.Core.Domain
{
    public class EndingCondition
    {
        public string Id { get; }
        public string Name { get; }
        public int Priority { get; }
        public Dictionary<StatType, int> RequiredStats { get; }
        public Dictionary<string, int> RequiredFavor { get; }
        public List<string> RequiredEvents { get; }
        public List<string> RequiredChoices { get; }
        public bool IsHidden { get; }
        public string Description { get; }

        public EndingCondition(string id, string name, int priority, Dictionary<StatType, int> requiredStats,
                              Dictionary<string, int> requiredFavor, List<string> requiredEvents,
                              List<string> requiredChoices, bool isHidden, string description)
        {
            Id = id;
            Name = name;
            Priority = priority;
            RequiredStats = requiredStats ?? new Dictionary<StatType, int>();
            RequiredFavor = requiredFavor ?? new Dictionary<string, int>();
            RequiredEvents = requiredEvents ?? new List<string>();
            RequiredChoices = requiredChoices ?? new List<string>();
            IsHidden = isHidden;
            Description = description;
        }

        public bool IsMet(GameState state)
        {
            // 스탯 조건 체크
            foreach (var statReq in RequiredStats)
            {
                if (state.Character.GetStat(statReq.Key) < statReq.Value)
                    return false;
            }

            // 호감도 조건 체크
            foreach (var favorReq in RequiredFavor)
            {
                var npc = state.GetNPC(favorReq.Key);
                if (npc == null || npc.Favorability < favorReq.Value)
                    return false;
            }

            // 이벤트 완료 체크
            foreach (var eventId in RequiredEvents)
            {
                if (!state.CompletedEvents.Contains(eventId))
                    return false;
            }

            // 선택지 체크
            foreach (var choiceId in RequiredChoices)
            {
                if (!state.ChoiceHistory.Contains(choiceId))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"[{Priority}] {Name}";
        }
    }
}
