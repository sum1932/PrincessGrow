using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ActionDatabase", menuName = "GameData/Databases/Action Database", order = 8)]
    public class ActionDatabase : ScriptableObject
    {
        [SerializeField] private List<ActionData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, ActionData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, ActionData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.ActionId))
                    _cache[item.ActionId] = item;
            }
            _initialized = true;
        }
        
        public ActionData Get(string actionId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(actionId);
        }
        
        public List<ActionData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<ActionData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        public List<ActionData> GetByCategory(string category)
        {
            return _data.Where(a => a.Category == category).ToList();
        }
        
        /// <summary>
        /// 특정 월에 표시되어야 하는 액션들을 반환
        /// Season: All(항상), Spring(3-5), Summer(6-8), Autumn(9-11), Winter(12,1-2)
        /// Month: 0이면 계절만 체크, 특정 월이면 해당 월에만 표시
        /// </summary>
        public List<ActionData> GetAvailableActions(int currentMonth)
        {
            return _data.Where(a => IsActionAvailable(a, currentMonth)).ToList();
        }
        
        private bool IsActionAvailable(ActionData action, int currentMonth)
        {
            // Season이 비어있거나 "All"이면 항상 사용 가능
            if (string.IsNullOrEmpty(action.Season) || action.Season == "All")
                return true;
            
            // 특정 월이 지정된 경우 (Month > 0)
            if (action.Month > 0)
            {
                return action.Month == currentMonth;
            }
            
            // 계절별 체크
            return action.Season.ToLower() switch
            {
                "spring" => currentMonth >= 3 && currentMonth <= 5,
                "summer" => currentMonth >= 6 && currentMonth <= 8,
                "autumn" => currentMonth >= 9 && currentMonth <= 11,
                "winter" => currentMonth == 12 || currentMonth <= 2,
                _ => true
            };
        }
        
        /// <summary>
        /// 카테고리와 계절/월을 모두 고려한 필터링
        /// </summary>
        public List<ActionData> GetAvailableActionsByCategory(string category, int currentMonth)
        {
            return _data.Where(a => a.Category == category && IsActionAvailable(a, currentMonth)).ToList();
        }
    }
}
