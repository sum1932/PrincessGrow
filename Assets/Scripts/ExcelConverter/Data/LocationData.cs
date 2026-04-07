using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 위치/장소 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "LocationData", menuName = "GameData/Location", order = 7)]
    public class LocationData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Location_ID")]
        public string LocationId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Name_EN")]
        public string NameEN;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;

        [ExcelColumn("Available_Actions")]
        public string AvailableActions;

        [ExcelColumn("Unlock_Condition")]
        public string UnlockCondition;

        [ExcelColumn("Background_Image")]
        public string BackgroundImage;
    }
}
