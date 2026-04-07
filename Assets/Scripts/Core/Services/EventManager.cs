using System;
using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Services
{
    public interface IEventManager
    {
        event Action<GameEvent> OnEventTriggered;
        event Action<GameEvent, EventChoice> OnChoiceMade;
        
        List<GameEvent> CheckEvents(GameState state);
        void ProcessEventChoice(GameEvent gameEvent, EventChoice choice, GameState state);
    }

    public class EventManager : IEventManager
    {
        public event Action<GameEvent> OnEventTriggered;
        public event Action<GameEvent, EventChoice> OnChoiceMade;
        
        private readonly List<GameEvent> _allEvents;
        private readonly Random _random;
        
        public EventManager(List<GameEvent> events)
        {
            _allEvents = events ?? new List<GameEvent>();
            _random = new Random();
        }
        
        public List<GameEvent> CheckEvents(GameState state)
        {
            var availableEvents = new List<GameEvent>();
            
            foreach (var evt in _allEvents)
            {
                // CanTrigger에서 이미 CompletedEvents 체크함
                if (evt.CanTrigger(state))
                {
                    availableEvents.Add(evt);
                }
            }
            
            // 우선순위 정렬 (높은 우선순위 먼저, 같은 우선순위면 랜덤)
            return availableEvents.OrderByDescending(e => e.Priority).ThenBy(e => _random.Next()).ToList();
        }
        
        public void ProcessEventChoice(GameEvent gameEvent, EventChoice choice, GameState state)
        {
            if (gameEvent == null || choice == null || state == null) return;
            
            // 스탯 효과 적용
            foreach (var effect in choice.StatEffects)
            {
                state.Character.ModifyStat(effect.Key, effect.Value);
            }
            
            // 호감도 효과 적용
            foreach (var favor in choice.FavorEffects)
            {
                var npc = state.GetNPC(favor.Key);
                if (npc != null)
                {
                    npc.ChangeFavorability(favor.Value);
                }
            }
            
            // 선택지 기록
            state.AddChoice(choice.Id);
            
            // 이벤트 완료 처리
            if (gameEvent.IsOneTime)
            {
                state.CompleteEvent(gameEvent.Id);
            }
            
            OnChoiceMade?.Invoke(gameEvent, choice);
        }
    }
}
