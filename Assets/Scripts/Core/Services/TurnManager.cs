using System;
using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Services
{
    public interface ITurnManager
    {
        event Action<TurnStartedEvent> OnTurnStarted;
        event Action<TurnEndedEvent> OnTurnEnded;
        event Action<TurnResult> OnTurnProcessed;
        
        bool SetSchedule(Activity activity1, Activity activity2);
        TurnResult EndTurn();
        bool CanEndTurn();
    }

    public class TurnStartedEvent
    {
        public int Turn { get; }
        public int Age { get; }
        public int Month { get; }
        
        public TurnStartedEvent(int turn, int age, int month)
        {
            Turn = turn;
            Age = age;
            Month = month;
        }
    }

    public class TurnEndedEvent
    {
        public int Turn { get; }
        public List<GameEvent> TriggeredEvents { get; }
        
        public TurnEndedEvent(int turn, List<GameEvent> events)
        {
            Turn = turn;
            TriggeredEvents = events;
        }
    }

    public class TurnResult
    {
        public bool Success { get; }
        public string Message { get; }
        public ScheduleEffects ScheduleEffects { get; }
        public MonthlyResult MonthlyResult { get; }
        public List<GameEvent> TriggeredEvents { get; }
        public bool IsEnding { get; }
        
        public TurnResult(bool success, string message, ScheduleEffects scheduleEffects,
                         MonthlyResult monthlyResult, List<GameEvent> events, bool isEnding)
        {
            Success = success;
            Message = message;
            ScheduleEffects = scheduleEffects;
            MonthlyResult = monthlyResult;
            TriggeredEvents = events;
            IsEnding = isEnding;
        }
    }

    public class ScheduleEffects
    {
        public Activity Activity1 { get; }
        public Activity Activity2 { get; }
        public Dictionary<StatType, int> TotalStatChanges { get; }
        public int TotalStressChange { get; }
        public int TotalCost { get; }
        public int TotalIncome { get; }
        
        public ScheduleEffects(Activity a1, Activity a2, Dictionary<StatType, int> statChanges,
                              int stressChange, int cost, int income)
        {
            Activity1 = a1;
            Activity2 = a2;
            TotalStatChanges = statChanges;
            TotalStressChange = stressChange;
            TotalCost = cost;
            TotalIncome = income;
        }
    }

    public class MonthlyResult
    {
        public int Income { get; }
        public int Expense { get; }
        public int Net { get; }
        
        public MonthlyResult(int income, int expense)
        {
            Income = income;
            Expense = expense;
            Net = income - expense;
        }
    }

    public class TurnManager : ITurnManager
    {
        public event Action<TurnStartedEvent> OnTurnStarted;
        public event Action<TurnEndedEvent> OnTurnEnded;
        public event Action<TurnResult> OnTurnProcessed;
        
        private readonly GameState _state;
        private readonly IEventManager _eventManager;
        
        public TurnManager(GameState state, IEventManager eventManager)
        {
            _state = state;
            _eventManager = eventManager;
        }
        
        public bool SetSchedule(Activity activity1, Activity activity2)
        {
            if (activity1 == null || activity2 == null)
                return false;
                
            // 활동 비용 체크
            int totalCost = activity1.Cost + activity2.Cost;
            if (!_state.Economy.CanAfford(totalCost))
                return false;
                
            _state.CurrentSchedule.SetActivity(activity1, 1);
            _state.CurrentSchedule.SetActivity(activity2, 2);
            _state.SetPhase(GamePhase.ActivityProcessing);
            
            return true;
        }
        
        public TurnResult EndTurn()
        {
            if (!_state.CurrentSchedule.IsComplete)
            {
                return new TurnResult(false, "일정이 완료되지 않았습니다.", null, null, null, false);
            }
            
            OnTurnStarted?.Invoke(new TurnStartedEvent(_state.Turn.CurrentTurn, 
                                                      _state.Turn.CurrentAge, 
                                                      _state.Turn.CurrentMonth));
            
            // 1. 일정 효과 적용
            var scheduleEffects = ApplySchedule();
            
            // 2. 월간 정산
            var monthlyResult = ProcessMonthly();
            
            // 3. 이벤트 체크
            var triggeredEvents = _eventManager.CheckEvents(_state);
            
            // 4. 다음 턴 준비
            _state.Turn.AdvanceTurn();
            _state.CurrentSchedule.Clear();
            
            // 5. 엔딩 체크
            bool isEnding = _state.Turn.IsLastTurn();
            
            OnTurnEnded?.Invoke(new TurnEndedEvent(_state.Turn.CurrentTurn, triggeredEvents));
            
            var result = new TurnResult(true, "턴이 종료되었습니다.", scheduleEffects, 
                                       monthlyResult, triggeredEvents, isEnding);
            OnTurnProcessed?.Invoke(result);
            
            return result;
        }
        
        public bool CanEndTurn()
        {
            return _state.CurrentSchedule.IsComplete;
        }
        
        private ScheduleEffects ApplySchedule()
        {
            var schedule = _state.CurrentSchedule;
            
            // 비용 처리
            int totalCost = schedule.GetTotalCost();
            _state.Economy.Spend(totalCost, "활동 비용");
            
            // 수입 처리
            int totalIncome = schedule.GetTotalIncome();
            _state.Economy.Earn(totalIncome, "활동 수입");
            
            // 스탯 효과 적용
            schedule.ApplyEffects(_state.Character);
            
            // 스트레스 체크
            if (_state.Character.GetStressLevel() == StressLevel.Dangerous)
            {
                _state.Character.ModifyStat(StatType.Stress, -50);
                _state.Character.ModifyStat(StatType.Morality, -10);
            }
            
            // 결과 수집
            var statChanges = new Dictionary<StatType, int>();
            if (schedule.Activity1 != null)
            {
                foreach (var effect in schedule.Activity1.StatEffects)
                {
                    statChanges[effect.Key] = statChanges.GetValueOrDefault(effect.Key, 0) + effect.Value;
                }
            }
            if (schedule.Activity2 != null)
            {
                foreach (var effect in schedule.Activity2.StatEffects)
                {
                    statChanges[effect.Key] = statChanges.GetValueOrDefault(effect.Key, 0) + effect.Value;
                }
            }
            
            return new ScheduleEffects(schedule.Activity1, schedule.Activity2, statChanges,
                                      schedule.GetTotalStressChange(), totalCost, totalIncome);
        }
        
        private MonthlyResult ProcessMonthly()
        {
            _state.Economy.ProcessMonthly();
            return new MonthlyResult(400, 350); // 기본값
        }
    }
}
