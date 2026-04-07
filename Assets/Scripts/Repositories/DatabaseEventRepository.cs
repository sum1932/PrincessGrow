using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using GameData.ScriptableObjects;
using UnityEngine;
using EventType = DessertKingdom.Core.Domain.EventType;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// Database ScriptableObject 기반 이벤트 저장소
    /// </summary>
    public class DatabaseEventRepository : IEventRepository
    {
        private EventDatabase _database;
        private List<GameEvent> _events;
        
        public DatabaseEventRepository(EventDatabase database)
        {
            _database = database;
            _events = new List<GameEvent>();
            LoadFromDatabase();
        }
        
        private void LoadFromDatabase()
        {
            if (_database == null)
            {
                Debug.LogWarning("EventDatabase가 연결되지 않았습니다.");
                return;
            }
            
            _database.Initialize();
            _events.Clear();
            
            foreach (var eventData in _database.GetAll())
            {
                var gameEvent = ConvertToGameEvent(eventData);
                if (gameEvent != null)
                    _events.Add(gameEvent);
            }
            
            Debug.Log($"이벤트 {_events.Count}개 로드 완료 (Database)");
        }
        
        private GameEvent ConvertToGameEvent(EventData data)
        {
            if (data == null) return null;
            
            try
            {
                // Type 파싱
                EventType type = ParseEventType(data.Type);
                
                // 조건 파싱
                var conditions = new Dictionary<string, object>();
                
                // 나이 조건
                if (data.AgeMin > 6) conditions["minAge"] = data.AgeMin;
                if (data.AgeMax < 18) conditions["maxAge"] = data.AgeMax;
                
                // 월/일 조건
                if (data.Month > 0) conditions["month"] = data.Month;
                if (data.Day > 0) conditions["day"] = data.Day;
                
                // 스탯 조건 파싱
                ParseStatConditions(data.StatCondition, conditions);
                
                // 호감도 조건 파싱
                ParseFavorConditions(data.NPCCondition, conditions);
                
                // 선택지는 비어있는 리스트로 초기화 (추후 Script_Path에서 로드 가능)
                var choices = new List<EventChoice>();
                
                // 스탯 효과와 호감도 효과는 비어있는 딕셔너리로 초기화
                var statEffects = new Dictionary<StatType, int>();
                var favorEffects = new Dictionary<string, int>();
                
                return new GameEvent(
                    data.EventId,
                    data.NameKO,
                    type,
                    data.Priority,
                    data.Description,
                    conditions,
                    choices,
                    statEffects,
                    favorEffects,
                    !data.IsRepeatable, // IsOneTime = !IsRepeatable
                    data.IsHidden,
                    data.RequiredPreviousEvent
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"이벤트 변환 실패: {e.Message}");
                return null;
            }
        }
        
        private EventType ParseEventType(string typeStr)
        {
            if (string.IsNullOrEmpty(typeStr)) return EventType.Random;
            
            switch (typeStr.ToLower())
            {
                case "fixed": return EventType.Fixed;
                case "random": return EventType.Random;
                case "npc": return EventType.NPC;
                case "hidden": return EventType.Hidden;
                case "prologue": return EventType.Prologue;
                case "special": return EventType.Special;
                default: return EventType.Random;
            }
        }
        
        private void ParseStatConditions(string statCondition, Dictionary<string, object> conditions)
        {
            if (string.IsNullOrEmpty(statCondition)) return;
            
            // 형식: "HP>=100,Charm>=50" 등 파싱
            try
            {
                var pairs = statCondition.Split(',');
                foreach (var pair in pairs)
                {
                    // >=, >, <=, <, = 연산자 처리
                    string[] operators = { ">=", "<=", ">", "<", "=" };
                    foreach (var op in operators)
                    {
                        if (pair.Contains(op))
                        {
                            var parts = pair.Split(op);
                            if (parts.Length == 2)
                            {
                                string statName = parts[0].Trim();
                                string valStr = parts[1].Trim();
                                
                                // 스탯 이름을 Enum 형식으로 변환
                                string statKey = ConvertStatNameToKey(statName);
                                if (!string.IsNullOrEmpty(statKey) && int.TryParse(valStr, out int val))
                                {
                                    conditions[statKey] = val;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"StatCondition 파싱 실패: {e.Message}");
            }
        }
        
        private void ParseFavorConditions(string npcCondition, Dictionary<string, object> conditions)
        {
            if (string.IsNullOrEmpty(npcCondition)) return;
            
            // 형식: "Favor_Aileen>=70" 등 파싱
            try
            {
                var pairs = npcCondition.Split(',');
                foreach (var pair in pairs)
                {
                    if (pair.Contains("Favor_") && pair.Contains("="))
                    {
                        var parts = pair.Split('=');
                        if (parts.Length >= 2)
                        {
                            string key = parts[0].Trim();
                            string valStr = parts[1].Replace("=", "").Replace(">", "").Replace("<", "").Trim();
                            
                            // "Favor_Aileen" -> "favor_aileen"
                            string npcId = key.ToLower().Replace("favor_", "");
                            if (int.TryParse(valStr, out int val))
                            {
                                conditions[$"favor_{npcId}"] = val;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"NPCCondition 파싱 실패: {e.Message}");
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
        
        public List<GameEvent> GetAll() => _events;
        
        public GameEvent GetById(string id) => 
            _events.FirstOrDefault(e => e.Id == id);
        
        public List<GameEvent> GetByType(EventType type) => 
            _events.Where(e => e.Type == type).ToList();
    }
}
