using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 스탯 데이터베이스 - 모든 스탯 정의를 관리
    /// </summary>
    [CreateAssetMenu(fileName = "StatDatabase", menuName = "GameData/Databases/Stat Database", order = 2)]
    public class StatDatabase : ScriptableObject
    {
        [SerializeField] private List<StatData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, StatData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, StatData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.StatId))
                    _cache[item.StatId] = item;
            }
            _initialized = true;
        }
        
        public StatData Get(string statId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(statId);
        }
        
        public List<StatData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<StatData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 주요 스탯만 반환
        /// </summary>
        public List<StatData> GetPrimaryStats()
        {
            return _data.Where(s => s.IsPrimary).ToList();
        }
        
        /// <summary>
        /// 초기값 설정용 Dictionary 반환
        /// </summary>
        public Dictionary<string, int> GetInitialValues()
        {
            var dict = new Dictionary<string, int>();
            foreach (var stat in _data)
                dict[stat.StatId] = stat.InitialValue;
            return dict;
        }
    }
}
