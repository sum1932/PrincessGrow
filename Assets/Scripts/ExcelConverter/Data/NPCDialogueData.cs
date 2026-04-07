using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// NPC 호감도 대사 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NPCDialogueData", menuName = "GameData/NPC Dialogue", order = 10)]
    public class NPCDialogueData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Dialogue_ID")]
        public string DialogueId;

        [ExcelColumn("NPC_ID")]
        public string NPCId;

        [ExcelColumn("Favor_Level")]
        public string FavorLevel;

        [ExcelColumn("Dialogue_Text_KO")]
        [TextArea(3, 10)]
        public string DialogueTextKO;

        [ExcelColumn("Dialogue_Text_EN")]
        [TextArea(3, 10)]
        public string DialogueTextEN;

        [ExcelColumn("CG_Variation")]
        public string CGVariation;

        [ExcelColumn("BGM_ID")]
        public string BGMId;
    }
}
