using System;
using DessertKingdom.Core.Domain;
using DessertKingdom.Adapters.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace DessertKingdom.Adapters.Unity
{
    /// <summary>
    /// Unity GameInput 구현체
    /// UI 버튼 클릭 등을 Core 이벤트로 변환
    /// </summary>
    public class UnityGameInput : MonoBehaviour, IGameInput
    {
        [Header("Events")]
        public UnityEvent<ActivitySelectedEvent> ActivitySelectedEvent;
        public UnityEvent<EventChoiceSelectedEvent> ChoiceSelectedEvent;
        public UnityEvent<CommandEvent> CommandEvent;

        // IGameInput 인터페이스 구현
        public event Action<ActivitySelectedEvent> OnActivitySelected;
        public event Action<EventChoiceSelectedEvent> OnChoiceSelected;
        public event Action<CommandEvent> OnCommand;

        /// <summary>
        /// 활동 선택 시 호출 (UI 버튼에서)
        /// </summary>
        public void SelectActivity(Activity activity, int slot)
        {
            if (activity == null) return;
            
            var evt = new ActivitySelectedEvent(activity, slot);
            OnActivitySelected?.Invoke(evt);
            ActivitySelectedEvent?.Invoke(evt);
        }

        /// <summary>
        /// 이벤트 선택지 선택 시 호출
        /// </summary>
        public void SelectChoice(GameEvent gameEvent, EventChoice choice)
        {
            if (gameEvent == null || choice == null) return;
            
            var evt = new EventChoiceSelectedEvent(gameEvent, choice);
            OnChoiceSelected?.Invoke(evt);
            ChoiceSelectedEvent?.Invoke(evt);
        }

        /// <summary>
        /// 명령 실행 (저장/로드 등)
        /// </summary>
        public void ExecuteCommand(CommandType type, object data = null)
        {
            var evt = new CommandEvent(type, data);
            OnCommand?.Invoke(evt);
            CommandEvent?.Invoke(evt);
        }

        // 편의 메서드들
        public void OnSaveButtonClicked() => ExecuteCommand(CommandType.Save);
        public void OnLoadButtonClicked() => ExecuteCommand(CommandType.Load);
        public void OnNewGameButtonClicked() => ExecuteCommand(CommandType.NewGame);
        public void OnQuitButtonClicked() => ExecuteCommand(CommandType.Quit);
    }

    // Unity Event Wrapper
    [Serializable]
    public class UnityEvent<T> : UnityEngine.Events.UnityEvent<T> { }
}
