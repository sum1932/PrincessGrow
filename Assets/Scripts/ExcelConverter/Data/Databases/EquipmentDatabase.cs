using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 장비 데이터베이스 - 모든 장비 관리
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "GameData/Databases/Equipment Database", order = 6)]
    public class EquipmentDatabase : ScriptableObject
    {
        [SerializeField] private List<EquipmentData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, EquipmentData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, EquipmentData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.EquipmentId))
                    _cache[item.EquipmentId] = item;
            }
            _initialized = true;
        }
        
        public EquipmentData Get(string equipmentId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(equipmentId);
        }
        
        public List<EquipmentData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<EquipmentData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 슬롯별 장비 검색
        /// </summary>
        public List<EquipmentData> GetBySlot(string slot)
        {
            return _data.Where(e => e.Slot == slot).ToList();
        }
    }
}
