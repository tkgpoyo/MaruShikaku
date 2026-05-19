using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using System.Linq;
using System.Collections.Generic;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using System.Collections;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    /// <summary>
    /// プレス能力を表すクラス
    /// </summary>
    public class PressAbility : PlayerAbility
    {
        /// <summary>プレス終了判定における、地面との接触割合の閾値</summary>
        private const float PRESS_STOP_THRE = 0.3f;
        /// <summary>プレス開始アニメーションのトリガー名</summary>
        private const string TRIG_PRESS_START = "PressStart";
        /// <summary>プレス開始アニメーションの終了時イベント名</summary>
        private const string PRESS_START_END_EVENT = "PressStartEnd";
        /// <summary>プレス終了アニメーションのトリガー名</summary>
        private const string TRIG_PRESS_END = "PressEnd";
        /// <summary>プレス終了アニメーションの終了時イベント名</summary>
        private const string PRESS_END_END_EVENT = "PressEndEnd";

        /// <summary>プレスの垂直方向の速さ</summary>
        [SerializeField] private float _pressSpeed = 10f;
        /// <summary>プレス時の最大シフト距離</summary>
        [SerializeField] private float _maxShiftDistance = 1f;

        /// <summary>プレス終了要求</summary>
        private bool _endRequested = false;

        /// <inheritdoc/>
        protected override bool CanStart(PlayerInputData input)
        {
            return input.Press;     // プレス入力があるか
        }
        /// <inheritdoc/>
        protected override void OnAnticipationStart(PlayerInputData input)
        {
            Context.LockMove(this); // プレス中は移動禁止にする
        }
        /// <inheritdoc/>
        protected override IEnumerator Anticipation(PlayerInputData input)
        {
            yield return new WaitForFixedUpdate();                          // 物理演算を行うため、FixedUpdateに同期させる

            Context.RigidBody.linearVelocityX = 0f;                         // 垂直方向の速さを0にする

            Visual.PlayTrigger(TRIG_PRESS_START);                           // プレス開始アニメーションの開始
            while (!Visual.ConsumeAnimationEvent(PRESS_START_END_EVENT))    // プレス開始アニメーションが終了するまで
            {
                yield return null;                                          // 待機
            }
        }
        /// <inheritdoc/>
        protected override void OnActionStart(PlayerInputData input)
        {
            _endRequested = false;                                          // プレス終了要求がない状態でアクション開始
        }
        /// <inheritdoc/>
        protected override IEnumerator Action(PlayerInputData input)
        {
            yield return new WaitForFixedUpdate();                          // FixedUpdateと合わせる

            while (!_endRequested)                                          // プレス終了要求があるまで
            {
                var groundContactList = new List<GroundContactData>();      // 地面との接触データ

                bool hitSpring = false;                                     // バネに当たったかどうか
                foreach (var contactData in Context.GroundContactData)      // 接触しているすべての物体に対して
                {
                    if (contactData.Collider.TryGetComponent<FragileBlock>(out var fragileBlock))   // 壊れるブロックに当たった場合
                    {
                        fragileBlock.Break();                               // 壊す
                    }
                    else if (contactData.Collider.TryGetComponent<Spring>(out var spring))          // バネに当たった場合
                    {
                        Context.RigidBody.linearVelocityY = 0f;             // 垂直方向の速さを0にする
                        spring.Compress(Context);                           // バネの反発を開始
                        hitSpring = true;                                   // バネに接触したとする
                        break;
                    }
                    else                                                                            // 上記以外と接触した場合
                    {
                        groundContactList.Add(contactData);                 // 地面との接触データに加える
                    }
                }

                if (hitSpring)
                {
                    // バネに当たったらプレス終了
                    break;
                }

                if (groundContactList.Any(data => data.Contact.HasFlag(EContactSide.Left)) && 
                    groundContactList.Any(data => data.Contact.HasFlag(EContactSide.Right)))
                {
                    // 両側が接している場合、プレスを終了
                    break;
                }

                if (groundContactList.Sum(data => data.Overlap) >= PRESS_STOP_THRE)
                {
                    // 大部分が地面と接触している場合、プレスを終了
                    break;
                }

                if (!TryShiftBesideGround(groundContactList))
                {
                    // 左右へのシフトができない場合、プレスを終了
                    break;
                }

                // 何も接触してない場合やシフトできた場合、等速で落下
                Context.RigidBody.linearVelocityY = -_pressSpeed;
                yield return new WaitForFixedUpdate();  // ループ内で物理演算を多用するため、FixedUpdateに同期
            }

            Context.RigidBody.linearVelocityY = 0f;     // プレス終了時にY方向の速度を0にする
        }
        /// <inheritdoc/>
        protected override IEnumerator Recovery(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_PRESS_END);                             // プレス終了アニメーションの開始
            while (!Visual.ConsumeAnimationEvent(PRESS_END_END_EVENT))      // プレス終了アニメーションが終了するまで
            {
                yield return null;                                          // 待機
            }
        }
        /// <inheritdoc/>
        protected override void OnFinished()
        {
            _endRequested = false;          // プレス終了要求をなしにする
            Context.UnlockMove(this);       // プレイヤーの移動を許可する
        }
        /// <inheritdoc/>
        protected override void OnTickDuringAbility(PlayerInputData input)
        {
            if (CanStart(input))
            {
                // プレス中にプレス入力されれば、現在のプレスを終了
                _endRequested = true;
            }
        }

        /// <summary>
        /// プレス時に少しだけ地面と接触している場合に、プレイヤの位置を少しX方向にシフトさせて、地面との接触を避ける処理です。
        /// </summary>
        /// <param name="contactData">地面との接触データ</param>
        /// <returns>地面との接触を避けるようなシフトができたかどうか</returns>
        private bool TryShiftBesideGround(ICollection<GroundContactData> contactData)
        {
            var mainContact = contactData.OrderBy(data => data.Intersect.width).FirstOrDefault();   // 最も接触している地面データを取得
            if (mainContact == null || mainContact.Intersect.width <= 0f) { return true; }          // 接触している地面データがない場合や、十分な接触幅がない場合、シフトさせる必要がない

            var playerWidth = Context.RigidBody.transform.localScale.x;     // プレイヤーの横幅
            var playerPosX = Context.RigidBody.position.x;                  // プレイヤーのX座標
            float targetX;                                                  // シフト後のプレイヤーのX座標

            if (mainContact.Contact == EContactSide.Right)                  // 右側と接触している場合
            {
                targetX = mainContact.Intersect.xMin - playerWidth / 2;     // 地面の右にシフトさせる
            }
            else if (mainContact.Contact == EContactSide.Left)              // 左側と接触している場合
            {
                targetX = mainContact.Intersect.xMax + playerWidth / 2;     // 地面の左にシフトさせる
            }
            else                                                            // プレイヤーの両側に接触している場合
            {
                return false;                                               // 接触を避けるようなシフトできない
            }

            if (Mathf.Abs(targetX - playerPosX) > _maxShiftDistance)        // シフト距離が大きすぎる場合
            {
                return false;                                               // 接触を避けるようなシフトできない
            }

            // プレイヤーをtargetXのX座標へシフトさせる
            var pos = Context.RigidBody.position;
            pos.x = targetX;
            Context.RigidBody.position = pos;

            return true;                                                    // シフトできた
        }

    }
}