using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using UnityEngine;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// CSV 기반 엔딩 저장소
    /// </summary>
    public class CsvEndingRepository : IEndingRepository
    {
        private readonly List<EndingCondition> _endings = new List<EndingCondition>();
        private readonly string _csvPath = "Data/Ending_Conditions";
        
        public CsvEndingRepository()
        {
            LoadFromCsv();
        }
        
        public CsvEndingRepository(string csvPath)
        {
            _csvPath = csvPath;
            LoadFromCsv();
        }
        
        private void LoadFromCsv()
        {
            var csvAsset = Resources.Load<TextAsset>(_csvPath);
            if (csvAsset == null)
            {
                Debug.LogWarning($"CSV 파일을 찾을 수 없습니다: {_csvPath}");
                return;
            }
            
            var rows = CsvParser.Parse(csvAsset.text);
            foreach (var row in rows)
            {
                var ending = ParseEnding(row);
                if (ending != null)
                    _endings.Add(ending);
            }
            
            Debug.Log($"엔딩 {(_endings.Count)}개 로드 완료");
        }
        
        private EndingCondition ParseEnding(Dictionary<string, string> row)
        {
            try
            {
                string id = CsvParser.GetString(row, "Ending_ID");
                if (string.IsNullOrEmpty(id)) id = CsvParser.GetString(row, "Id");
                
                string name = CsvParser.GetString(row, "Name_KO");
                if (string.IsNullOrEmpty(name)) name = CsvParser.GetString(row, "Name");
                
                int priority = CsvParser.GetInt(row, "Priority", 10);
                string description = CsvParser.GetString(row, "Description");
                bool isHidden = CsvParser.GetBool(row, "Type", false); // "Hidden"이면 true
                
                // 필수 스탯 파싱
                var requiredStats = new Dictionary<StatType, int>();
                ParseRequiredStats(row, requiredStats);
                
                // 필수 호감도 파싱
                var requiredFavor = new Dictionary<string, int>();
                ParseRequiredFavor(row, requiredFavor);
                
                // 필수 이벤트/선택지
                var requiredEvents = CsvParser.GetList(row, "Req_Events");
                var requiredChoices = CsvParser.GetList(row, "Req_Choices");
                
                return new EndingCondition(id, name, priority, requiredStats, 
                    requiredFavor, requiredEvents, requiredChoices, isHidden, description);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"엔딩 파싱 실패: {e.Message}");
                return null;
            }
        }
        
        private void ParseRequiredStats(Dictionary<string, string> row, Dictionary<StatType, int> stats)
        {
            // "800+" 같은 형식 파싱
            ParseStatRequirement(row, "Req_HP", StatType.HP, stats);
            ParseStatRequirement(row, "Req_Charm", StatType.Charm, stats);
            ParseStatRequirement(row, "Req_INT", StatType.Intelligence, stats);
            ParseStatRequirement(row, "Req_Art", StatType.Art, stats);
            ParseStatRequirement(row, "Req_Morality", StatType.Morality, stats);
        }
        
        private void ParseStatRequirement(Dictionary<string, string> row, string column, 
            StatType statType, Dictionary<StatType, int> stats)
        {
            string value = CsvParser.GetString(row, column);
            if (string.IsNullOrEmpty(value)) return;
            
            // "800+" 또는 "800" 형식
            value = value.Replace("+", "").Replace("-", "").Trim();
            if (int.TryParse(value, out int req))
            {
                stats[statType] = req;
            }
        }
        
        private void ParseRequiredFavor(Dictionary<string, string> row, Dictionary<string, int> favor)
        {
            ParseFavorRequirement(row, "Req_Favor_Ino", "ino", favor);
            ParseFavorRequirement(row, "Req_Favor_Aileen", "aileen", favor);
            ParseFavorRequirement(row, "Req_Favor_Kyle", "kyle", favor);
            ParseFavorRequirement(row, "Req_Favor_Lian", "lian", favor);
        }
        
        private void ParseFavorRequirement(Dictionary<string, string> row, string column, 
            string npcId, Dictionary<string, int> favor)
        {
            string value = CsvParser.GetString(row, column);
            if (string.IsNullOrEmpty(value)) return;
            
            // ">=80" 또는 "80" 형식
            value = value.Replace("<=", "").Replace("<", "").Replace("=", "").Replace(">=", "").Replace(">", "").Trim();
            if (int.TryParse(value, out int req))
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
