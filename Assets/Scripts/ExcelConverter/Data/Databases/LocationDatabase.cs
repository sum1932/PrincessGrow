using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LocationDatabase", menuName = "GameData/Databases/Location Database", order = 7)]
    public class LocationDatabase : ScriptableObject
    {
        [SerializeField] private List<LocationData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, LocationData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, LocationData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.LocationId))
                    _cache[item.LocationId] = item;
            }
            _initialized = true;
        }
        
        public LocationData Get(string locationId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(locationId);
        }
        
        public List<LocationData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<LocationData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
    }
}
