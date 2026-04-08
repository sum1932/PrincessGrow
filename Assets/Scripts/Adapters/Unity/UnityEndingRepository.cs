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
    /// Unity JsonUtility를 사용한 엔딩 저장소 구현
    /// </summary>
    public class UnityEndingRepository : IEndingRepository
    {
        private readonly List<EndingCondition> _endings;
        private readonly string _filePath;

        public UnityEndingRepository(string filePath)
        {
            _filePath = filePath;
            _endings = LoadFromJson();
        }

        public UnityEndingRepository() : this(Path.Combine(Application.streamingAssetsPath, "Data", "Endings.json"))
        {
        }

        private List<EndingCondition> LoadFromJson()
        {
            if (!File.Exists(_filePath))
            {
                Debug.LogWarning($"[UnityEndingRepository] 파일을 찾을 수 없음: {_filePath}");
                return new List<EndingCondition>();
            }

            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                var wrapper = JsonUtility.FromJson<EndingsWrapper>(jsonContent);
                
                if (wrapper?.endings == null || wrapper.endings.Length == 0)
                {
                    Debug.LogWarning("[UnityEndingRepository] 엔딩 데이터가 없습니다.");
                    return new List<EndingCondition>();
                }

                return wrapper.endings.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityEndingRepository] 로딩 오류: {ex.Message}");
                return new List<EndingCondition>();
            }
        }

        private EndingCondition MapToDomain(EndingData data)
        {
            var requiredStats = new Dictionary<StatType, int>();
            if (data.requiredStats != null)
            {
                foreach (var req in data.requiredStats)
                {
                    if (Enum.TryParse<StatType>(req.key, true, out var statType))
                    {
                        requiredStats[statType] = req.value;
                    }
                }
            }

            var requiredFavor = data.requiredFavor?.ToDictionary(item => item.key, item => item.value) ?? new Dictionary<string, int>();
            var requiredEvents = data.requiredEvents?.items?.ToList() ?? new List<string>();
            var requiredChoices = data.requiredChoices?.items?.ToList() ?? new List<string>();

            return new EndingCondition(
                data.id,
                data.name,
                data.priority,
                requiredStats,
                requiredFavor,
                requiredEvents,
                requiredChoices,
                data.isHidden,
                data.description
            );
        }

        public EndingCondition GetById(string id)
        {
            return _endings.FirstOrDefault(e => e.Id == id);
        }

        public List<EndingCondition> GetAll()
        {
            return _endings.ToList();
        }

        public List<EndingCondition> GetByType(bool isHidden)
        {
            return _endings.Where(e => e.IsHidden == isHidden).ToList();
        }
    }

    // Unity JsonUtility 호환 래퍼 클래스
    [Serializable]
    public class EndingsWrapper
    {
        public EndingData[] endings;
    }

    [Serializable]
    public class EndingData
    {
        public string id;
        public string name;
        public int priority;
        public EndingStatRequirementData[] requiredStats;
        public EndingKeyValueIntData[] requiredFavor;
        public EndingStringArrayData requiredEvents;
        public EndingStringArrayData requiredChoices;
        public bool isHidden;
        public string description;
    }

    [Serializable]
    public class EndingStatRequirementData
    {
        public string key;
        public int value;
    }

    [Serializable]
    public class EndingKeyValueIntData
    {
        public string key;
        public int value;

        public Dictionary<string, int> ToDictionary()
        {
            return new Dictionary<string, int> { { key, value } };
        }
    }

    [Serializable]
    public class EndingStringArrayData
    {
        public string[] items;

        public List<string> ToList()
        {
            return items?.ToList() ?? new List<string>();
        }
    }
}
