using System.Collections.Generic;
using UnityEngine;
using ExcelConverter.Attributes;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 대사 데이터베이스 - 이벤트 대사 관리
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueDatabase", menuName = "GameData/Databases/Dialogue Database", order = 5)]
    public class DialogueDatabase : ScriptableObject
    {
        [SerializeField] private List<DialogueData> _data = new();
        
        [System.NonSerialized] private Dictionary<string, DialogueData> _cache;
        [System.NonSerialized] private bool _initialized = false;
        
        public void Initialize()
        {
            if (_initialized) return;
            
            _cache = new Dictionary<string, DialogueData>();
            foreach (var item in _data)
            {
                if (item != null && !string.IsNullOrEmpty(item.EventId))
                {
                    _cache[item.EventId] = item;
                    // JSON 파싱
                    item.ParseJson();
                }
            }
            _initialized = true;
        }
        
        public DialogueData Get(string eventId)
        {
            if (!_initialized) Initialize();
            return _cache.GetValueOrDefault(eventId);
        }
        
        public List<DialogueData> GetAll()
        {
            return _data;
        }
        
        public void SetData(List<DialogueData> data)
        {
            _data = data;
            _cache = null;
            _initialized = false;
        }
    }
    
    /// <summary>
    /// 개별 대사 데이터
    /// </summary>
    [System.Serializable]
    public class DialogueData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Event_ID")]
        public string EventId;
        
        [ExcelColumn("Event_Name")]
        public string EventName;
        
        [ExcelColumn("Background_Image")]
        public string BackgroundImage;
        
        [ExcelColumn("BGM")]
        public string BGM;
        
        // Lines와 Choices는 JSON 문자열로 저장
        [ExcelColumn("Lines_Json")]
        [TextArea(3, 10)]
        public string LinesJson;
        
        [ExcelColumn("Choices_Json")]
        [TextArea(3, 10)]
        public string ChoicesJson;
        
        // 런타임에서 사용할 파싱된 데이터
        [System.NonSerialized]
        public List<DialogueLineData> Lines;
        
        [System.NonSerialized]
        public List<DialogueChoiceData> Choices;
        
        /// <summary>
        /// JSON 문자열을 파싱하여 Lines와 Choices를 채웁니다
        /// </summary>
        public void ParseJson()
        {
            if (!string.IsNullOrEmpty(LinesJson))
            {
                try
                {
                    Lines = JsonUtility.FromJson<DialogueLineList>("{\"Lines\":" + LinesJson + "}").Lines;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[{EventId}] Lines JSON 파싱 실패: {ex.Message}");
                    Lines = new List<DialogueLineData>();
                }
            }
            else
            {
                Lines = new List<DialogueLineData>();
            }
            
            if (!string.IsNullOrEmpty(ChoicesJson))
            {
                try
                {
                    Choices = JsonUtility.FromJson<DialogueChoiceList>("{\"Choices\":" + ChoicesJson + "}").Choices;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[{EventId}] Choices JSON 파싱 실패: {ex.Message}");
                    Choices = new List<DialogueChoiceData>();
                }
            }
            else
            {
                Choices = new List<DialogueChoiceData>();
            }
        }
        
        [System.Serializable]
        private class DialogueLineList
        {
            public List<DialogueLineData> Lines;
        }
        
        [System.Serializable]
        private class DialogueChoiceList
        {
            public List<DialogueChoiceData> Choices;
        }
    }
    
    /// <summary>
    /// 대사 라인 데이터
    /// </summary>
    [System.Serializable]
    public class DialogueLineData
    {
        public string Text;
        public string CharacterId;
        public string CharacterName;
        public string PortraitPath;
        public float DisplayDuration;
    }
    
    /// <summary>
    /// 선택지 데이터
    /// </summary>
    [System.Serializable]
    public class DialogueChoiceData
    {
        public string Text;
        public List<DialogueEffectData> Effects;
    }
    
    /// <summary>
    /// 효과 데이터
    /// </summary>
    [System.Serializable]
    public class DialogueEffectData
    {
        public string Type; // "Stat", "Favor", etc.
        public string Target; // stat name or NPC name
        public int Value;
    }
}
