using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players
{
    public readonly struct GroundState
    {
        public bool IsGrounded { get; }
        public bool JustLanded { get; }
        public bool JustLeftGround { get; }

        public GroundState(bool isGrounded, bool justLanded, bool justLeftGround)
        {
            IsGrounded = isGrounded;
            JustLanded = justLanded;
            JustLeftGround = justLeftGround;
        }
    }

    [Flags]
    public enum EContactSide
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1
    }

    public class GroundContactData
    {
        public EContactSide Contact { get; set; }
        public float Overlap { get; set; }
        public Rect Intersect { get; set; }
        public Collider2D Collider { get; set; }
    }
    public class GroundChecker : MonoBehaviour
    {
        private bool _wasGround = false;

        [Header("Ground Check"), Tooltip("地面との衝突関連のパラメータ")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Vector2 _groundCheckSize = new (0.6f, 1.2f);
        [SerializeField] private LayerMask _groundLayer;

        public GroundState Evaluate()
        {
            var isGround = Physics2D.OverlapBox(
                _groundCheck.position,
                _groundCheckSize,
                0f,
                _groundLayer
            );

            var justLanded = isGround && !_wasGround;
            var justLeftGround = !isGround && _wasGround;

            return new GroundState(isGround, justLanded, justLeftGround);
        }

        private Rect GetIntersectRect(Rect sensorBound, Rect targetBound)
        {
            var left = Mathf.Max(sensorBound.min.x, targetBound.min.x);
            var right = Mathf.Min(sensorBound.max.x, targetBound.max.x);
            return new Rect(new (left, sensorBound.min.y), new (Mathf.Max(0f, right - left), sensorBound.size.y));
        }

        private Rect BoundsToRect(Bounds bounds) => new Rect(bounds.center - bounds.size * 0.5f, bounds.size);

        public List<GroundContactData> GetContactData()
        {
            var grounds = Physics2D.OverlapBoxAll(_groundCheck.position, _groundCheckSize, 0f, _groundLayer);
            var sensorBound = new Bounds(_groundCheck.position, _groundCheckSize);

            var groundContactDataList = new List<GroundContactData>();

            foreach (var ground in grounds)
            {
                var side = EContactSide.None;
                var intersectRect = GetIntersectRect(BoundsToRect(sensorBound), BoundsToRect(ground.bounds));
                var groundOverlapRatio = intersectRect.width / sensorBound.size.x;

                if (intersectRect.xMin < _groundCheck.position.x) { side |= EContactSide.Left; }
                if (intersectRect.xMax > _groundCheck.position.x) { side |= EContactSide.Right; }

                groundContactDataList.Add(new() { Contact = side, Intersect = intersectRect, Overlap = groundOverlapRatio, Collider = ground });
            }
            return groundContactDataList;
        }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null) { return; }
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundCheck.position, _groundCheckSize);
        }
    #endif
    }
}