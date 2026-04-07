using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using GameData.ScriptableObjects;
using UnityEngine;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// Database ScriptableObject 기반 엔딩 저장소
    /// </summary>
    public class DatabaseEndingRepository : IEndingRepository
    {
        private EndingDatabase _database;
        private List<EndingCondition> _endings;
        
        public DatabaseEndingRepository(EndingDatabase database)
        {
            _database = database;
            _endings = new List<EndingCondition>();
            LoadFromDatabase();
        }
        
        private void LoadFromDatabase()
        {
            if (_database == null)
            {
                Debug.LogWarning("EndingDatabase가 연결되지 않았습니다.");
                return;
            }
            
            _database.Initialize();
            _endings.Clear();
            
            foreach (var endingData in _database.GetAll())
            {
                var ending = ConvertToEndingCondition(endingData);
                if (ending != null)
                    _endings.Add(ending);
            }
            
            Debug.Log($"엔딩 {_endings.Count}개 로드 완료 (Database)");
        }
        
        private EndingCondition ConvertToEndingCondition(EndingData data)
        {
            if (data == null) return null;
            
            try
            {
                // 필수 스탯 파싱
                var requiredStats = new Dictionary<StatType, int>();
                ParseRequiredStat(requiredStats, StatType.HP, data.ReqHP);
                ParseRequiredStat(requiredStats, StatType.Charm, data.ReqCharm);
                ParseRequiredStat(requiredStats, StatType.Intelligence, data.ReqInt);
                ParseRequiredStat(requiredStats, StatType.Art, data.ReqArt);
                ParseRequiredStat(requiredStats, StatType.Morality, data.ReqMorality);
                
                // 필수 호감도 파싱
                var requiredFavor = new Dictionary<string, int>();
                ParseFavorRequirement(requiredFavor, "ino", data.ReqFavorIno);
                ParseFavorRequirement(requiredFavor, "aileen", data.ReqFavorAileen);
                ParseFavorRequirement(requiredFavor, "kyle", data.ReqFavorKyle);
                ParseFavorRequirement(requiredFavor, "lian", data.ReqFavorLian);
                
                // 필수 이벤트 파싱
                var requiredEvents = new List<string>();
                if (!string.IsNullOrEmpty(data.ReqEvents))
                {
                    requiredEvents = data.ReqEvents.Split(',')
                        .Select(e => e.Trim())
                        .Where(e => !string.IsNullOrEmpty(e))
                        .ToList();
                }
                
                // 필수 선택지 파싱
                var requiredChoices = new List<string>();
                if (!string.IsNullOrEmpty(data.ReqChoices))
                {
                    requiredChoices = data.ReqChoices.Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();
                }
                
                // IsHidden 파싱
                bool isHidden = !string.IsNullOrEmpty(data.Type) && 
                               data.Type.ToLower() == "hidden";
                
                return new EndingCondition(
                    data.EndingId,
                    data.NameKO,
                    data.Priority,
                    requiredStats,
                    requiredFavor,
                    requiredEvents,
                    requiredChoices,
                    isHidden,
                    data.Description
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"엔딩 변환 실패: {e.Message}");
                return null;
            }
        }
        
        private void ParseRequiredStat(Dictionary<StatType, int> stats, StatType statType, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            // "800+", "600+" 등 파싱
            value = value.Replace("+", "").Replace(">=", "").Replace(">", "").Trim();
            if (int.TryParse(value, out int req) && req > 0)
            {
                stats[statType] = req;
            }
        }
        
        private void ParseFavorRequirement(Dictionary<string, int> favor, string npcId, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            // ">=80", "80-100" 등 파싱
            value = value.Replace(">=", "").Replace(">", "").Replace("=", "").Trim();
            
            // "80-100" 형식 처리
            if (value.Contains("-"))
            {
                var parts = value.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int minValue))
                {
                    favor[npcId] = minValue;
                }
            }
            else if (int.TryParse(value, out int req) && req > 0)
            {
                favor[npcId] = req;
            }
        }
        
        public List<EndingCondition> GetAll() => _endings;
        
        public EndingCondition GetById(string id) => 
            _endings.FirstOrDefault(e => e.Id == id);
        
        public List<EndingCondition> GetByType(bool isHidden) => 
            _endings.Where(e => e.IsHidden == isHidden).ToList();
    }
}
