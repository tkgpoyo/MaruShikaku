using System;
using MaruSikaku.Gameplay.Players.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MaruSikaku.Gameplay.Players.Visuals
{
    public class MaruVisual : PlayerVisual
    {
        private const string TRIG_JUMP_START = "JumpStart";
        private const string TRIG_JUMP_END = "JumpEnd";

        public event Action OnJumpStartAnimEnd;
        public event Action OnJumpEndAnimEnd;

        protected override void UpdateAnimation(PlayerContext context)
        {
        } 

        public void JumpStart()
        {
            _anim.SetTrigger(TRIG_JUMP_START);
        }

        public void JumpStartAnimEnd()
        {
            OnJumpStartAnimEnd?.Invoke();
        }

        public void JumpEnd()
        {
            _anim.SetTrigger(TRIG_JUMP_END);
        }

        public void JumpEndAnimEnd()
        {
            OnJumpEndAnimEnd?.Invoke();
        }
    }
}