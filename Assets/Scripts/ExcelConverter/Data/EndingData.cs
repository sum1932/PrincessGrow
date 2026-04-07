using ExcelConverter.Attributes;
using UnityEngine;

namespace GameData.ScriptableObjects
{
    /// <summary>
    /// 엔딩 조건 정보 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EndingData", menuName = "GameData/Ending", order = 9)]
    public class EndingData : ScriptableObject
    {
        [ExcelId]
        [ExcelColumn("Ending_ID")]
        public string EndingId;

        [ExcelColumn("Name_KO")]
        public string NameKO;

        [ExcelColumn("Name_EN")]
        public string NameEN;

        [ExcelColumn("Type")]
        public string Type;

        [ExcelColumn("Priority")]
        public int Priority;

        [ExcelColumn("Req_HP")]
        public string ReqHP;  // "600+" 형식

        [ExcelColumn("Req_Charm")]
        public string ReqCharm;

        [ExcelColumn("Req_Int")]
        public string ReqInt;

        [ExcelColumn("Req_Art")]
        public string ReqArt;

        [ExcelColumn("Req_Morality")]
        public string ReqMorality;

        [ExcelColumn("Req_Stress")]
        public string ReqStress;  // "<=400" 형식

        [ExcelColumn("Req_Favor_Ino")]
        public string ReqFavorIno;  // ">=60" 또는 "80-100" 형식

        [ExcelColumn("Req_Favor_Aileen")]
        public string ReqFavorAileen;

        [ExcelColumn("Req_Favor_Kyle")]
        public string ReqFavorKyle;

        [ExcelColumn("Req_Favor_Lian")]
        public string ReqFavorLian;

        [ExcelColumn("Req_Events")]
        public string ReqEvents;

        [ExcelColumn("Req_Choices")]
        public string ReqChoices;

        [ExcelColumn("Req_Flags")]
        public string ReqFlags;

        [ExcelColumn("Script_Path")]
        public string ScriptPath;

        [ExcelColumn("CG_Path")]
        public string CGPath;

        [ExcelColumn("BGM_Path")]
        public string BGMPath;

        [ExcelColumn("Description")]
        [TextArea(2, 5)]
        public string Description;

        [ExcelColumn("Unlock_Next_Run")]
        public bool UnlockNextRun;
    }
}
