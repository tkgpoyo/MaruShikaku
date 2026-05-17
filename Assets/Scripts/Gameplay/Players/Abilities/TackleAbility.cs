using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class TackleAbility : PlayerAbility
    {
        [SerializeField] private SikakuVisual _visual;
        [SerializeField] private Collider2D _tackleCollider;

        public override void FixedTick(PlayerInputData input)
        {
        }
        public override void OnControlStart()
        {
            _tackleCollider.enabled = false;
        }
        public override void OnControlEnd()
        {
            _tackleCollider.enabled = true;
        }
    }
}