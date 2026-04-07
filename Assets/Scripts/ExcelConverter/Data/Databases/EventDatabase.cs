using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 이벤트 데이터베이스 - 모든 게임 이벤트 관리
    /// </summary>
    [CreateAssetMenu(fileName = "EventDatabase", menuName = "GameData/Databases/Event Database", order = 3)]
    public class EventDatabase : ScriptableObject
    {
        [SerializeField] private List<EventData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, EventData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, EventData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.EventId))
                    _cache[item.EventId] = item;
            }
            _initialized = true;
        }
        
        public EventData Get(string eventId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(eventId);
        }
        
        public List<EventData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<EventData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 특정 나이에 발생하는 이벤트 검색
        /// </summary>
        public List<EventData> GetByAge(int age)
        {
            return _data.Where(e => age >= e.AgeMin && age <= e.AgeMax)
                     .OrderByDescending(e => e.Priority)
                     .ToList();
        }
        
        /// <summary>
        /// 특정 날짜에 발생하는 이벤트 검색
        /// </summary>
        public List<EventData> GetByDate(int month, int day)
        {
            return _data.Where(e => e.Month == month && e.Day == day)
                     .OrderByDescending(e => e.Priority)
                     .ToList();
        }
        
        /// <summary>
        /// 특정 타입의 이벤트 검색
        /// </summary>
        public List<EventData> GetByType(string eventType)
        {
            return _data.Where(e => e.Type == eventType).ToList();
        }
        
        /// <summary>
        /// 트리거 타입별 이벤트 검색
        /// </summary>
        public List<EventData> GetByTrigger(string triggerType)
        {
            return _data.Where(e => e.TriggerType == triggerType).ToList();
        }
    }
}
