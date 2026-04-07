using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 아이템 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "GameData/Item", order = 5)]
    public class ItemData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Item_ID")]
        public string ItemId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Name_EN")]
        public string NameEN;

        [ExcelColumn("Type")]
        public string Type;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;

        [ExcelColumn("Price")]
        public int Price;

        [ExcelColumn("Effect")]
        public string Effect;

        [ExcelColumn("Icon_Path")]
        public string IconPath;

        [ExcelColumn("Is_Consumable")]
        public bool IsConsumable;
    }
}
