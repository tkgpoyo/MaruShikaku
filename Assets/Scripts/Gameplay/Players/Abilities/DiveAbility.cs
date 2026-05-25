using System.Collections;
using MaruSikaku.Gameplay.Players.Inputs;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class DiveAbility : PlayerAbility
    {
        /// <summary>ダイブ開始アニメーションのトリガー名</summary>
        private const string TRIG_DIVE_START = "DiveStart";
        /// <summary>ダイブ開始アニメーションの終了時イベント名</summary>
        private const string DIVE_START_END_EVENT = "DiveStartEnd";
        /// <summary>ダイブ終了アニメーションのトリガー名</summary>
        private const string TRIG_DIVE_END = "DiveEnd";
        /// <summary>ダイブ終了アニメーションの終了時イベント名</summary>
        private const string DIVE_END_END_EVENT = "DiveEndEnd";

        /// <inheritdoc/>
        protected override bool CanStart(PlayerInputData input)
        {
            return input.Dive && Context.GroundState.IsGrounded;
        }
        /// <inheritdoc/>
        protected override void OnActionStart(PlayerInputData input)
        {
            Context.LockMove(this);                 // 移動を禁止
        }
        /// <inheritdoc/>
        protected override IEnumerator Anticipation(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_DIVE_START);    // ダイブ開始アニメーションを開始
            while (!Visual.ConsumeAnimationEvent(DIVE_START_END_EVENT)) // ダイブ開始アニメーションが終了するまで
            {
                yield return null;                  // 1フレーム待つ
            }
        }
        /// <inheritdoc/>
        protected override IEnumerator Action(PlayerInputData input)
        {
            return base.Action(input);
        }
        /// <inheritdoc/>
        protected override IEnumerator Recovery(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_DIVE_END);      // ダイブ終了アニメーションを開始
            while (!Visual.ConsumeAnimationEvent(DIVE_END_END_EVENT))   // ダイブ終了アニメーションが終了するまで
            {
                yield return null;                  // 1フレーム待つ
            }
        }
        /// <inheritdoc/>
        protected override void OnFinished()
        {
            Context.UnlockMove(this);               // 動きの制限を解除
        }
    }
}