using System;
using MaruSikaku.Gameplay.Players.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MaruSikaku.Gameplay.Players.Visuals
{
    public class SikakuVisual : PlayerVisual
    {
        private const string TRIG_PRESS_START = "PressStart";
        private const string TRIG_PRESS_END = "PressEnd";
        private const string TRIG_TACKLE_START = "TackleStart";

        public event Action OnLockMove;
        public event Action OnPressStartAnimEnd;
        public event Action OnPressEndAnimEnd;

        protected override void UpdateAnimation(PlayerContext context)
        {
        } 

        public void PressStart()
        {
            _anim.SetTrigger(TRIG_PRESS_START);
        }

        public void PressEnd()
        {
            _anim.SetTrigger(TRIG_PRESS_END);
        }

        public void LockMove()
        {
            OnLockMove?.Invoke();
        }

        public void PressStartAnimEnd()
        {
            OnPressStartAnimEnd?.Invoke();
        }

        public void PressEndAnimEnd()
        {
            OnPressEndAnimEnd?.Invoke();
        }

        public void TackleStart()
        {
            _anim.SetTrigger(TRIG_TACKLE_START);
        }
    }
}