using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using System.Linq;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using System.Collections;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class JumpAbility : PlayerAbility
    {
        private const string TRIG_JUMP_START = "JumpStart";
        private const string TRIG_JUMP_START_END = "JumpStartEnd";
        private const string TRIG_JUMP_END = "JumpEnd";
        private const string TRIG_JUMP_END_END = "JumpEndEnd";

        [SerializeField] private float _jumpPower = 8f;

        protected override bool CanStart(PlayerInputData input)
        {
            return input.Jump && Context.GroundState.IsGrounded;
        }
        protected override IEnumerator Anticipation(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_JUMP_START);
            while (!Visual.ConsumeAnimationEvent(TRIG_JUMP_START_END))
            {
                yield return null;
            }
        }
        protected override IEnumerator Action(PlayerInputData input)
        {
            yield return new WaitForFixedUpdate();

            Context.RigidBody.linearVelocityY = 0f;
            Context.RigidBody.AddForceY(_jumpPower, ForceMode2D.Impulse);

            // 地面から離れるまで待機
            while (Context.GroundState.IsGrounded)
            {
                yield return new WaitForFixedUpdate();
            }

            // 地面に着地するまで待機
            while (!Context.GroundState.IsGrounded)
            {
                yield return new WaitForFixedUpdate();
            }
        }
        protected override IEnumerator Recovery(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_JUMP_END);
            while (!Visual.ConsumeAnimationEvent(TRIG_JUMP_END_END))
            {
                yield return null;
            }
        }
    }
}