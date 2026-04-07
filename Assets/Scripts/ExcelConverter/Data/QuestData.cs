using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 퀘스트 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "QuestData", menuName = "GameData/Quest", order = 4)]
    public class QuestData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Quest_ID")]
        public string QuestId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Steps")]
        public int Steps;

        [ExcelColumn("Total_Turns")]
        public int TotalTurns;

        [ExcelColumn("Trigger_Type")]
        public string TriggerType;

        [ExcelColumn("Trigger_Value")]
        public string TriggerValue;

        [ExcelColumn("Required_NPC")]
        public string RequiredNPC;

        [ExcelColumn("Required_Favor")]
        public int RequiredFavor;

        [ExcelColumn("Required_Age")]
        public int RequiredAge;

        [ExcelColumn("Required_Stat")]
        public string RequiredStat;

        [ExcelColumn("Stat_Value")]
        public string StatValue;

        [ExcelColumn("Reward_Type")]
        public string RewardType;

        [ExcelColumn("Reward_Value")]
        public string RewardValue;

        [ExcelColumn("Special_Reward")]
        public string SpecialReward;

        [ExcelColumn("Related_Ending")]
        public string RelatedEnding;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;
    }
}
