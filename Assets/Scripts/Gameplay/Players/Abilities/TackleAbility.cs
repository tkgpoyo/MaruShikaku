using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    public class TackleAbility : PlayerAbility
    {
        protected override bool CanStart(PlayerInputData input)
        {
            return input.Tackle && Context.GroundState.IsGrounded;
        }
    }
}