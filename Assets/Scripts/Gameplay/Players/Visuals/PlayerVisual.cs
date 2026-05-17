using MaruSikaku.Gameplay.Players.Inputs;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Visuals
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public abstract class PlayerVisual : MonoBehaviour
    {
        protected const string PARAM_RUN = "Run";
        protected const string PARAM_FALL = "Fall";
        protected const string TRIG_SLEEP = "Sleep";
        protected const string TRIG_WAKE = "Wake";

        protected SpriteRenderer _render { get; private set; }
        protected Animator _anim { get; private set; }

        [SerializeField] protected Transform _directionRoot;

        void Awake()
        {
            _render = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
        }

        public virtual void UpdateVisual(PlayerContext context)
        {
            UpdateFacing(context);
            UpdateCommonAnimation(context);
            UpdateAnimation(context);
        }

        protected abstract void UpdateAnimation(PlayerContext context);

        private void UpdateFacing(PlayerContext context)
        {
            _directionRoot.localScale = context.FacingDirection switch
            {
                EFaceDirection.Right => new (1, _directionRoot.localScale.y, _directionRoot.localScale.z),
                EFaceDirection.Left => new (-1, _directionRoot.localScale.y, _directionRoot.localScale.z),
                _ => new (1, _directionRoot.localScale.y, _directionRoot.localScale.z)
            };
        }

        private void UpdateCommonAnimation(PlayerContext context)
        {
            _anim.SetBool(PARAM_FALL, !context.GroundState.IsGrounded);
            _anim.SetFloat(PARAM_RUN, Mathf.Abs(context.RigidBody.linearVelocityX));
        }

        public void Sleep()
        {
            _anim.SetTrigger(TRIG_SLEEP);
        }

        public void Wake()
        {
            _anim.SetTrigger(TRIG_WAKE);
        }
    }
}