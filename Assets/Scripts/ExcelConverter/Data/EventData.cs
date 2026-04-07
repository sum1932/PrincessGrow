using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 이벤트 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EventData", menuName = "GameData/Event", order = 3)]
    public class EventData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Event_ID")]
        public string EventId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Type")]
        public string Type;

        [ExcelColumn("Trigger_Type")]
        public string TriggerType;

        [ExcelColumn("Trigger_Value")]
        public string TriggerValue;

        [ExcelColumn("Required_Previous_Event")]
        public string RequiredPreviousEvent;

        [ExcelColumn("Age_Min")]
        public int AgeMin;

        [ExcelColumn("Age_Max")]
        public int AgeMax;

        [ExcelColumn("Month")]
        public int Month;

        [ExcelColumn("Day")]
        public int Day;

        [ExcelColumn("Stat_Condition")]
        public string StatCondition;

        [ExcelColumn("NPC_Condition")]
        public string NPCCondition;

        [ExcelColumn("Script_Path")]
        public string ScriptPath;

        [ExcelColumn("CG_ID")]
        public string CGId;

        [ExcelColumn("BGM_ID")]
        public string BGMId;

        [ExcelColumn("Next_Event")]
        public string NextEvent;

        [ExcelColumn("Is_Repeatable")]
        public bool IsRepeatable;

        [ExcelColumn("Is_Hidden")]
        public bool IsHidden;

        [ExcelColumn("Priority")]
        public int Priority;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;
    }
}
