using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using UnityEngine.InputSystem;
using System.Collections;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public enum EActionPhase
    {
        None,
        Anticipation,
        Action,
        Recovery
    }

    public abstract class PlayerAbility : MonoBehaviour
    {
        [SerializeField] private int _order = 0;
        public int Order => _order;

        protected PlayerContext Context { get; private set; }
        protected PlayerVisual Visual { get; private set; }
        public EActionPhase Phase { get; private set; } = EActionPhase.None;
        public bool IsRunning => Phase is not EActionPhase.None;

        private Coroutine _flow;

        public virtual void OnControlStart() { }
        public virtual void OnControlEnd() { }

        public virtual void Initialize(PlayerContext context, PlayerVisual visual)
        {
            Context = context;
            Visual = visual;
        }

        public virtual void Tick(PlayerInputData input)
        {
            if (IsRunning)
            {
                OnTickDuringAction(input);
            }
        }

        public virtual void FixedTick(PlayerInputData input)
        {
            if (IsRunning)
            {
                OnFixedTickDuringAction(input);
            }
            else if (CanStart(input))           // 物理演算などによる実行可能判定を行う可能性があるため、FixedTickの方で Start させる
            {
                _flow = StartCoroutine(ActionFlow(input));
            }
        }

        private IEnumerator ActionFlow(PlayerInputData input)
        {
            Phase = EActionPhase.Anticipation;
            OnAnticipationStart(input);
            yield return Anticipation(input);

            Phase = EActionPhase.Action;
            OnActionStart(input);
            yield return Action(input);

            Phase = EActionPhase.Recovery;
            OnRecoveryStart(input);
            yield return Recovery(input);

            Finish();
        }

        private void Finish()
        {
            Phase = EActionPhase.None;
            _flow = null;
            OnFinished();
        }

        protected void Cancel()
        {
            if (_flow != null)
            {
                StopCoroutine(_flow);
                _flow = null;
            }
            Phase = EActionPhase.None;
            OnCanceled();
        }

        protected virtual void OnTickDuringAction(PlayerInputData input) {}
        protected virtual void OnFixedTickDuringAction(PlayerInputData input) {}

        protected abstract bool CanStart(PlayerInputData input);
        protected virtual void OnAnticipationStart(PlayerInputData input) {}
        protected virtual IEnumerator Anticipation(PlayerInputData input) { yield break; }
        protected virtual void OnActionStart(PlayerInputData input) {}
        protected virtual IEnumerator Action(PlayerInputData input) { yield break; }
        protected virtual void OnRecoveryStart(PlayerInputData input) {}
        protected virtual IEnumerator Recovery(PlayerInputData input) { yield break; }
        protected virtual void OnFinished() {}
        protected virtual void OnCanceled() {}
    }
}