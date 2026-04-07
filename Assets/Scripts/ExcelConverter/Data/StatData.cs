using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 캐릭터 스탯 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "StatData", menuName = "GameData/Stat", order = 2)]
    public class StatData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Stat_ID")]
        public string StatId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Name_EN")]
        public string NameEN;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;

        [ExcelColumn("Min_Value")]
        public int MinValue;

        [ExcelColumn("Max_Value")]
        public int MaxValue;

        [ExcelColumn("Initial_Value")]
        public int InitialValue;

        [ExcelColumn("Growth_Rate")]
        public float GrowthRate;

        [ExcelColumn("Is_Primary")]
        public bool IsPrimary;

        [ExcelColumn("Icon_Path")]
        public string IconPath;

        [ExcelColumn("Color_Code")]
        public string ColorCode;
    }
}
