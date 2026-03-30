using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using UnityEngine;
using EventType = DessertKingdom.Core.Domain.EventType;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// CSV 기반 이벤트 저장소
    /// </summary>
    public class CsvEventRepository : IEventRepository
    {
        private readonly List<GameEvent> _events = new List<GameEvent>();
        private readonly string _csvPath = "Data/Events";
        
        public CsvEventRepository()
        {
            LoadFromCsv();
        }
        
        public CsvEventRepository(string csvPath)
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
                var evt = ParseEvent(row);
                if (evt != null)
                    _events.Add(evt);
            }
            
            Debug.Log($"이벤트 {(_events.Count)}개 로드 완료");
        }
        
        private GameEvent ParseEvent(Dictionary<string, string> row)
        {
            try
            {
                string id = CsvParser.GetString(row, "Event_ID");
                if (string.IsNullOrEmpty(id)) id = CsvParser.GetString(row, "Id");
                
                string name = CsvParser.GetString(row, "Name_KO");
                if (string.IsNullOrEmpty(name)) name = CsvParser.GetString(row, "Name");
                
                var type = CsvParser.GetEnum<EventType>(row, "Type", EventType.Random);
                int priority = CsvParser.GetInt(row, "Priority", 1);
                string description = CsvParser.GetString(row, "Description");
                bool isOneTime = CsvParser.GetBool(row, "Is_OneTime", true);
                bool isHidden = CsvParser.GetBool(row, "Is_Hidden", false);
                
                // 조건 파싱
                var conditions = new Dictionary<string, object>();
                
                int minAge = CsvParser.GetInt(row, "Min_Age", 6);
                if (minAge > 6) conditions["minAge"] = minAge;
                
                int maxAge = CsvParser.GetInt(row, "Max_Age", 18);
                if (maxAge < 18) conditions["maxAge"] = maxAge;
                
                int month = CsvParser.GetInt(row, "Month", 0);
                if (month > 0) conditions["month"] = month;
                
                // 스탯 조건 파싱
                ParseStatConditions(row, conditions);
                
                // 호감도 조건 파싱
                ParseFavorConditions(row, conditions);
                
                // 선택지는 추후 로드 (별도 파일 또는 기본값)
                var choices = new List<EventChoice>();
                
                // 스탯 효과
                var statEffects = new Dictionary<StatType, int>();
                var favorEffects = new Dictionary<string, int>();
                
                return new GameEvent(id, name, type, priority, description, 
                    conditions, choices, statEffects, favorEffects, isOneTime, isHidden);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"이벤트 파싱 실패: {e.Message}");
                return null;
            }
        }
        
        private void ParseStatConditions(Dictionary<string, string> row, Dictionary<string, object> conditions)
        {
            // Req_HP, Req_Charm 등의 컬럼 파싱
            int hp = CsvParser.GetInt(row, "Req_HP", 0);
            if (hp > 0) conditions["stat_HP"] = hp;
            
            int charm = CsvParser.GetInt(row, "Req_Charm", 0);
            if (charm > 0) conditions["stat_Charm"] = charm;
            
            int intel = CsvParser.GetInt(row, "Req_INT", 0);
            if (intel > 0) conditions["stat_Intelligence"] = intel;
            
            int art = CsvParser.GetInt(row, "Req_Art", 0);
            if (art > 0) conditions["stat_Art"] = art;
            
            int morality = CsvParser.GetInt(row, "Req_Morality", 0);
            if (morality > 0) conditions["stat_Morality"] = morality;
        }
        
        private void ParseFavorConditions(Dictionary<string, string> row, Dictionary<string, object> conditions)
        {
            int ino = CsvParser.GetInt(row, "Req_Favor_Ino", 0);
            if (ino > 0) conditions["favor_ino"] = ino;
            
            int aileen = CsvParser.GetInt(row, "Req_Favor_Aileen", 0);
            if (aileen > 0) conditions["favor_aileen"] = aileen;
            
            int kyle = CsvParser.GetInt(row, "Req_Favor_Kyle", 0);
            if (kyle > 0) conditions["favor_kyle"] = kyle;
            
            int lian = CsvParser.GetInt(row, "Req_Favor_Lian", 0);
            if (lian > 0) conditions["favor_lian"] = lian;
        }
        
        public List<GameEvent> GetAll() => _events;
        
        public GameEvent GetById(string id) => 
            _events.FirstOrDefault(e => e.Id == id);
        
        public List<GameEvent> GetByType(EventType type) => 
            _events.Where(e => e.Type == type).ToList();
    }
}
