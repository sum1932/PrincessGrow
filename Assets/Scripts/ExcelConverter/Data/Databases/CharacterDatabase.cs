using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "GameData/Databases/Character Database", order = 1)]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private List<CharacterData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, CharacterData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, CharacterData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.CharacterId))
                    _cache[item.CharacterId] = item;
            }
            _initialized = true;
        }
        
        public CharacterData Get(string characterId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(characterId);
        }
        
        public List<CharacterData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<CharacterData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        public List<CharacterData> GetByRole(string role)
        {
            return _data.Where(c => c.Role == role).ToList();
        }
        
        public List<CharacterData> GetRomanceable()
        {
            return _data.Where(c => c.IsRomanceable).ToList();
        }
    }
}
