using System.Collections;
using UnityEngine;
using MaruSikaku.Gameplay.Players.Abilities;
using System.Linq;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;

namespace MaruSikaku.Gameplay.Players
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerContext))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerContext _context;
        [SerializeField] private GroundChecker _grdCheck;
        [SerializeField] private PlayerVisual _visual;
        [SerializeField] private Collider2D _standable;

        public PlayerContext Context => _context;

        private PlayerAbility[] _abilities;

        void Awake()
        {
            _abilities = GetComponents<PlayerAbility>().OrderBy(ability => ability.Order).ToArray();
            foreach (var ability in _abilities)
            {
                ability.Initialize(_context);
            }
        }

        public void Tick(PlayerInputData input)
        {
            if (!_context.IsActive) { return; }
            foreach (var ability in _abilities)
            {
                ability.Tick(input);
            }
        }

        public void FixedTick(PlayerInputData input)
        {
            if (!_context.IsActive) { return; }

            UpdateGroundState();

            foreach (var ability in _abilities)
            {
                ability.FixedTick(input);
            }
        }

        public void VisualTick()
        {
            _visual.UpdateVisual(_context);
        }

        public void SetActive(bool isActive)
        {
            _context.IsActive = isActive;
            if (isActive)
            {
                _visual.Wake();
                _context.RigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
                _standable.enabled = false;
            }
            else
            {
                _visual.Sleep();
                _context.RigidBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                _standable.enabled = true;
                _context.RigidBody.linearVelocityX = 0;
            }
        }

        private void UpdateGroundState()
        {
            _context.GroundState = _grdCheck.Evaluate();
            _context.GroundContactData = _grdCheck.GetContactData();
        }
    }
}