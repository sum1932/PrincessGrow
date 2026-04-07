using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 엔딩 데이터베이스 - 모든 엔딩 조건 관리
    /// </summary>
    [CreateAssetMenu(fileName = "EndingDatabase", menuName = "GameData/Databases/Ending Database", order = 9)]
    public class EndingDatabase : ScriptableObject
    {
        [SerializeField] private List<EndingData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, EndingData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, EndingData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.EndingId))
                    _cache[item.EndingId] = item;
            }
            _initialized = true;
        }
        
        public EndingData Get(string endingId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(endingId);
        }
        
        public List<EndingData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<EndingData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
        
        /// <summary>
        /// 타입별 엔딩 검색
        /// </summary>
        public List<EndingData> GetByType(string endingType)
        {
            return _data.Where(e => e.Type == endingType).ToList();
        }
        
        /// <summary>
        /// 현재 스탯으로 달성 가능한 엔딩 검색
        /// </summary>
        public List<EndingData> GetAchievableEndings(int hp, int charm, int intel, int art, int morality, int stress)
        {
            return _data.Where(e => 
                ParseStatValue(e.ReqHP) > 0 ? hp >= ParseStatValue(e.ReqHP) : true &&
                ParseStatValue(e.ReqCharm) > 0 ? charm >= ParseStatValue(e.ReqCharm) : true &&
                ParseStatValue(e.ReqInt) > 0 ? intel >= ParseStatValue(e.ReqInt) : true &&
                ParseStatValue(e.ReqArt) > 0 ? art >= ParseStatValue(e.ReqArt) : true &&
                ParseStatValue(e.ReqMorality) > 0 ? morality >= ParseStatValue(e.ReqMorality) : true &&
                ParseMaxStress(e.ReqStress) > 0 ? stress <= ParseMaxStress(e.ReqStress) : true
            ).ToList();
        }
        
        private int ParseStatValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            value = value.Replace("+", "").Replace(">=", "").Replace(">", "").Trim();
            if (int.TryParse(value, out int result))
                return result;
            return 0;
        }
        
        private int ParseMaxStress(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            value = value.Replace("<=", "").Replace("<", "").Trim();
            if (int.TryParse(value, out int result))
                return result;
            return 0;
        }
    }
}
