using System.Collections.Generic;

namespace DessertKingdom.Core.Domain
{
    /// <summary>
    /// 이벤트 대사 데이터
    /// </summary>
    public class EventDialogue
    {
        public string CharacterId { get; }
        public string CharacterName { get; }
        public string PortraitPath { get; }
        public List<DialogueLine> Lines { get; }
        public bool ShowStandIllustration { get; }
        public string StandIllustrationPath { get; }

        public EventDialogue(string characterId, string characterName, string portraitPath,
                           List<DialogueLine> lines, bool showStandIllustration = false,
                           string standIllustrationPath = null)
        {
            CharacterId = characterId;
            CharacterName = characterName;
            PortraitPath = portraitPath;
            Lines = lines ?? new List<DialogueLine>();
            ShowStandIllustration = showStandIllustration;
            StandIllustrationPath = standIllustrationPath;
        }
    }

    /// <summary>
    /// 개별 대사 라인
    /// </summary>
    public class DialogueLine
    {
        public string Text { get; }
        public DialogueEffect Effect { get; }
        public float DisplayDuration { get; }

        public DialogueLine(string text, DialogueEffect effect = null, float displayDuration = 0f)
        {
            Text = text;
            Effect = effect;
            DisplayDuration = displayDuration;
        }
    }

    /// <summary>
    /// 대사 효과 (스탯 변화 등)
    /// </summary>
    public class DialogueEffect
    {
        public Dictionary<StatType, int> StatChanges { get; }
        public Dictionary<string, int> FavorChanges { get; }
        public bool ShowStatText { get; }

        public DialogueEffect(Dictionary<StatType, int> statChanges = null,
                            Dictionary<string, int> favorChanges = null,
                            bool showStatText = true)
        {
            StatChanges = statChanges ?? new Dictionary<StatType, int>();
            FavorChanges = favorChanges ?? new Dictionary<string, int>();
            ShowStatText = showStatText;
        }
    }

    /// <summary>
    /// 캐릭터 정의
    /// </summary>
    public static class CharacterDefinitions
    {
        public const string INO = "Ino";
        public const string AILEEN = "Aileen";
        public const string KYLE = "Kyle";
        public const string LIAN = "Lian";
        public const string LUA = "Lua"; // 플레이어 (루아)
        public const string NARRATOR = "Narrator"; // 나레이션

        public static string GetCharacterName(string characterId)
        {
            return characterId switch
            {
                INO => "이노",
                AILEEN => "아이린",
                KYLE => "카일",
                LIAN => "리안",
                LUA => "루아",
                NARRATOR => "",
                _ => characterId
            };
        }

        public static string GetPortraitPath(string characterId)
        {
            return $"Characters/Portraits/{characterId}";
        }

        public static string GetStandIllustrationPath(string characterId)
        {
            return $"Characters/Stand/{characterId}";
        }

        public static bool HasStandIllustration(string characterId)
        {
            // 이노는 플레이어 캐릭터이므로 스탠드 일러스트 없음
            return characterId != INO && characterId != NARRATOR && characterId != LUA;
        }
    }
}
