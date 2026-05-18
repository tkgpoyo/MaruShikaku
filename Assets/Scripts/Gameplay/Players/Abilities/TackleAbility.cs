using System.Collections;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class TackleAbility : PlayerAbility
    {
        private const string TRIG_TACKLE_START = "TackleStart";
        private const string TRIG_TACKLE_START_END = "TackleStartEnd";
        private const string TRIG_TACKLE_SUCCESS_END = "TackleSuccessEnd";
        private const string TRIG_TACKLE_SUCCESS_END_END = "TackleSuccessEndEnd";
        private const string TRIG_TACKLE_FAIL_END = "TackleFailEnd";
        private const string TRIG_TACKLE_FAIL_END_END = "TackleFailEndEnd";

        [SerializeField] private Transform _tackleCheck;
        [SerializeField] private Vector2 _tackleCheckSize = new (0.4f, 1.0f);

        private FragileBlock _block;
        private bool _isSuccess;

        protected override bool CanStart(PlayerInputData input)
        {
            return input.Tackle && Context.GroundState.IsGrounded;
        }
        protected override void OnAnticipationStart(PlayerInputData input)
        {
            _isSuccess = false;
            Context.LockMove(this);
            Context.RigidBody.linearVelocityX = 0f;

            var blocks = Physics2D.OverlapBoxAll(_tackleCheck.position, _tackleCheckSize, 0f);

            _block = null;
            foreach (var block in blocks)
            {
                if (block.TryGetComponent<FragileBlock>(out var b))
                {
                    _block = b;
                    break;
                }
            }
        }
        protected override IEnumerator Anticipation(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_TACKLE_START);
            while (!Visual.ConsumeAnimationEvent(TRIG_TACKLE_START_END))
            {
                yield return null;
            }
        }
        protected override IEnumerator Action(PlayerInputData input)
        {
            if (_block != null)
            {
                _isSuccess = true;
                // 壊せるブロックがあるため、壊す
                _block.Break();
            }
            yield break;
        }
        protected override IEnumerator Recovery(PlayerInputData input)
        {
            if (_isSuccess)
            {
                // 成功
                Visual.PlayTrigger(TRIG_TACKLE_SUCCESS_END);
                while (!Visual.ConsumeAnimationEvent(TRIG_TACKLE_SUCCESS_END_END))
                {
                    yield return null;
                }
            }
            else
            {
                // 失敗
                Visual.PlayTrigger(TRIG_TACKLE_FAIL_END);
                while (!Visual.ConsumeAnimationEvent(TRIG_TACKLE_FAIL_END_END))
                {
                    yield return null;
                }
            }
        }
        protected override void OnFinished()
        {
            Context.UnlockMove(this);
        }
        protected override void OnCanceled()
        {
            Context.UnlockMove(this);
        }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_tackleCheck == null) { return; }
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_tackleCheck.position, _tackleCheckSize);
        }
    #endif
    }
}