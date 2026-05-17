using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public abstract class PlayerAbility : MonoBehaviour
    {
        protected PlayerContext Context { get; private set; }
        public int Order => _order;

        [SerializeField] private int _order = 0;

        public virtual void Tick(PlayerInputData input) { }
        public virtual void FixedTick(PlayerInputData input) { }
        public virtual void OnControlStart() { }
        public virtual void OnControlEnd() { }
        public virtual void Initialize(PlayerContext context)
        {
            Context = context;
        }
    }
}