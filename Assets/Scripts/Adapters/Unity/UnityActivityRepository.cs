using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DessertKingdom.Core.Data;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Adapters.Unity
{
    /// <summary>
    /// Unity JsonUtility를 사용한 활동 저장소 구현
    /// </summary>
    public class UnityActivityRepository : IActivityRepository
    {
        private readonly List<Activity> _activities;
        private readonly string _filePath;

        public UnityActivityRepository(string filePath)
        {
            _filePath = filePath;
            _activities = LoadFromJson();
        }

        public UnityActivityRepository() : this(Path.Combine(Application.streamingAssetsPath, "Data", "Activities.json"))
        {
        }

        private List<Activity> LoadFromJson()
        {
            if (!File.Exists(_filePath))
            {
                Debug.LogWarning($"[UnityActivityRepository] 파일을 찾을 수 없음: {_filePath}");
                return new List<Activity>();
            }

            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                var wrapper = JsonUtility.FromJson<ActivitiesWrapper>(jsonContent);
                
                if (wrapper?.activities == null || wrapper.activities.Length == 0)
                {
                    Debug.LogWarning("[UnityActivityRepository] 활동 데이터가 없습니다.");
                    return new List<Activity>();
                }

                return wrapper.activities.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityActivityRepository] 로딩 오류: {ex.Message}");
                return new List<Activity>();
            }
        }

        private Activity MapToDomain(ActivityData data)
        {
            var statEffects = new Dictionary<StatType, int>();
            
            if (data.statEffects != null)
            {
                foreach (var effect in data.statEffects)
                {
                    if (Enum.TryParse<StatType>(effect.key, true, out var statType))
                    {
                        statEffects[statType] = effect.value;
                    }
                }
            }

            if (!Enum.TryParse<ActivityType>(data.type, true, out var activityType))
            {
                activityType = ActivityType.Lesson;
            }

            return new Activity(
                data.id,
                data.name,
                activityType,
                data.cost,
                data.income,
                data.minAge,
                statEffects,
                data.stressChange,
                data.description
            );
        }

        public Activity GetById(string id)
        {
            return _activities.FirstOrDefault(a => a.Id == id);
        }

        public List<Activity> GetAll()
        {
            return _activities.ToList();
        }

        public List<Activity> GetByType(ActivityType type)
        {
            return _activities.Where(a => a.Type == type).ToList();
        }

        public List<Activity> GetAvailable(int age)
        {
            return _activities.Where(a => a.MinAge <= age).ToList();
        }
    }

    // Unity JsonUtility 호환 래퍼 클래스
    [Serializable]
    public class ActivitiesWrapper
    {
        public ActivityData[] activities;
    }

    [Serializable]
    public class ActivityData
    {
        public string id;
        public string name;
        public string type;
        public int cost;
        public int income;
        public int minAge;
        public ActivityStatEffectData[] statEffects;
        public int stressChange;
        public string description;
    }

    [Serializable]
    public class ActivityStatEffectData
    {
        public string key;
        public int value;
    }
}
