using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Data
{
    public class JsonActivityRepository : IActivityRepository
    {
        private readonly List<Activity> _activities;
        private readonly string _filePath;

        public JsonActivityRepository(string filePath)
        {
            _filePath = filePath;
            _activities = LoadFromJson();
        }

        public JsonActivityRepository() : this(Path.Combine("Data", "Activities.json"))
        {
        }

        private List<Activity> LoadFromJson()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Warning: Activity file not found at {_filePath}");
                return new List<Activity>();
            }

            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dto = JsonSerializer.Deserialize<ActivitiesDto>(jsonContent, options);
                
                if (dto?.Activities == null)
                {
                    Console.WriteLine("Warning: No activities found in JSON file");
                    return new List<Activity>();
                }

                return dto.Activities.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading activities: {ex.Message}");
                return new List<Activity>();
            }
        }

        private Activity MapToDomain(ActivityDto dto)
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

            if (!Enum.TryParse<ActivityType>(dto.Type, true, out var activityType))
            {
                activityType = ActivityType.Lesson;
            }

            return new Activity(
                dto.Id,
                dto.Name,
                activityType,
                dto.Cost,
                dto.Income,
                dto.MinAge,
                statEffects,
                dto.StressChange,
                dto.Description
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

        public List<Activity> GetAvailable(int age)
        {
            return _activities.Where(a => a.MinAge <= age).ToList();
        }

        public List<Activity> GetByType(ActivityType type)
        {
            return _activities.Where(a => a.Type == type).ToList();
        }
    }

    // DTO classes for JSON deserialization
    public class ActivitiesDto
    {
        [JsonPropertyName("activities")]
        public List<ActivityDto> Activities { get; set; }
    }

    public class ActivityDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("income")]
        public int Income { get; set; }

        [JsonPropertyName("minAge")]
        public int MinAge { get; set; }

        [JsonPropertyName("statEffects")]
        public Dictionary<string, int> StatEffects { get; set; }

        [JsonPropertyName("stressChange")]
        public int StressChange { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
