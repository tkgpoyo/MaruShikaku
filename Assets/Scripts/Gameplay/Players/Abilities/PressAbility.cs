using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using System.Linq;
using System;
using System.Collections.Generic;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using static MaruSikaku.Gameplay.Players.GroundChecker;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class PressAbility : PlayerAbility
    {
        private enum EPressState
        {
            Idle,
            StartPress,
            Pressing,
            EndPress
        }

        private const float PRESS_STOP_THRE = 0.3f;
        private const float MIN_INTERSECT_WIDTH = 0.05f;

        private EPressState _pressState = EPressState.Idle;
        private List<Spring> _springs = new();

        [SerializeField] private SikakuVisual _visual;
        [SerializeField] private float _pressSpeed = 10f;
        [SerializeField] private float _maxShiftDistance = 1f;

        void OnEnable()
        {
            _visual.OnLockMove += LockMove;
            _visual.OnPressStartAnimEnd += PressStart;
            _visual.OnPressEndAnimEnd += PressEnd;
        }

        void OnDisable()
        {
            _visual.OnPressStartAnimEnd -= PressStart;
            _visual.OnPressEndAnimEnd -= PressEnd;
        }

        public override void Tick(PlayerInputData input)
        {
            if (!input.Press) { return; }

            switch (_pressState)
            {
            case EPressState.Idle:
                _pressState = EPressState.StartPress;
                Context.AddSwitchBlocker(ESwitchBlocker.Press);
                _visual.PressStart();
                break;
            case EPressState.Pressing:
                // プレス中に入力があれば，プレス終了
                StopPress();
                break;
            }
        }

        public override void FixedTick(PlayerInputData input)
        {
            if (_pressState != EPressState.Pressing) { return; }

            var groundContactList = new List<GroundContactData>();      // 地面の接触データ

            foreach (var contactData in Context.GroundContactData)
            {
                if (contactData.Collider.TryGetComponent<FragileBlock>(out var fragileBlock))
                {
                    fragileBlock.Break();
                }
                else if (contactData.Collider.TryGetComponent<Spring>(out var spring))
                {
                    StopPress();
                    // バネにプレスした時は，バネが伸びるまで移動制限
                    Context.AddMoveBlocker(EMoveBlocker.Spring);
                    spring.Compress();
                    _springs.Add(spring);
                    spring.OnStretch += Bounce;
                }
                else
                {
                    groundContactList.Add(contactData);
                }
            }

            if (!groundContactList.Any())                 // 地面と接触していない場合
            {
                Context.RigidBody.linearVelocityY = -_pressSpeed;
                return;
            }

            if (groundContactList.Any(data => data.Contact.HasFlag(EContactSide.Left)) && 
                groundContactList.Any(data => data.Contact.HasFlag(EContactSide.Right)))
            {
                StopPress();
                return;
            }

            if (groundContactList.Sum(data => data.Overlap) >= PRESS_STOP_THRE)
            {
                StopPress();
                return;
            }

            if (!TryShiftBesideGround(groundContactList))
            {
                StopPress();
                return;
            }

            Context.RigidBody.linearVelocityY = -_pressSpeed;
        }

        private void LockMove()
        {
            Context.AddMoveBlocker(EMoveBlocker.Press);
            Context.RigidBody.linearVelocityX = 0f;
        }

        private void PressStart()
        {
            _pressState = EPressState.Pressing;
        }

        private void PressEnd()
        {
            _pressState = EPressState.Idle;
            Context.RemoveMoveBlocker(EMoveBlocker.Press);
            Context.RemoveSwitchBlocker(ESwitchBlocker.Press);
        }

        private void StopPress()
        {
            _pressState = EPressState.EndPress;
            _visual.PressEnd();
            Context.RigidBody.linearVelocityY = 0f;
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

        private void Bounce()
        {
            var stretchPower = 0f;
            foreach (var spring in _springs)
            {
                spring.OnStretch -= Bounce;
                stretchPower = Mathf.Max(stretchPower, spring.StretchPower);
            }
            Context.RemoveMoveBlocker(EMoveBlocker.Spring);
            Context.RigidBody.AddForceY(stretchPower, ForceMode2D.Impulse);
            _springs.Clear();
        }
    }
}