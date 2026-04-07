using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 액션/활동 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ActionData", menuName = "GameData/Action", order = 8)]
    public class ActionData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Action_ID")]
        public string ActionId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Category")]
        public string Category;

        [ExcelColumn("Subcategory")]
        public string Subcategory;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;

        [ExcelColumn("Effect_HP")]
        public int EffectHP;

        [ExcelColumn("Effect_Charm")]
        public int EffectCharm;

        [ExcelColumn("Effect_Int")]
        public int EffectInt;

        [ExcelColumn("Effect_Art")]
        public int EffectArt;

        [ExcelColumn("Effect_Morality")]
        public int EffectMorality;

        [ExcelColumn("Effect_Stress")]
        public int EffectStress;

        [ExcelColumn("Cost_Sweets")]
        public int CostSweets;

        [ExcelColumn("Income_Sweets")]
        public int IncomeSweets;

        [ExcelColumn("Required_Stat")]
        public string RequiredStat;

        [ExcelColumn("Required_Value")]
        public int RequiredValue;

        [ExcelColumn("Required_Age")]
        public int RequiredAge;

        [ExcelColumn("Season")]
        public string Season;

        [ExcelColumn("Month")]
        public int Month;

        [ExcelColumn("Is_Repeatable")]
        public bool IsRepeatable;
    }
}
