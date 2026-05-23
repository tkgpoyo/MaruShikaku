using System.Collections;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    /// <summary>
    /// タックル能力を表すクラス
    /// </summary>
    public class TackleAbility : PlayerAbility
    {
        /// <summary>タックル開始アニメーションのトリガー名</summary>
        private const string TRIG_TACKLE_START = "TackleStart";
        /// <summary>タックル開始アニメーションの終了時イベント名</summary>
        private const string TACKLE_START_END_EVENT = "TackleStartEnd";
        /// <summary>タックル成功時における、タックル終了アニメーションのトリガー名</summary>
        private const string TRIG_TACKLE_SUCCESS_END = "TackleSuccessEnd";
        /// <summary>タックル成功時における、タックル終了アニメーションの終了時イベント名</summary>
        private const string TACKLE_SUCCESS_END_EVENT = "TackleSuccessEndEnd";
        /// <summary>タックル失敗時における、タックル終了アニメーションのトリガー名</summary>
        private const string TRIG_TACKLE_FAIL_END = "TackleFailEnd";
        /// <summary>タックル失敗時における、タックル終了アニメーションの終了時イベント名</summary>
        private const string TACKLE_FAIL_END_EVENT = "TackleFailEndEnd";

        /// <summary>タックルの当たり判定の中心</summary>
        [SerializeField] private Transform _tackleCheck;
        /// <summary>タックルの当たり判定のBoxサイズ</summary>
        [SerializeField] private Vector2 _tackleCheckSize = new (0.4f, 1.0f);

        /// <summary>タックル時に接触した、壊れるブロック</summary>
        private FragileBlock _fragileBlock;
        /// <summary>タックル時に接触した，動かせるブロック</summary>
        private PressableBlock _pressableBlock;
        /// <summary>タックルに成功したかどうか</summary>
        private bool _isSuccess;

        /// <inheritdoc/>
        protected override bool CanStart(PlayerInputData input)
        {
            return input.Tackle && Context.GroundState.IsGrounded;          // タックル入力があるかつ地面に接しているか
        }
        /// <inheritdoc/>
        protected override void OnAnticipationStart(PlayerInputData input)
        {
            _isSuccess = false;                                             // タックルに成功していないとする
            Context.LockMove(this);                                         // タックル中は移動禁止にする
            Context.RigidBody.linearVelocityX = 0f;                         // X方向の速さを0にする

            // 周囲のオブジェクトは動かないと仮定し、能力開始動作時にすでにタックル成功かどうかを判定する
            var blocks = Physics2D.OverlapBoxAll(_tackleCheck.position, _tackleCheckSize, 0f);

            _fragileBlock = null;                                                   // タックルによって接触した、壊せるブロック
            _pressableBlock = null;                                                 // タックルによって接触した、動かせるブロック
            foreach (var block in blocks)                                           // 接触したブロックについて
            {
                if (block.TryGetComponent<FragileBlock>(out var b))                 // 壊せるブロックの場合
                {
                    _fragileBlock = b;                                              // そのブロックを取得
                    break;                                                          // ループを抜ける
                }
                if (block.TryGetComponent<PressableBlock>(out var pressableBlock))  // 動かせるブロックの場合
                {
                    _pressableBlock = pressableBlock;                               // そのブロックを取得
                    break;                                                          // ループを抜ける
                }
            }
        }
        /// <inheritdoc/>
        protected override IEnumerator Anticipation(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_TACKLE_START);                          // タックル開始アニメーションを開始
            while (!Visual.ConsumeAnimationEvent(TACKLE_START_END_EVENT))   // タックル開始アニメーションが終了するまで
            {
                yield return null;                                          // 待機
            }
        }
        /// <inheritdoc/>
        protected override IEnumerator Action(PlayerInputData input)
        {
            if (_fragileBlock != null)                                      // 壊せるブロックに接触していた場合
            {
                _isSuccess = true;                                          // タックル成功
                _fragileBlock.Break();                                      // ブロックを壊す
            }
            if (_pressableBlock != null)                                    // 動かせるブロックに接触していた場合
            {
                _isSuccess = true;                                          // タックル成功
                switch (Context.FacingDirection)                            // 向いている方向に応じてブロックの動かす向きを変える
                {
                case EFaceDirection.Right:
                    _pressableBlock.Move(EBlockMoveDirection.Right);        // 右に動かす
                    break;
                case EFaceDirection.Left:
                    _pressableBlock.Move(EBlockMoveDirection.Left);         // 左に動かす
                    break;
                }
            }
            yield break;
        }
        /// <inheritdoc/>
        protected override IEnumerator Recovery(PlayerInputData input)
        {
            if (_isSuccess)                                                         // タックル成功の場合
            {
                // 成功アニメーションを流す
                Visual.PlayTrigger(TRIG_TACKLE_SUCCESS_END);
                while (!Visual.ConsumeAnimationEvent(TACKLE_SUCCESS_END_EVENT))
                {
                    yield return null;
                }
            }
            else
            {
                // 失敗アニメーションを流す
                Visual.PlayTrigger(TRIG_TACKLE_FAIL_END);
                while (!Visual.ConsumeAnimationEvent(TACKLE_FAIL_END_EVENT))
                {
                    yield return null;
                }
            }
        }
        /// <inheritdoc/>
        protected override void OnFinished()
        {
            Context.UnlockMove(this);       // タックル終了時に移動制限を解除
        }
        /// <inheritdoc/>
        protected override void OnCanceled()
        {
            Context.UnlockMove(this);       // キャンセル時にも、移動制限を解除
        }

    #if UNITY_EDITOR
        /// <summary>
        /// タックルの当たり判定を可視化します。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_tackleCheck == null) { return; }
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_tackleCheck.position, _tackleCheckSize);
        }
    #endif
    }
}