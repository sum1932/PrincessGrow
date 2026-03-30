using System;
using System.Collections.Generic;
using DessertKingdom.Core.Domain;
using DessertKingdom.Adapters.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace DessertKingdom.Adapters.Unity
{
    /// <summary>
    /// Unity GamePresenter 구현체
    /// Core의 게임 상태를 Unity UI에 표시
    /// </summary>
    public class UnityGamePresenter : MonoBehaviour, IGamePresenter
    {
        [Header("Events")]
        public UnityEvent<TurnDisplayData> OnTurnUpdated;
        public UnityEvent<StatsDisplayData> OnStatsUpdated;
        public UnityEvent<int> OnEconomyUpdated;
        public UnityEvent OnScheduleSelectionRequested;
        public UnityEvent<GameEvent> OnEventDisplayed;
        public UnityEvent<EndingCondition> OnEndingDisplayed;
        public UnityEvent<string> OnMessageShown;

        public void DisplayTurn(GameTurn turn)
        {
            var data = new TurnDisplayData
            {
                Turn = turn.CurrentTurn,
                Age = turn.CurrentAge,
                Month = turn.CurrentMonth,
                Chapter = turn.CurrentChapter,
                Season = turn.CurrentSeason.ToString()
            };
            OnTurnUpdated?.Invoke(data);
        }

        public void DisplayStats(CharacterStats stats)
        {
            var data = new StatsDisplayData
            {
                HP = stats.HP,
                Charm = stats.Charm,
                Intelligence = stats.Intelligence,
                Art = stats.Art,
                Morality = stats.Morality,
                Stress = stats.Stress,
                Personality = stats.GetPersonality().ToString()
            };
            OnStatsUpdated?.Invoke(data);
        }

        public void DisplayEconomy(Economy economy)
        {
            OnEconomyUpdated?.Invoke(economy.CurrentMoney);
        }

        public void DisplayScheduleSelection()
        {
            OnScheduleSelectionRequested?.Invoke();
        }

        public void DisplayEvent(GameEvent gameEvent)
        {
            OnEventDisplayed?.Invoke(gameEvent);
        }

        public void DisplayEnding(EndingCondition ending)
        {
            OnEndingDisplayed?.Invoke(ending);
        }

        public void ShowMessage(string message)
        {
            OnMessageShown?.Invoke(message);
        }
    }

    // Unity Event Wrapper
    [Serializable]
    public class UnityEvent<T0, T1> : UnityEvent<T0> { }
}
