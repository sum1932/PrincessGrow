using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 퀘스트 데이터베이스 - 모든 퀘스트 관리
    /// </summary>
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "GameData/Databases/Quest Database", order = 4)]
    public class QuestDatabase : ScriptableObject
    {
        [SerializeField] private List<QuestData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, QuestData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, QuestData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.QuestId))
                    _cache[item.QuestId] = item;
            }
            _initialized = true;
        }
        
        public QuestData Get(string questId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(questId);
        }
        
        public List<QuestData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<QuestData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 특정 NPC 관련 퀘스트 검색
        /// </summary>
        public List<QuestData> GetByNPC(string npcId)
        {
            return _data.Where(q => q.RequiredNPC == npcId).ToList();
        }
        
        /// <summary>
        /// 특정 나이에 가능한 퀘스트 검색
        /// </summary>
        public List<QuestData> GetByAge(int age)
        {
            return _data.Where(q => age >= q.RequiredAge).ToList();
        }
    }
}
