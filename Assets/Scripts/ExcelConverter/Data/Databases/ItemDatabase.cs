using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 아이템 데이터베이스 - 모든 아이템 관리
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "GameData/Databases/Item Database", order = 5)]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, ItemData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, ItemData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.ItemId))
                    _cache[item.ItemId] = item;
            }
            _initialized = true;
        }
        
        public ItemData Get(string itemId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(itemId);
        }
        
        public List<ItemData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<ItemData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 타입별 아이템 검색
        /// </summary>
        public List<ItemData> GetByType(string itemType)
        {
            return _data.Where(i => i.Type == itemType).ToList();
        }
        
        /// <summary>
        /// 소모품만 검색
        /// </summary>
        public List<ItemData> GetConsumables()
        {
            return _data.Where(i => i.IsConsumable).ToList();
        }
    }
}
