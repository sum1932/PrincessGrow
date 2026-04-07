using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 캐릭터 기본 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "GameData/Character", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Character_ID")]
        public string CharacterId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Name_EN")]
        public string NameEN;

        [ExcelColumn("Essence")]
        public string Essence;

        [ExcelColumn("Age")]
        public string Age;

        [ExcelColumn("Gender")]
        public string Gender;

        [ExcelColumn("Role")]
        public string Role;

        [ExcelColumn("Personality_Traits")]
        public string PersonalityTraits;

        [ExcelColumn("Description")]
        [TextArea(3, 10)]
        public string Description;

        [ExcelColumn("Default_Location")]
        public string DefaultLocation;

        [ExcelColumn("Favor_Min")]
        public int FavorMin;

        [ExcelColumn("Favor_Max")]
        public int FavorMax;

        [ExcelColumn("CG_Variations")]
        public string CGVariations;

        [ExcelColumn("Is_Romanceable")]
        public bool IsRomanceable;

        [ExcelColumn("Unlock_Condition")]
        public string UnlockCondition;

        public string DisplayName => string.IsNullOrEmpty(NameKO) ? NameEN : NameKO;
    }
}
