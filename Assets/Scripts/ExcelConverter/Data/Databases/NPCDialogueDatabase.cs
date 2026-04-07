using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// NPC 대사 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "NPCDialogueDatabase", menuName = "GameData/Databases/NPC Dialogue Database", order = 10)]
    public class NPCDialogueDatabase : ScriptableObject
    {
        [SerializeField] private List<NPCDialogueData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, NPCDialogueData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, NPCDialogueData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.DialogueId))
                    _cache[item.DialogueId] = item;
            }
            _initialized = true;
        }
        
        public NPCDialogueData Get(string dialogueId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(dialogueId);
        }
        
        public List<NPCDialogueData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<NPCDialogueData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 특정 NPC의 모든 대사 검색
        /// </summary>
        public List<NPCDialogueData> GetByNPC(string npcId)
        {
            return _data.Where(d => d.NPCId == npcId).ToList();
        }
        
        /// <summary>
        /// 특정 호감도 레벨의 대사 검색
        /// </summary>
        public List<NPCDialogueData> GetByFavorLevel(string favorLevel)
        {
            return _data.Where(d => d.FavorLevel == favorLevel).ToList();
        }
    }
}
