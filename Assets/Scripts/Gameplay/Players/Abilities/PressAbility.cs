using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using System.Linq;
using System.Collections.Generic;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using System.Collections;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class PressAbility : PlayerAbility
    {
        private const float PRESS_STOP_THRE = 0.3f;
        private const string TRIG_PRESS_START = "PressStart";
        private const string TRIG_PRESS_START_END = "PressStartEnd";
        private const string TRIG_PRESS_END = "PressEnd";
        private const string TRIG_PRESS_END_END = "PressEndEnd";

        [SerializeField] private float _pressSpeed = 10f;
        [SerializeField] private float _maxShiftDistance = 1f;

        private bool _endRequested = false;

        protected override bool CanStart(PlayerInputData input)
        {
            return input.Press;
        }
        protected override void OnAnticipationStart(PlayerInputData input)
        {
            Context.LockMove(this);
        }
        protected override IEnumerator Anticipation(PlayerInputData input)
        {
            yield return new WaitForFixedUpdate();

            Context.RigidBody.linearVelocityX = 0f;

            Visual.PlayTrigger(TRIG_PRESS_START);
            while (!Visual.ConsumeAnimationEvent(TRIG_PRESS_START_END))
            {
                yield return null;
            }
        }
        protected override void OnActionStart(PlayerInputData input)
        {
            _endRequested = false;
        }
        protected override IEnumerator Action(PlayerInputData input)
        {
            yield return new WaitForFixedUpdate();                          // FixedUpdateと合わせる

            while (!_endRequested)
            {
                var groundContactList = new List<GroundContactData>();      // 地面の接触データ

                bool hitSpring = false;
                foreach (var contactData in Context.GroundContactData)
                {
                    if (contactData.Collider.TryGetComponent<FragileBlock>(out var fragileBlock))
                    {
                        fragileBlock.Break();
                    }
                    else if (contactData.Collider.TryGetComponent<Spring>(out var spring))
                    {
                        Context.RigidBody.linearVelocityY = 0f;
                        spring.Compress(Context);
                        hitSpring = true;
                        break;
                    }
                    else
                    {
                        groundContactList.Add(contactData);
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
                    // シフトができない場合、プレスを終了
                    break;
                }

                // 何も接触してない場合やシフトできた場合、等速で落下
                Context.RigidBody.linearVelocityY = -_pressSpeed;
                yield return new WaitForFixedUpdate();
            }

            Context.RigidBody.linearVelocityY = 0f;     // プレス終了時にY方向の速度を0にする
        }
        protected override IEnumerator Recovery(PlayerInputData input)
        {
            Visual.PlayTrigger(TRIG_PRESS_END);
            while (!Visual.ConsumeAnimationEvent(TRIG_PRESS_END_END))
            {
                yield return null;
            }
        }
        protected override void OnFinished()
        {
            _endRequested = false;
            Context.UnlockMove(this);
        }
        protected override void OnTickDuringAction(PlayerInputData input)
        {
            if (CanStart(input))
            {
                // プレス中にプレス入力されれば、現在のプレスを終了
                _endRequested = true;
            }
        }

        private bool TryShiftBesideGround(ICollection<GroundContactData> contactData)
        {
            var mainContact = contactData.OrderBy(data => data.Intersect.width).FirstOrDefault();
            if (mainContact == null || mainContact.Intersect.width <= 0f) { return true; }

            var playerWidth = Context.RigidBody.transform.localScale.x;
            var playerPosX = Context.RigidBody.position.x;
            float targetX;

            if (mainContact.Contact == EContactSide.Right)
            {
                targetX = mainContact.Intersect.xMin - playerWidth / 2;
            }
            else if (mainContact.Contact == EContactSide.Left)
            {
                targetX = mainContact.Intersect.xMax + playerWidth / 2;
            }
            else
            {
                return false;
            }

            if (Mathf.Abs(targetX - playerPosX) > _maxShiftDistance)
            {
                return false;
            }

            var pos = Context.RigidBody.position;
            pos.x = targetX;
            Context.RigidBody.position = pos;

            return true;
        }

    }
}