using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using System.Linq;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using System.Collections;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    /// <summary>
    /// ジャンプ能力を表すクラス
    /// </summary>
    public class JumpAbility : PlayerAbility
    {
        /// <summary>ジャンプ開始アニメーションのトリガー名</summary>
        private const string TRIG_JUMP_START = "JumpStart";
        /// <summary>ジャンプ開始アニメーションの終了時イベント名</summary>
        private const string JUMP_START_END_EVENT = "JumpStartEnd";
        /// <summary>ジャンプ終了アニメーションのトリガー名</summary>
        private const string TRIG_JUMP_END = "JumpEnd";
        /// <summary>ジャンプ終了アニメーションの終了時イベント名</summary>
        private const string JUMP_END_END_EVENT = "JumpEndEnd";

        /// <summary>ジャンプの瞬間の垂直方向の速さ</summary>
        [SerializeField] private float _jumpVelocity = 8f;

        /// <inheritdoc/>
        protected override bool CanStart(PlayerInputData input)
        {
            return input.Jump && Context.GroundState.IsGrounded;            // ジャンプ入力かつ地上にいるか
        }
        /// <inheritdoc/>
        protected override IEnumerator Anticipation(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_JUMP_START);                            // ジャンプ開始アニメーションの開始
            while (!Visual.ConsumeAnimationEvent(JUMP_START_END_EVENT))     // ジャンプ開始アニメーションが終了するまで
            {
                yield return null;                                          // 1フレーム待機
            }
        }
        /// <inheritdoc/>
        protected override IEnumerator Action(PlayerInputData input)
        {
            // 物理演算を行うため、FixedUpdateに同期する
            yield return new WaitForFixedUpdate();                          // FixedUpdateまで待機

            Context.RigidBody.linearVelocityY = _jumpVelocity;              // 垂直方向の速さを設定することで、ジャンプさせる

            // ジャンプの着地を安全に判定するため、地面から離れるまで一旦待機
            while (Context.GroundState.IsGrounded)                          // 地面から離れるまで
            {
                yield return new WaitForFixedUpdate();                      // 待機(GroundStateは物理演算による判定を行うため、FixedUpdateに同期)
            }

            while (!Context.GroundState.IsGrounded)                         // 地面に着地するまで
            {
                yield return new WaitForFixedUpdate();                      // 待機(GroundStateは物理演算による判定を行うため、FixedUpdateに同期)
            }
        }
        /// <inheritdoc/>
        protected override IEnumerator Recovery(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_JUMP_END);                              // ジャンプ終了アニメーションの開始
            while (!Visual.ConsumeAnimationEvent(JUMP_END_END_EVENT))       // ジャンプ終了アニメーションが終了するまで
            {
                yield return null;                                          // 1フレーム待機
            }
        }
    }
}