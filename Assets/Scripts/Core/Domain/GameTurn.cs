namespace DessertKingdom.Core.Domain
{
    public class GameTurn
    {
        public int CurrentTurn { get; private set; }      // 0~144
        public int CurrentAge { get; private set; }       // 6~18
        public int CurrentMonth { get; private set; }     // 1~12
        public int CurrentChapter { get; private set; }   // 1~3
        public Season CurrentSeason { get; private set; }

        private const int StartAge = 6;
        private const int EndAge = 18;
        private const int TotalTurns = 144;
        private const int MonthsPerYear = 12;

        public GameTurn()
        {
            CurrentTurn = 0;
            CurrentAge = StartAge;
            CurrentMonth = 1;
            CurrentChapter = 1;
            UpdateSeason();
        }

        public void AdvanceTurn()
        {
            if (IsLastTurn())
                return;

            CurrentTurn++;
            CurrentMonth++;

            if (CurrentMonth > MonthsPerYear)
            {
                CurrentMonth = 1;
                CurrentAge++;
            }

            UpdateChapter();
            UpdateSeason();
        }

        private void UpdateChapter()
        {
            CurrentChapter = CurrentAge switch
            {
                >= 6 and <= 10 => 1,
                >= 11 and <= 15 => 2,
                >= 16 and <= 18 => 3,
                _ => 3
            };
        }

        private void UpdateSeason()
        {
            CurrentSeason = CurrentMonth switch
            {
                >= 3 and <= 5 => Season.Spring,
                >= 6 and <= 8 => Season.Summer,
                >= 9 and <= 11 => Season.Autumn,
                _ => Season.Winter
            };
        }

        public bool IsLastTurn() => CurrentTurn >= TotalTurns;
        public int GetRemainingTurns() => TotalTurns - CurrentTurn;
        public int GetTurnsInCurrentYear() => CurrentMonth - 1;
        public bool IsBirthday() => CurrentMonth == 12 && GetDayOfMonth() == 24;
        
        private int GetDayOfMonth() => 24; // 생일은 항상 24일로 고정

        public override string ToString()
        {
            return $"{CurrentAge}세 {CurrentMonth}월 (턴 {CurrentTurn}/144) - 챕터 {CurrentChapter}";
        }
    }
}
