using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Data
{
    public class JsonEventRepository : IEventRepository
    {
        private readonly List<GameEvent> _events;
        private readonly string _filePath;

        public JsonEventRepository(string filePath)
        {
            _filePath = filePath;
            _events = LoadFromJson();
        }

        public JsonEventRepository() : this(Path.Combine("Data", "Events.json"))
        {
        }

        private List<GameEvent> LoadFromJson()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Warning: Events file not found at {_filePath}");
                return new List<GameEvent>();
            }

            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dto = JsonSerializer.Deserialize<EventsDto>(jsonContent, options);
                
                if (dto?.Events == null)
                {
                    Console.WriteLine("Warning: No events found in JSON file");
                    return new List<GameEvent>();
                }

                return dto.Events.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading events: {ex.Message}");
                return new List<GameEvent>();
            }
        }

        private GameEvent MapToDomain(EventDto dto)
        {
            if (!Enum.TryParse<EventType>(dto.Type, true, out var eventType))
            {
                eventType = EventType.Fixed;
            }

            var conditions = dto.Conditions ?? new Dictionary<string, object>();
            
            var statEffects = new Dictionary<StatType, int>();
            if (dto.StatEffects != null)
            {
                foreach (var effect in dto.StatEffects)
                {
                    if (Enum.TryParse<StatType>(effect.Key, true, out var statType))
                    {
                        statEffects[statType] = effect.Value;
                    }
                }
            }

            var choices = new List<EventChoice>();
            if (dto.Choices != null)
            {
                foreach (var choiceDto in dto.Choices)
                {
                    choices.Add(MapChoiceToDomain(choiceDto));
                }
            }

            var favorEffects = dto.FavorEffects ?? new Dictionary<string, int>();

            return new GameEvent(
                dto.Id,
                dto.Name,
                eventType,
                dto.Priority,
                dto.Description,
                conditions,
                choices,
                statEffects,
                favorEffects,
                dto.IsOneTime,
                dto.IsHidden
            );
        }

        private EventChoice MapChoiceToDomain(EventChoiceDto dto)
        {
            var statEffects = new Dictionary<StatType, int>();
            if (dto.StatEffects != null)
            {
                foreach (var effect in dto.StatEffects)
                {
                    if (Enum.TryParse<StatType>(effect.Key, true, out var statType))
                    {
                        statEffects[statType] = effect.Value;
                    }
                }
            }

            return new EventChoice(
                dto.Id,
                dto.Text,
                statEffects,
                dto.FavorEffects ?? new Dictionary<string, int>(),
                dto.Requirements ?? new Dictionary<string, object>()
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

    // DTO classes for JSON deserialization
    public class EventsDto
    {
        [JsonPropertyName("events")]
        public List<EventDto> Events { get; set; }
    }

    public class EventDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("conditions")]
        public Dictionary<string, object> Conditions { get; set; }

        [JsonPropertyName("choices")]
        public List<EventChoiceDto> Choices { get; set; }

        [JsonPropertyName("statEffects")]
        public Dictionary<string, int> StatEffects { get; set; }

        [JsonPropertyName("favorEffects")]
        public Dictionary<string, int> FavorEffects { get; set; }

        [JsonPropertyName("isOneTime")]
        public bool IsOneTime { get; set; }

        [JsonPropertyName("isHidden")]
        public bool IsHidden { get; set; }
    }

    public class EventChoiceDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("statEffects")]
        public Dictionary<string, int> StatEffects { get; set; }

        [JsonPropertyName("favorEffects")]
        public Dictionary<string, int> FavorEffects { get; set; }

        [JsonPropertyName("requirements")]
        public Dictionary<string, object> Requirements { get; set; }
    }
}
