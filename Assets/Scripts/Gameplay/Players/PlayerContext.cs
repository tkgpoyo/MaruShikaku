using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players
{
    public enum EFaceDirection
    {
        Left,
        Right
    }

    public enum EMoveBlocker
    {
        Press,
        Tackle,
        Spring
    }

    public enum ESwitchBlocker
    {
        Jump,
        Press,
    }

    public enum EActionState
    {
        None,
        Start,
        Acting,
        End
    }

    public enum ELocomotionState
    {
        Idle,
        Run,
        Airborne,
        Landing
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerContext : MonoBehaviour
    {
        public GroundState GroundState { get; set; }
        public List<GroundContactData> GroundContactData { get; set; }
        public EActionState ActionState { get; set; } = EActionState.None;
        public ELocomotionState LocomotionState { get; set; } = ELocomotionState.Idle;
        public bool IsActive { get; set; }
        public Rigidbody2D RigidBody { get; set; }
        public EFaceDirection FacingDirection { get; set; } = EFaceDirection.Right;
        public bool CanMove => IsActive && !_moveBlocker.Any();
        public bool CanSwitch => !_switchBlocker.Any();

        private HashSet<EMoveBlocker> _moveBlocker = new();
        private HashSet<ESwitchBlocker> _switchBlocker = new();

        void Awake()
        {
            this.RigidBody = GetComponent<Rigidbody2D>();
        }

        public void AddMoveBlocker(EMoveBlocker moveBlocker)
        {
            _moveBlocker.Add(moveBlocker);
        }
        public void RemoveMoveBlocker(EMoveBlocker moveBlocker)
        {
            if (_moveBlocker.Contains(moveBlocker))
            {
                _moveBlocker.Remove(moveBlocker);
            }
        }
        public void AddSwitchBlocker(ESwitchBlocker switchBlocker)
        {
            _switchBlocker.Add(switchBlocker);
        }
        public void RemoveSwitchBlocker(ESwitchBlocker switchBlocker)
        {
            if (_switchBlocker.Contains(switchBlocker))
            {
                _switchBlocker.Remove(switchBlocker);
            }
        }
    }
}