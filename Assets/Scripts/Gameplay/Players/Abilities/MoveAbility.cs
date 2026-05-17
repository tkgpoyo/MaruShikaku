using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using System;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class MoveAbility : PlayerAbility
    {
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 10f;

        public override void FixedTick(PlayerInputData input)
        {
            if (!Context.IsActive || !Context.CanMove) { return; }
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
    }
}