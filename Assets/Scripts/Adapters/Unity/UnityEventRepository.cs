using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DessertKingdom.Core.Data;
using DessertKingdom.Core.Domain;
using EventType = DessertKingdom.Core.Domain.EventType;

namespace DessertKingdom.Adapters.Unity
{
    /// <summary>
    /// Unity JsonUtility를 사용한 이벤트 저장소 구현
    /// </summary>
    public class UnityEventRepository : IEventRepository
    {
        private readonly List<GameEvent> _events;
        private readonly string _filePath;

        public UnityEventRepository(string filePath)
        {
            _filePath = filePath;
            _events = LoadFromJson();
        }

        public UnityEventRepository() : this(Path.Combine(Application.streamingAssetsPath, "Data", "Events.json"))
        {
        }

        private List<GameEvent> LoadFromJson()
        {
            if (!File.Exists(_filePath))
            {
                Debug.LogWarning($"[UnityEventRepository] 파일을 찾을 수 없음: {_filePath}");
                return new List<GameEvent>();
            }

            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                var wrapper = JsonUtility.FromJson<EventsWrapper>(jsonContent);
                
                if (wrapper?.events == null || wrapper.events.Length == 0)
                {
                    Debug.LogWarning("[UnityEventRepository] 이벤트 데이터가 없습니다.");
                    return new List<GameEvent>();
                }

                return wrapper.events.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityEventRepository] 로딩 오류: {ex.Message}");
                return new List<GameEvent>();
            }
        }

        private GameEvent MapToDomain(EventData data)
        {
            if (!Enum.TryParse<EventType>(data.type, true, out var eventType))
            {
                eventType = EventType.Fixed;
            }

            var conditions = data.conditions?.ToDictionary(item => item.key, item => (object)item.value) ?? new Dictionary<string, object>();
            
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

            var choices = new List<EventChoice>();
            if (data.choices != null)
            {
                foreach (var choiceData in data.choices)
                {
                    choices.Add(MapChoiceToDomain(choiceData));
                }
            }

            var favorEffects = data.favorEffects?.ToDictionary(item => item.key, item => item.value) ?? new Dictionary<string, int>();

            return new GameEvent(
                data.id,
                data.name,
                eventType,
                data.priority,
                data.description,
                conditions,
                choices,
                statEffects,
                favorEffects,
                data.isOneTime,
                data.isHidden
            );
        }

        private EventChoice MapChoiceToDomain(EventChoiceData data)
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

            return new EventChoice(
                data.id,
                data.text,
                statEffects,
                data.favorEffects?.ToDictionary(item => item.key, item => item.value) ?? new Dictionary<string, int>(),
                data.requirements?.ToDictionary(item => item.key, item => (object)item.value) ?? new Dictionary<string, object>()
            );
        }

        public GameEvent GetById(string id)
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }

        public List<GameEvent> GetAll()
        {
            return _events.ToList();
        }

        public List<GameEvent> GetByType(EventType type)
        {
            return _events.Where(e => e.Type == type).ToList();
        }
    }

    // Unity JsonUtility 호환 래퍼 클래스
    [Serializable]
    public class EventsWrapper
    {
        public EventData[] events;
    }

    [Serializable]
    public class EventData
    {
        public string id;
        public string name;
        public string type;
        public int priority;
        public string description;
        public EventKeyValueData[] conditions;
        public EventChoiceData[] choices;
        public EventStatEffectData[] statEffects;
        public EventKeyValueIntData[] favorEffects;
        public bool isOneTime;
        public bool isHidden;
    }

    [Serializable]
    public class EventChoiceData
    {
        public string id;
        public string text;
        public EventStatEffectData[] statEffects;
        public EventKeyValueIntData[] favorEffects;
        public EventKeyValueData[] requirements;
    }

    [Serializable]
    public class EventKeyValueData
    {
        public string key;
        public string value;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object> { { key, value } };
        }
    }

    [Serializable]
    public class EventKeyValueIntData
    {
        public string key;
        public int value;

        public Dictionary<string, int> ToDictionary()
        {
            return new Dictionary<string, int> { { key, value } };
        }
    }

    [Serializable]
    public class EventStatEffectData
    {
        public string key;
        public int value;
    }
}
