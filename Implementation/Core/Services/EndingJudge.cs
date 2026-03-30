using System;
using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Services
{
    public interface IEndingJudge
    {
        event Action<EndingCondition> OnEndingDetermined;
        EndingCondition EvaluateEnding(GameState finalState);
    }

    public class EndingJudge : IEndingJudge
    {
        public event Action<EndingCondition> OnEndingDetermined;
        
        private readonly List<EndingCondition> _endings;
        
        public EndingJudge(List<EndingCondition> endings)
        {
            _endings = endings ?? new List<EndingCondition>();
        }
        
        public EndingCondition EvaluateEnding(GameState finalState)
        {
            // 히든 엔딩 체크 (우선순위 1-2)
            foreach (var ending in _endings.Where(e => e.IsHidden).OrderBy(e => e.Priority))
            {
                if (ending.IsMet(finalState))
                {
                    OnEndingDetermined?.Invoke(ending);
                    return ending;
                }
            }
            
            // 기본 엔딩 체크 (우선순위 3-17)
            foreach (var ending in _endings.Where(e => !e.IsHidden).OrderBy(e => e.Priority))
            {
                if (ending.IsMet(finalState))
                {
                    OnEndingDetermined?.Invoke(ending);
                    return ending;
                }
            }
            
            // 기본값: 평범한 행복
            var defaultEnding = _endings.FirstOrDefault(e => e.Id == "normal_happiness") 
                              ?? new EndingCondition("normal_happiness", "평범한 행복", 17, 
                                                     new Dictionary<StatType, int>(),
                                                     new Dictionary<string, int>(),
                                                     new List<string>(), new List<string>(),
                                                     false, "특별하지 않지만 행복한 삶");
            
            OnEndingDetermined?.Invoke(defaultEnding);
            return defaultEnding;
        }
    }
}
