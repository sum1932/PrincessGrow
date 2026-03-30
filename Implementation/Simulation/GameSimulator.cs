using System;
using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;
using DessertKingdom.Core.Services;

namespace DessertKingdom.Simulation
{
    public class GameSimulator
    {
        private readonly ITurnManager _turnManager;
        private readonly IEventManager _eventManager;
        private readonly IEndingJudge _endingJudge;
        private readonly GameState _state;
        private ISimulationStrategy _strategy;

        public GameSimulator(ITurnManager turnManager, IEventManager eventManager, 
                            IEndingJudge endingJudge, GameState state)
        {
            _turnManager = turnManager;
            _eventManager = eventManager;
            _endingJudge = endingJudge;
            _state = state;
            
            // EventManager 초기화
            if (_eventManager is EventManager em)
            {
                em.Initialize(state);
            }
        }

        public void SetStrategy(ISimulationStrategy strategy)
        {
            _strategy = strategy;
        }

        public SimulationResult RunFullSimulation()
        {
            var results = new List<TurnResult>();
            int consecutiveFailures = 0;
            const int maxConsecutiveFailures = 3;
            
            while (!_state.Turn.IsLastTurn())
            {
                // 자동으로 일정 선택
                var (activity1, activity2) = _strategy.SelectActivities(_state);
                
                // 활동이 null이거나 SetSchedule 실패 시 처리
                bool scheduleSet = false;
                if (activity1 != null && activity2 != null)
                {
                    scheduleSet = _turnManager.SetSchedule(activity1, activity2);
                }
                
                if (scheduleSet)
                {
                    consecutiveFailures = 0;
                    var result = _turnManager.EndTurn();
                    results.Add(result);
                    
                    // 이벤트 처리
                    foreach (var evt in result.TriggeredEvents)
                    {
                        ProcessEvent(evt);
                    }
                }
                else
                {
                    consecutiveFailures++;
                    Console.WriteLine($"[경고] 턴 {_state.Turn.CurrentTurn} (나이 {_state.Turn.CurrentAge}세 {_state.Turn.CurrentMonth}월) 설정 실패 ({consecutiveFailures}/{maxConsecutiveFailures})");
                    
                    if (activity1 == null || activity2 == null)
                    {
                        Console.WriteLine("  → 활동이 null입니다.");
                    }
                    else if (!_state.Economy.CanAfford(activity1.Cost + activity2.Cost))
                    {
                        Console.WriteLine($"  → 자금 부족: 현재 {_state.Economy.CurrentMoney}, 필요 {activity1.Cost + activity2.Cost}");
                    }
                    
                    if (consecutiveFailures >= maxConsecutiveFailures)
                    {
                        Console.WriteLine("[오류] 연속 설정 실패로 시뮬레이션을 종료합니다.");
                        break;
                    }
                    
                    // 비용 문제일 경우 한 턴 쉬고 진행
                    if (_state.Economy.CurrentMoney < 100)
                    {
                        Console.WriteLine("  → 자금이 너무 부족하여 기본 생활비로만 진행합니다.");
                        _state.Turn.AdvanceTurn();
                        _state.Economy.ProcessMonthly();
                        consecutiveFailures = 0;
                    }
                }
            }
            
            // 엔딩 판정
            var ending = _endingJudge.EvaluateEnding(_state);
            
            return new SimulationResult
            {
                TurnResults = results,
                FinalState = _state,
                Ending = ending
            };
        }

        private void ProcessEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return;
            
            if (gameEvent.Choices?.Count > 0)
            {
                var choice = _strategy.MakeChoice(gameEvent, gameEvent.Choices);
                if (choice != null)
                {
                    _eventManager.ProcessEventChoice(gameEvent, choice);
                }
            }
        }
    }

    public class SimulationResult
    {
        public List<TurnResult> TurnResults { get; set; }
        public GameState FinalState { get; set; }
        public EndingCondition Ending { get; set; }
    }

    public interface ISimulationStrategy
    {
        (Activity Activity1, Activity Activity2) SelectActivities(GameState state);
        EventChoice MakeChoice(GameEvent gameEvent, List<EventChoice> choices);
    }

    public class RandomStrategy : ISimulationStrategy
    {
        private readonly Random _random = new Random();
        private readonly List<Activity> _activities;

        public RandomStrategy(List<Activity> activities)
        {
            _activities = activities;
        }

        public (Activity Activity1, Activity Activity2) SelectActivities(GameState state)
        {
            var available = _activities.Where(a => a.IsAvailable(state.Turn.CurrentAge, state.Character)).ToList();
            
            if (available.Count == 0)
                return (null, null);
            
            var a1 = available[_random.Next(available.Count)];
            var a2 = available[_random.Next(available.Count)];
            
            return (a1, a2);
        }

        public EventChoice MakeChoice(GameEvent gameEvent, List<EventChoice> choices)
        {
            var available = choices.Where(c => c.IsAvailable(null)).ToList();
            if (available.Count == 0)
                return choices.FirstOrDefault();
            
            return available[_random.Next(available.Count)];
        }
    }
}
