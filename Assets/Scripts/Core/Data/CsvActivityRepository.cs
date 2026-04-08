using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Data
{
    public class CsvActivityRepository : IActivityRepository
    {
        private readonly List<Activity> _activities;
        private readonly string _filePath;

        public CsvActivityRepository(string filePath)
        {
            _filePath = filePath;
            _activities = LoadFromCsv();
        }

        public CsvActivityRepository() : this(Path.Combine("..", "..", "..", "..", "..", "01_GameDesign", "Data", "Actions.csv"))
        {
        }

        private List<Activity> LoadFromCsv()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Warning: Activity CSV file not found at {_filePath}");
                return new List<Activity>();
            }

            try
            {
                var lines = File.ReadAllLines(_filePath);
                var activities = new List<Activity>();
                
                // Skip header and comments, find actual data rows
                bool headerFound = false;
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Skip empty lines and comments
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;
                    
                    // Skip header row
                    if (!headerFound)
                    {
                        if (trimmedLine.Contains("Action_ID"))
                        {
                            headerFound = true;
                        }
                        continue;
                    }
                    
                    // Parse data row
                    var activity = ParseCsvLine(trimmedLine);
                    if (activity != null)
                    {
                        activities.Add(activity);
                    }
                }
                
                return activities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading activities from CSV: {ex.Message}");
                return new List<Activity>();
            }
        }

        private Activity ParseCsvLine(string line)
        {
            try
            {
                // Handle quoted fields with commas
                var fields = new List<string>();
                bool inQuotes = false;
                var currentField = new System.Text.StringBuilder();
                
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    
                    if (c == '"')
                    {
                        inQuotes = !inQuotes;
                    }
                    else if (c == ',' && !inQuotes)
                    {
                        fields.Add(currentField.ToString().Trim());
                        currentField.Clear();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                fields.Add(currentField.ToString().Trim());
                
                if (fields.Count < 17) return null;
                
                // Parse fields
                string actionId = fields[0];
                string name = fields[1];
                string category = fields[2];
                string subcategory = fields[3];
                
                int effectHp = ParseInt(fields[4]);
                int effectCharm = ParseInt(fields[5]);
                int effectInt = ParseInt(fields[6]);
                int effectArt = ParseInt(fields[7]);
                int effectMorality = ParseInt(fields[8]);
                int effectStress = ParseInt(fields[9]);
                int cost = ParseInt(fields[10]);
                int income = ParseInt(fields[11]);
                string requiredStat = fields[12];
                int requiredValue = ParseInt(fields[13]);
                int requiredAge = ParseInt(fields[14]);
                string description = fields[15];
                
                // Build stat effects dictionary
                var statEffects = new Dictionary<StatType, int>();
                if (effectHp != 0) statEffects[StatType.HP] = effectHp;
                if (effectCharm != 0) statEffects[StatType.Charm] = effectCharm;
                if (effectInt != 0) statEffects[StatType.Intelligence] = effectInt;
                if (effectArt != 0) statEffects[StatType.Art] = effectArt;
                if (effectMorality != 0) statEffects[StatType.Morality] = effectMorality;
                
                // Parse activity type
                ActivityType activityType = ActivityType.Lesson;
                switch (category.ToLower())
                {
                    case "job":
                        activityType = ActivityType.PartTime;
                        break;
                    case "rest":
                        activityType = ActivityType.Rest;
                        break;
                    case "outing":
                        activityType = ActivityType.Outing;
                        break;
                    case "special":
                        activityType = ActivityType.Special;
                        break;
                }
                
                return new Activity(
                    actionId,
                    name,
                    activityType,
                    cost,
                    income,
                    requiredAge,
                    statEffects,
                    effectStress,
                    description
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {line}. Error: {ex.Message}");
                return null;
            }
        }
        
        private int ParseInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return 0;
        }

        public Activity GetById(string id)
        {
            return _activities.FirstOrDefault(a => a.Id == id);
        }

        public List<Activity> GetAll()
        {
            return _activities.ToList();
        }

        // 확장 메서드 - 인터페이스에 정의되지 않음
        // public List<Activity> GetAvailable(GameState gameState)
        // {
        //     return _activities.Where(a => a.IsAvailable(gameState.Turn.CurrentAge, gameState.Character, gameState.Turn.CurrentMonth)).ToList();
        // }

        public List<Activity> GetAvailable(int age)
        {
            return _activities.Where(a => a.MinAge <= age).ToList();
        }

        public List<Activity> GetByType(ActivityType type)
        {
            return _activities.Where(a => a.Type == type).ToList();
        }
    }
}
