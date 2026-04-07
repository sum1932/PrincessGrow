using System;
using System.Collections.Generic;
using DessertKingdom.Core.Domain;
using DessertKingdom.Adapters.Interfaces;
using DessertKingdom.Controllers;
using DessertKingdom.Views;
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
        [Header("View References")]
        public TurnView turnView;
        public StatsView statsView;
        public EconomyView economyView;
        public ActivityPanelView activityPanelView;
        public MonthlyScheduleView monthlyScheduleView;  // 월간 스케줄 표
        public EventView eventView;
        public EndingView endingView;
        public MessagePanelView messagePanelView;

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
            if (activityPanelView != null)
            {
                // GameController에서 활동 목록을 받아서 표시
                var activities = GameController.Instance?.GetAvailableActivities();
                var gameState = GameController.Instance?.GetGameState();
                if (activities != null && gameState != null)
                {
                    activityPanelView.ShowActivities(activities, gameState.Turn.CurrentAge);
                }
            }
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
            if (messagePanelView != null)
                messagePanelView.Show(message);
        }
    }

    // Unity Event Wrapper
    [Serializable]
    public class UnityEvent<T0, T1> : UnityEvent<T0> { }
}
