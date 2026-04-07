using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 장비 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "GameData/Equipment", order = 6)]
    public class EquipmentData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Equipment_ID")]
        public string EquipmentId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Name_EN")]
        public string NameEN;

        [ExcelColumn("Slot")]
        public string Slot;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;

        [ExcelColumn("HP_Bonus")]
        public int HPBonus;

        [ExcelColumn("CHARM_Bonus")]
        public int CharmBonus;

        [ExcelColumn("INT_Bonus")]
        public int IntBonus;

        [ExcelColumn("ART_Bonus")]
        public int ArtBonus;

        [ExcelColumn("Icon_Path")]
        public string IconPath;
    }
}
