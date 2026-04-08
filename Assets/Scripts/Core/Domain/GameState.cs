using System.Collections.Generic;
using System.Linq;

namespace DessertKingdom.Core.Domain
{
    public class GameState
    {
        public GameTurn Turn { get; }
        public CharacterStats Character { get; }
        public Economy Economy { get; }
        public MonthlySchedule CurrentSchedule { get; }
        public List<NPCRelationship> NPCs { get; }
        public List<string> CompletedEvents { get; }
        public List<string> ChoiceHistory { get; }
        public GamePhase Phase { get; private set; }
        public bool PrologueShown { get; set; }
        public Inventory Inventory { get; }
        
        // 플레이어 및 육성대상 이름
        public string PlayerName { get; set; } = "이노";      // 기본값: 이노
        public string TargetName { get; set; } = "루아";     // 기본값: 루아

        public GameState()
        {
            Turn = new GameTurn();
            Character = new CharacterStats();
            Economy = new Economy();
            CurrentSchedule = new MonthlySchedule();
            Inventory = new Inventory();
            NPCs = new List<NPCRelationship>();
            CompletedEvents = new List<string>();
            ChoiceHistory = new List<string>();
            Phase = GamePhase.Prologue;

            // 기본 NPC 초기화
            InitializeNPCs();
        }

        private void InitializeNPCs()
        {
            NPCs.Add(new NPCRelationship("ino", "이노", 50));      // 주인공
            NPCs.Add(new NPCRelationship("aileen", "아이린", 0));  // 소꿉친구
            NPCs.Add(new NPCRelationship("kyle", "카일", 0));      // 호위원
            NPCs.Add(new NPCRelationship("lian", "리안", 0));      // 인간 친구 (13세에 등장)
        }

        public NPCRelationship GetNPC(string npcId)
        {
            return NPCs.FirstOrDefault(n => n.NPCId == npcId.ToLower());
        }

        public void SetPhase(GamePhase phase)
        {
            Phase = phase;
        }

        public void CompleteEvent(string eventId)
        {
            if (!CompletedEvents.Contains(eventId))
                CompletedEvents.Add(eventId);
        }

        public void AddChoice(string choiceId)
        {
            ChoiceHistory.Add(choiceId);
        }

        public void ModifyNPCFavor(string npcId, int amount)
        {
            var npc = GetNPC(npcId);
            if (npc != null)
                npc.ChangeFavorability(amount);
        }

        public string Serialize()
        {
            // TODO: 직렬화 구현 (JSON 등)
            return $"Turn:{Turn.CurrentTurn},Age:{Turn.CurrentAge}";
        }

        public static GameState Deserialize(string data)
        {
            // TODO: 역직렬화 구현
            return new GameState();
        }

        public GameState Clone()
        {
            var clone = new GameState();
            // TODO: 깊은 복사 구현
            return clone;
        }

        public override string ToString()
        {
            return $"턴: {Turn}, 스탯: {Character}, 자산: {Economy.CurrentMoney}";
        }
    }
}
