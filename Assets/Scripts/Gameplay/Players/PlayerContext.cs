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

    public enum EActionState
    {
        None,
        Start,
        Acting,
        End
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerContext : MonoBehaviour
    {
        public GroundState GroundState { get; set; }
        public List<GroundContactData> GroundContactData { get; set; }

        public bool IsActive { get; set; }
        public Rigidbody2D RigidBody { get; set; }
        public EFaceDirection FacingDirection { get; set; } = EFaceDirection.Right;
        public bool CanMove => IsActive && !_moveLocks.Any();

        private HashSet<object> _moveLocks = new();

        void Awake()
        {
            this.RigidBody = GetComponent<Rigidbody2D>();
        }

        public void LockMove(object owner)
        {
            _moveLocks.Add(owner);
        }

        public void UnlockMove(object owner)
        {
            _moveLocks.Remove(owner);
        }
    }
}