using System.Collections;
using UnityEngine;
using MaruSikaku.Gameplay.Players.Abilities;
using System.Linq;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using System.Collections.Generic;

namespace MaruSikaku.Gameplay.Players
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerContext))]
    public class PlayerController : MonoBehaviour
    {
        private const string TRIG_SLEEP = "Sleep";
        private const string TRIG_WAKE = "Wake";
        private const string TRIG_FALL = "Fall";
        private const string TRIG_LAND = "Land";
        private const string TRIG_LAND_END = "LandEnd";

        [SerializeField] private PlayerContext _context;
        [SerializeField] private GroundChecker _grdCheck;
        [SerializeField] private PlayerVisual _visual;
        [SerializeField] private Collider2D _standable;
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 10f;

        public PlayerContext Context => _context;
        public bool CanSwitch => !IsRunning && _context.GroundState.IsGrounded && !_isLandingAnimation;
        public bool IsRunning => _abilities != null && _abilities.Any(ability => ability.Phase is not EActionPhase.None);

        private PlayerAbility[] _abilities;
        private bool _isFallAnimation;
        private bool _isLandingAnimation;

        void Awake()
        {
            _abilities = GetComponents<PlayerAbility>().OrderBy(ability => ability.Order).ToArray();
            foreach (var ability in _abilities)
            {
                ability.Initialize(_context, _visual);
            }
        }

        public void Tick(PlayerInputData input)
        {
            if (!_context.IsActive) { return; }
            if (!_isLandingAnimation)
            {
                TickAbilities(input);
            }
        }

        public void FixedTick(PlayerInputData input)
        {
            if (!_context.IsActive) { return; }

            UpdateGroundState();

            if (!_isLandingAnimation)
            {
                FixedTickAbilities(input);
            }

            Move(input);
        }

        public void VisualTick()
        {
            _visual.UpdateVisual(_context);
        }

        public void SetInitialActive(bool isActive)
        {
            SetActiveCore(isActive, playWakeTrigger: false);
        }

        public void SetActive(bool isActive)
        {
            SetActiveCore(isActive, playWakeTrigger: true);
        }

        private void SetActiveCore(bool isActive, bool playWakeTrigger)
        {
            _context.IsActive = isActive;

            if (isActive)
            {
                if (playWakeTrigger)
                {
                    _visual.PlayTrigger(TRIG_WAKE);
                }

                _context.RigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
                _standable.enabled = false;
            }
            else
            {
                _visual.PlayTrigger(TRIG_SLEEP);
                _context.RigidBody.constraints =
                    RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                _standable.enabled = true;
                _context.RigidBody.linearVelocityX = 0;
            }
        }

        private void TickAbilities(PlayerInputData input)
        {
            var runningAbility = _abilities.FirstOrDefault(ability => ability.Phase is not EActionPhase.None);

            if (runningAbility != null)
            {
                runningAbility.Tick(input);
                return;
            }

            foreach (var ability in _abilities)
            {
                ability.Tick(input);

                if (ability.Phase is not EActionPhase.None)
                {
                    CancelFall();
                    break;
                }
            }
        }
        private void FixedTickAbilities(PlayerInputData input)
        {
            var runningAbility = _abilities.FirstOrDefault(ability => ability.Phase is not EActionPhase.None);

            if (runningAbility != null)
            {
                runningAbility.FixedTick(input);
                return;
            }

            foreach (var ability in _abilities)
            {
                ability.FixedTick(input);

                if (ability.Phase is not EActionPhase.None)
                {
                    CancelFall();
                    break;
                }
            }
        }
        private void UpdateGroundState()
        {
            _context.GroundState = _grdCheck.Evaluate();
            _context.GroundContactData = _grdCheck.GetContactData();

            if (!IsRunning)
            {
                // アクションがない場合は、Fallアニメーションを実行するかどうかを判定する
                if (!_context.GroundState.IsGrounded && !_isFallAnimation)
                {
                    Fall();
                    return;
                }

                if (_isFallAnimation && _context.GroundState.JustLanded)
                {
                    _isFallAnimation = false;
                    _isLandingAnimation = true;
                    StartCoroutine(Landing());
                }
            }
        }
        private void Move(PlayerInputData input)
        {
            if (!Context.CanMove) { return; }
            var targetSpeed = input.Move.x * _speed;
            var diffSpeed = targetSpeed - Context.RigidBody.linearVelocityX;
            var accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _acceleration : _deceleration;
            var movement = diffSpeed * accelRate;
            Context.RigidBody.AddForce(Vector2.right * movement);

            if (Context.RigidBody.linearVelocityX > _speed)     // 最大速度を上回る場合
            {
                Context.RigidBody.linearVelocityX = _speed;
            }

            // 向きの更新
            if (Mathf.Abs(input.Move.x) > 0.01f)
            {
                Context.FacingDirection = input.Move.x > 0 ? EFaceDirection.Right : EFaceDirection.Left;
            }
        }
        private void Fall()
        {
            _isFallAnimation = true;
            _visual.PlayTrigger(TRIG_FALL);
        }
        private void CancelFall()
        {
            _isFallAnimation = false;
        }
        private IEnumerator Landing()
        {
            _isLandingAnimation = true;

            _visual.PlayTrigger(TRIG_LAND);

            while (!_visual.ConsumeAnimationEvent(TRIG_LAND_END))
            {
                yield return null;
            }

            _isLandingAnimation = false;
        }
    }
}