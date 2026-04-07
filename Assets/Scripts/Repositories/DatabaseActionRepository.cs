using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using GameData.ScriptableObjects;
using UnityEngine;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// Database ScriptableObject 기반 활동 저장소
    /// </summary>
    public class DatabaseActionRepository : IActivityRepository
    {
        private ActionDatabase _database;
        private List<Activity> _activities;
        
        public DatabaseActionRepository(ActionDatabase database)
        {
            _database = database;
            _activities = new List<Activity>();
            LoadFromDatabase();
        }
        
        private void LoadFromDatabase()
        {
            if (_database == null)
            {
                Debug.LogWarning("ActionDatabase가 연결되지 않았습니다.");
                return;
            }
            
            _database.Initialize();
            _activities.Clear();
            
            var allData = _database.GetAll();
            Debug.Log($"[DatabaseActionRepository] Database에서 {allData?.Count ?? 0}개의 ActionData 로드 시도");
            
            if (allData == null || allData.Count == 0)
            {
                Debug.LogError("[DatabaseActionRepository] Database에 데이터가 없습니다! CSV Converter를 다시 실행하세요.");
                return;
            }
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (var actionData in allData)
            {
                if (actionData == null)
                {
                    Debug.LogWarning("[DatabaseActionRepository] null ActionData 발견");
                    failCount++;
                    continue;
                }
                
                var activity = ConvertToActivity(actionData);
                if (activity != null)
                {
                    _activities.Add(activity);
                    successCount++;
                }
                else
                {
                    failCount++;
                }
            }
            
            Debug.Log($"[DatabaseActionRepository] 활동 로드 완료 - 성공: {successCount}, 실패: {failCount}, 총: {_activities.Count}개");
        }
        
        private Activity ConvertToActivity(ActionData data)
        {
            if (data == null) 
            {
                Debug.LogWarning("[ConvertToActivity] ActionData가 null입니다.");
                return null;
            }
            
            // ActionId와 NameKO 확인
            if (string.IsNullOrEmpty(data.ActionId))
            {
                Debug.LogWarning($"[ConvertToActivity] ActionId가 비어있습니다. NameKO: {data.NameKO}");
            }
            
            try
            {
                // Category → ActivityType 매핑
                ActivityType type = ActivityType.Lesson;
                if (!string.IsNullOrEmpty(data.Category))
                {
                    switch (data.Category.ToLower())
                    {
                        case "lesson": type = ActivityType.Lesson; break;
                        case "job": type = ActivityType.PartTime; break;
                        case "rest": type = ActivityType.Rest; break;
                        case "outing": type = ActivityType.Outing; break;
                        case "special": type = ActivityType.Special; break;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ConvertToActivity] Category가 비어있습니다. ActionId: {data.ActionId}");
                }
                
                // 스탯 효과 파싱
                var statEffects = new Dictionary<StatType, int>();
                if (data.EffectHP != 0) statEffects[StatType.HP] = data.EffectHP;
                if (data.EffectCharm != 0) statEffects[StatType.Charm] = data.EffectCharm;
                if (data.EffectInt != 0) statEffects[StatType.Intelligence] = data.EffectInt;
                if (data.EffectArt != 0) statEffects[StatType.Art] = data.EffectArt;
                if (data.EffectMorality != 0) statEffects[StatType.Morality] = data.EffectMorality;
                
                // 요구사항 파싱 (Required_Stat, Required_Value)
                var requirements = new Dictionary<string, object>();
                ParseRequirements(data.RequiredStat, data.RequiredValue, requirements);
                
                var activity = new Activity(
                    data.ActionId ?? $"unknown_{System.Guid.NewGuid().ToString()[..8]}",
                    data.NameKO ?? "이름 없음",
                    type,
                    data.CostSweets,
                    data.IncomeSweets,
                    data.RequiredAge,
                    statEffects,
                    data.EffectStress,
                    data.Description ?? "",
                    requirements,
                    data.Season,
                    data.Month
                );
                
                //Debug.Log($"[ConvertToActivity] 성공: {activity.Name} (ID: {activity.Id})");
                return activity;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ConvertToActivity] 활동 변환 실패 - ActionId: {data.ActionId}, NameKO: {data.NameKO}\n{e.Message}\n{e.StackTrace}");
                return null;
            }
        }
        
        private void ParseRequirements(string requiredStat, int requiredValue, Dictionary<string, object> requirements)
        {
            if (string.IsNullOrEmpty(requiredStat) || requiredStat.ToLower() == "none" || requiredValue <= 0) 
                return;
            
            // 스탯 이름을 requirements 키 형식으로 변환
            string statKey = ConvertStatNameToKey(requiredStat);
            if (!string.IsNullOrEmpty(statKey))
            {
                requirements[statKey] = requiredValue;
            }
        }
        
        private string ConvertStatNameToKey(string statName)
        {
            string normalized = statName.ToLower().Trim();
            
            return normalized switch
            {
                "hp" => "stat_HP",
                "charm" => "stat_Charm",
                "int" => "stat_Intelligence",
                "intelligence" => "stat_Intelligence",
                "art" => "stat_Art",
                "morality" => "stat_Morality",
                "stress" => "stat_Stress",
                _ => $"stat_{statName}"
            };
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
