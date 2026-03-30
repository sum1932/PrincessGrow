using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using UnityEngine;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// CSV 기반 활동 저장소
    /// </summary>
    public class CsvActivityRepository : IActivityRepository
    {
        private readonly List<Activity> _activities = new List<Activity>();
        private readonly string _csvPath = "Data/Actions";
        
        public CsvActivityRepository()
        {
            LoadFromCsv();
        }
        
        public CsvActivityRepository(string csvPath)
        {
            _csvPath = csvPath;
            LoadFromCsv();
        }
        
        private void LoadFromCsv()
        {
            var csvAsset = Resources.Load<TextAsset>(_csvPath);
            if (csvAsset == null)
            {
                Debug.LogWarning($"CSV 파일을 찾을 수 없습니다: {_csvPath}");
                return;
            }
            
            var rows = CsvParser.Parse(csvAsset.text);
            foreach (var row in rows)
            {
                var activity = ParseActivity(row);
                if (activity != null)
                    _activities.Add(activity);
            }
            
            Debug.Log($"활동 {(_activities.Count)}개 로드 완료");
        }
        
        private Activity ParseActivity(Dictionary<string, string> row)
        {
            try
            {
                string id = CsvParser.GetString(row, "Action_ID");
                if (string.IsNullOrEmpty(id)) id = CsvParser.GetString(row, "Id");
                
                string name = CsvParser.GetString(row, "Name_KO");
                if (string.IsNullOrEmpty(name)) name = CsvParser.GetString(row, "Name");
                
                var type = CsvParser.GetEnum<ActivityType>(row, "Type", ActivityType.Lesson);
                int cost = CsvParser.GetInt(row, "Cost");
                int income = CsvParser.GetInt(row, "Income");
                int minAge = CsvParser.GetInt(row, "Min_Age", 6);
                int stressChange = CsvParser.GetInt(row, "Stress_Change");
                string description = CsvParser.GetString(row, "Description");
                
                // 스탯 효과 파싱
                var statEffects = new Dictionary<StatType, int>();
                
                // Effect_HP, Effect_Charm 등의 컬럼 파싱
                ParseStatEffect(row, "Effect_HP", StatType.HP, statEffects);
                ParseStatEffect(row, "Effect_Charm", StatType.Charm, statEffects);
                ParseStatEffect(row, "Effect_INT", StatType.Intelligence, statEffects);
                ParseStatEffect(row, "Effect_Art", StatType.Art, statEffects);
                ParseStatEffect(row, "Effect_Morality", StatType.Morality, statEffects);
                ParseStatEffect(row, "Effect_Stress", StatType.Stress, statEffects);
                
                // Effects 파싱 (JSON 형식 또는 쉼표 구분)
                string effectsStr = CsvParser.GetString(row, "Effects");
                if (!string.IsNullOrEmpty(effectsStr))
                {
                    ParseEffectsString(effectsStr, statEffects);
                }
                
                return new Activity(id, name, type, cost, income, minAge, 
                    statEffects, stressChange, description);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"활동 파싱 실패: {e.Message}");
                return null;
            }
        }
        
        private void ParseStatEffect(Dictionary<string, string> row, string column, 
            StatType statType, Dictionary<StatType, int> effects)
        {
            int value = CsvParser.GetInt(row, column, 0);
            if (value != 0)
                effects[statType] = value;
        }
        
        private void ParseEffectsString(string effectsStr, Dictionary<StatType, int> effects)
        {
            // 형식: "HP:10,Charm:-3" 또는 "{\"HP\":10,\"Charm\":-3}"
            var pairs = effectsStr.Split(',');
            foreach (var pair in pairs)
            {
                var kv = pair.Split(':');
                if (kv.Length == 2)
                {
                    string key = kv[0].Trim().Replace("\"", "").Replace("{", "").Replace("}", "");
                    string valStr = kv[1].Trim().Replace("\"", "").Replace("}", "");
                    
                    if (System.Enum.TryParse<StatType>(key, true, out var statType))
                    {
                        if (int.TryParse(valStr, out int val))
                            effects[statType] = val;
                    }
                }
            }
        }
        
        public List<Activity> GetAll() => _activities;
        
        public Activity GetById(string id) => 
            _activities.FirstOrDefault(a => a.Id == id);
        
        public List<Activity> GetByType(ActivityType type) => 
            _activities.Where(a => a.Type == type).ToList();
        
        public List<Activity> GetAvailable(int age) => 
            _activities.Where(a => a.MinAge <= age).ToList();
    }
}
