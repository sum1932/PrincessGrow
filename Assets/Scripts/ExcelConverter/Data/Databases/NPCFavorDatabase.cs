using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// NPC 호감도 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "NPCFavorDatabase", menuName = "GameData/Databases/NPC Favor Database", order = 11)]
    public class NPCFavorDatabase : ScriptableObject
    {
        [SerializeField] private List<NPCFavorData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, NPCFavorData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, NPCFavorData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.NPCId))
                    _cache[item.NPCId] = item;
            }
            _initialized = true;
        }
        
        public NPCFavorData Get(string npcId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(npcId);
        }
        
        public List<NPCFavorData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<NPCFavorData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 특정 호감도에 해당하는 레벨 계산
        /// </summary>
        public int GetFavorLevel(string npcId, int currentFavor)
        {
            var favorData = Get(npcId);
            if (favorData == null) return 0;
            
            if (currentFavor >= favorData.Threshold4) return 4;
            if (currentFavor >= favorData.Threshold3) return 3;
            if (currentFavor >= favorData.Threshold2) return 2;
            if (currentFavor >= favorData.Threshold1) return 1;
            return 0;
        }
        
        /// <summary>
        /// 다음 레벨까지 필요한 호감도 계산
        /// </summary>
        public int GetRequiredFavorForNextLevel(string npcId, int currentFavor)
        {
            var favorData = Get(npcId);
            if (favorData == null) return 0;
            
            int level = GetFavorLevel(npcId, currentFavor);
            return level switch
            {
                0 => favorData.Threshold1 - currentFavor,
                1 => favorData.Threshold2 - currentFavor,
                2 => favorData.Threshold3 - currentFavor,
                3 => favorData.Threshold4 - currentFavor,
                _ => 0
            };
        }
    }
}
