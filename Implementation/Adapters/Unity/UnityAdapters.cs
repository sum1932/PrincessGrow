// Unity 전용 - MonoBehaviour 기반 어댑터
// 참고: 이 파일은 Unity 프로젝트에 추가되어야 함

using UnityEngine;
using System;
using DessertKingdom.Adapters.Interfaces;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Adapters.Unity
{
    public class UnityGameInput : MonoBehaviour, IGameInput
    {
        public event Action<ActivitySelectedEvent> OnActivitySelected;
        public event Action<EventChoiceSelectedEvent> OnChoiceSelected;
        public event Action<CommandEvent> OnCommand;

        // UI 버튼에서 호출
        public void SelectActivity(string activityId, int slot)
        {
            // Activity는 Repository에서 조회
            OnActivitySelected?.Invoke(new ActivitySelectedEvent(null, slot));
        }

        public void SelectChoice(GameEvent evt, EventChoice choice)
        {
            OnChoiceSelected?.Invoke(new EventChoiceSelectedEvent(evt, choice));
        }

        public void SendCommand(CommandType type, object data = null)
        {
            OnCommand?.Invoke(new CommandEvent(type, data));
        }
    }

    public class UnityGamePresenter : MonoBehaviour, IGamePresenter
    {
        [Header("UI References")]
        public TurnView turnView;
        public StatsView statsView;
        public EconomyView economyView;
        public ScheduleView scheduleView;
        public EventView eventView;
        public EndingView endingView;
        public MessageView messageView;

        public void DisplayTurn(GameTurn turn)
        {
            if (turnView != null)
                turnView.Show(turn);
        }

        public void DisplayStats(CharacterStats stats)
        {
            if (statsView != null)
                statsView.Show(stats);
        }

        public void DisplayEconomy(Economy economy)
        {
            if (economyView != null)
                economyView.Show(economy);
        }

        public void DisplayScheduleSelection()
        {
            if (scheduleView != null)
                scheduleView.Show();
        }

        public void DisplayEvent(GameEvent gameEvent)
        {
            if (eventView != null)
                eventView.Show(gameEvent);
        }

        public void DisplayEnding(EndingCondition ending)
        {
            if (endingView != null)
                endingView.Show(ending);
        }

        public void ShowMessage(string message)
        {
            if (messageView != null)
                messageView.Show(message);
        }
    }

    // Unity View 클래스들 (스텁)
    public class TurnView : MonoBehaviour
    {
        public void Show(GameTurn turn)
        {
            Debug.Log($"턴: {turn.CurrentTurn}, 나이: {turn.CurrentAge}, 월: {turn.CurrentMonth}");
        }
    }

    public class StatsView : MonoBehaviour
    {
        public void Show(CharacterStats stats)
        {
            Debug.Log($"스탯: HP={stats.HP}, 매력={stats.Charm}, 지능={stats.Intelligence}");
        }
    }

    public class EconomyView : MonoBehaviour
    {
        public void Show(Economy economy)
        {
            Debug.Log($"자산: {economy.CurrentMoney}");
        }
    }

    public class ScheduleView : MonoBehaviour
    {
        public void Show()
        {
            Debug.Log("일정 선택 화면 표시");
        }
    }

    public class EventView : MonoBehaviour
    {
        public void Show(GameEvent evt)
        {
            Debug.Log($"이벤트: {evt.Name}");
        }
    }

    public class EndingView : MonoBehaviour
    {
        public void Show(EndingCondition ending)
        {
            Debug.Log($"엔딩: {ending.Name}");
        }
    }

    public class MessageView : MonoBehaviour
    {
        public void Show(string message)
        {
            Debug.Log($"메시지: {message}");
        }
    }
}
