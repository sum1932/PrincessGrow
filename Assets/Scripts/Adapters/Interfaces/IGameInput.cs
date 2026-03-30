using System;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Adapters.Interfaces
{
    // Unity에서 Core로의 입력 인터페이스
    public interface IGameInput
    {
        event Action<ActivitySelectedEvent> OnActivitySelected;
        event Action<EventChoiceSelectedEvent> OnChoiceSelected;
        event Action<CommandEvent> OnCommand;
    }

    public class ActivitySelectedEvent
    {
        public Activity Activity { get; }
        public int Slot { get; }
        
        public ActivitySelectedEvent(Activity activity, int slot)
        {
            Activity = activity;
            Slot = slot;
        }
    }

    public class EventChoiceSelectedEvent
    {
        public GameEvent GameEvent { get; }
        public EventChoice Choice { get; }
        
        public EventChoiceSelectedEvent(GameEvent gameEvent, EventChoice choice)
        {
            GameEvent = gameEvent;
            Choice = choice;
        }
    }

    public class CommandEvent
    {
        public CommandType Type { get; }
        public object Data { get; }
        
        public CommandEvent(CommandType type, object data = null)
        {
            Type = type;
            Data = data;
        }
    }

    public enum CommandType
    {
        Save,
        Load,
        NewGame,
        Quit
    }
}
