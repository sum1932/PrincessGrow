using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Data
{
    public class JsonEndingRepository : IEndingRepository
    {
        private readonly List<EndingCondition> _endings;
        private readonly string _filePath;

        public JsonEndingRepository(string filePath)
        {
            _filePath = filePath;
            _endings = LoadFromJson();
        }

        public JsonEndingRepository() : this(Path.Combine("Data", "Endings.json"))
        {
        }

        private List<EndingCondition> LoadFromJson()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Warning: Endings file not found at {_filePath}");
                return new List<EndingCondition>();
            }

            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dto = JsonSerializer.Deserialize<EndingsDto>(jsonContent, options);
                
                if (dto?.Endings == null)
                {
                    Console.WriteLine("Warning: No endings found in JSON file");
                    return new List<EndingCondition>();
                }

                return dto.Endings.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading endings: {ex.Message}");
                return new List<EndingCondition>();
            }
        }

        private EndingCondition MapToDomain(EndingDto dto)
        {
            var requiredStats = new Dictionary<StatType, int>();
            if (dto.RequiredStats != null)
            {
                foreach (var req in dto.RequiredStats)
                {
                    if (Enum.TryParse<StatType>(req.Key, true, out var statType))
                    {
                        requiredStats[statType] = req.Value;
                    }
                }
            }

            return new EndingCondition(
                dto.Id,
                dto.Name,
                dto.Priority,
                requiredStats,
                dto.RequiredFavor ?? new Dictionary<string, int>(),
                dto.RequiredEvents ?? new List<string>(),
                dto.RequiredChoices ?? new List<string>(),
                dto.IsHidden,
                dto.Description
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
    }

    // DTO classes for JSON deserialization
    public class EndingsDto
    {
        [JsonPropertyName("endings")]
        public List<EndingDto> Endings { get; set; }
    }

    public class EndingDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("requiredStats")]
        public Dictionary<string, int> RequiredStats { get; set; }

        [JsonPropertyName("requiredFavor")]
        public Dictionary<string, int> RequiredFavor { get; set; }

        [JsonPropertyName("requiredEvents")]
        public List<string> RequiredEvents { get; set; }

        [JsonPropertyName("requiredChoices")]
        public List<string> RequiredChoices { get; set; }

        [JsonPropertyName("isHidden")]
        public bool IsHidden { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
