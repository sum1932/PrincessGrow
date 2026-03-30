using System;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Adapters.Interfaces
{
    // Core에서 Unity로의 출력 인터페이스
    public interface IGamePresenter
    {
        void DisplayTurn(GameTurn turn);
        void DisplayStats(CharacterStats stats);
        void DisplayEconomy(Economy economy);
        void DisplayScheduleSelection();
        void DisplayEvent(GameEvent gameEvent);
        void DisplayEnding(EndingCondition ending);
        void ShowMessage(string message);
    }

    // 표시용 데이터 클래스들
    public class TurnDisplayData
    {
        public int Turn { get; set; }
        public int Age { get; set; }
        public int Month { get; set; }
        public int Chapter { get; set; }
        public string Season { get; set; }
    }

    public class StatsDisplayData
    {
        public int HP { get; set; }
        public int Charm { get; set; }
        public int Intelligence { get; set; }
        public int Art { get; set; }
        public int Morality { get; set; }
        public int Stress { get; set; }
        public string Personality { get; set; }
    }
}
