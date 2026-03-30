namespace DessertKingdom.Core.Domain
{
    public class NPCRelationship
    {
        public string NPCId { get; }
        public string NPCName { get; }
        public int Favorability { get; private set; }
        public RelationshipLevel Level { get; private set; }

        public NPCRelationship(string npcId, string npcName, int initialFavorability = 0)
        {
            NPCId = npcId;
            NPCName = npcName;
            Favorability = initialFavorability;
            UpdateLevel();
        }

        public void ChangeFavorability(int amount)
        {
            Favorability = System.Math.Clamp(Favorability + amount, 0, 100);
            UpdateLevel();
        }

        private void UpdateLevel()
        {
            Level = Favorability switch
            {
                >= 81 => RelationshipLevel.Bonded,
                >= 61 => RelationshipLevel.Trusted,
                >= 41 => RelationshipLevel.CloseFriend,
                >= 21 => RelationshipLevel.Friend,
                _ => RelationshipLevel.Stranger
            };
        }

        public bool CanTriggerEvent(int requiredLevel)
        {
            return (int)Level >= requiredLevel;
        }

        public bool CanTriggerEvent(RelationshipLevel requiredLevel)
        {
            return Level >= requiredLevel;
        }

        public override string ToString()
        {
            return $"{NPCName}: 호감도 {Favorability} ({Level})";
        }
    }
}
