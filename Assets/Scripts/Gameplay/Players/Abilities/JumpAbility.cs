using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using System.Linq;
using MaruSikaku.Gameplay.Stages.Gimmicks;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class JumpAbility : PlayerAbility
    {
        private bool _jumpRequested = false;
        private bool _isJumping = false;
        private bool _hasLeftGround = false;

        [SerializeField] private float _jumpPower = 8f;
        [SerializeField] private MaruVisual _visual;

        void OnEnable()
        {
            _visual.OnJumpStartAnimEnd += Jump;
            _visual.OnJumpEndAnimEnd += JumpEnd;
        }

        void OnDisable()
        {
            _visual.OnJumpStartAnimEnd -= Jump;
            _visual.OnJumpEndAnimEnd -= JumpEnd;
        }

        public override void Tick(PlayerInputData input)
        {
            _jumpRequested |= input.Jump;       // FixedUpdateまでに1度でもジャンプボタンを押したらOK
        }

        public override void FixedTick(PlayerInputData input)
        {
            if (!Context.IsActive) { return; }

            // ジャンプしていて，着地した時
            if (_isJumping)
            {
                if (!Context.GroundState.IsGrounded)
                {
                    _hasLeftGround = true;
                    return;
                }
                if (_hasLeftGround && Context.GroundState.IsGrounded)
                {
                    _isJumping = false;
                    _hasLeftGround = false;
                    _visual.JumpEnd();
                    return;
                }
            }

            if (!_jumpRequested) { return; }
            _jumpRequested = false;

            if (!Context.GroundState.IsGrounded) { return; }

            JumpStart();
        }

        public override void OnControlStart()
        {
            _jumpRequested = false;
        }

        public override void OnControlEnd()
        {
            _jumpRequested = false;
        }

        private void JumpStart()
        {
            Context.AddSwitchBlocker(ESwitchBlocker.Jump);
            _visual.JumpStart();
        }

        private void Jump()
        {
            Context.RigidBody.linearVelocityY = 0f;
            Context.RigidBody.AddForce(Vector2.up * _jumpPower, ForceMode2D.Impulse);
            _isJumping = true;
        }

        private void JumpEnd()
        {
            Context.RemoveSwitchBlocker(ESwitchBlocker.Jump);
        }
    }
}