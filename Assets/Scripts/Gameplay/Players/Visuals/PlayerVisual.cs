using System;
using System.Collections.Generic;
using MaruSikaku.Gameplay.Players.Inputs;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Visuals
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class PlayerVisual : MonoBehaviour
    {
        protected const string PARAM_RUN = "Run";

        protected SpriteRenderer _render { get; private set; }
        protected Animator _anim { get; private set; }

        private Dictionary<string, bool> _animationEvents = new();

        [SerializeField] protected Transform _directionRoot;

        void Awake()
        {
            _render = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
        }

        public virtual void UpdateVisual(PlayerContext context)
        {
            UpdateFacing(context);
            UpdateAnimation(context);
        }

        private void UpdateFacing(PlayerContext context)
        {
            _directionRoot.localScale = context.FacingDirection switch
            {
                EFaceDirection.Right => new (1, _directionRoot.localScale.y, _directionRoot.localScale.z),
                EFaceDirection.Left => new (-1, _directionRoot.localScale.y, _directionRoot.localScale.z),
                _ => new (1, _directionRoot.localScale.y, _directionRoot.localScale.z)
            };
        }

        private void UpdateAnimation(PlayerContext context)
        {
            _anim.SetFloat(PARAM_RUN, Mathf.Abs(context.RigidBody.linearVelocityX));
        }

        public void PlayTrigger(string triggerName)
        {
            _anim.SetTrigger(triggerName);
        }

        public void NotifyAnimationEvent(string eventName)
        {
            _animationEvents[eventName] = true;
        }

        public bool ConsumeAnimationEvent(string eventName)
        {
            if (!_animationEvents.TryGetValue(eventName, out var fired)) { return false; }
            if (!fired) { return false; }

            _animationEvents[eventName] = false;
            return true;
        }
    }
}