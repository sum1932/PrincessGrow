using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// NPC 호감도 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NPCFavorData", menuName = "GameData/NPC Favor", order = 11)]
    public class NPCFavorData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("NPC_ID")]
        public string NPCId;

        [ExcelColumn("Favor_Level_0")]
        public string FavorLevel0;

        [ExcelColumn("Favor_Level_1")]
        public string FavorLevel1;

        [ExcelColumn("Favor_Level_2")]
        public string FavorLevel2;

        [ExcelColumn("Favor_Level_3")]
        public string FavorLevel3;

        [ExcelColumn("Favor_Level_4")]
        public string FavorLevel4;

        [ExcelColumn("Threshold_1")]
        public int Threshold1;

        [ExcelColumn("Threshold_2")]
        public int Threshold2;

        [ExcelColumn("Threshold_3")]
        public int Threshold3;

        [ExcelColumn("Threshold_4")]
        public int Threshold4;
    }
}
