using System.Collections.Generic;
using UnityEngine;

namespace DessertKingdom.Views
{
    /// <summary>
    /// 캐릭터 정의 및 리소스 경로 관리
    /// </summary>
    public static class CharacterDefinitions
    {
        // 캐릭터 이름 매핑
        private static readonly Dictionary<string, string> CharacterNames = new()
        {
            { "Narrator", "나레이터" },
            { "Ino", "이노" },
            { "Rua", "루아" },
            { "Player", "플레이어" },
            { "System", "시스템" }
        };

        // 스탠드 일러스트를 가진 캐릭터 목록
        private static readonly HashSet<string> CharactersWithStand = new()
        {
            "Ino",
            "Rua",
            "Player"
        };

        /// <summary>
        /// 캐릭터 ID로 이름을 가져옵니다
        /// </summary>
        public static string GetCharacterName(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return "나레이터";

            if (CharacterNames.TryGetValue(characterId, out var name))
                return name;

            return characterId;
        }

        /// <summary>
        /// 해당 캐릭터가 스탠드 일러스트를 가지고 있는지 확인
        /// </summary>
        public static bool HasStandIllustration(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return false;

            return CharactersWithStand.Contains(characterId);
        }

        /// <summary>
        /// 캐릭터 초상화 경로 가져오기
        /// </summary>
        public static string GetPortraitPath(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return "Characters/Portraits/Narrator";

            return $"Characters/Portraits/{characterId}";
        }

        /// <summary>
        /// 캐릭터 스탠드 일러스트 경로 가져오기
        /// </summary>
        public static string GetStandIllustrationPath(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return "Characters/Stands/Narrator";

            return $"Characters/Stands/{characterId}";
        }

        /// <summary>
        /// 캐릭터 이름 등록 (런타임에 추가)
        /// </summary>
        public static void RegisterCharacterName(string characterId, string name)
        {
            CharacterNames[characterId] = name;
        }

        /// <summary>
        /// 스탠드 일러스트 캐릭터 등록 (런타임에 추가)
        /// </summary>
        public static void RegisterStandCharacter(string characterId)
        {
            CharactersWithStand.Add(characterId);
        }
    }
}
