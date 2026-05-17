using System.Collections.Generic;
using MaruSikaku.Gameplay.Players;
using MaruSikaku.Gameplay.Players.Inputs;
using UnityEngine;

namespace MaruSikaku.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private PlayerController[] _players;
        [SerializeField] private PlayerInputHandler _handler;
        [SerializeField] private CameraChaseController _camera;

        private int _currentIdx = 0;
        public IReadOnlyCollection<PlayerController> Players => _players;
        public PlayerController Current => _players[_currentIdx];

        void Start()
        {
            foreach (var player in _players)
            {
                player.SetActive(false);
            }
            _players[_currentIdx].SetActive(true);

            _handler.OnSwitch += Switch;
            _camera.Initialize(Current);
        }

        private void Switch()
        {
            if (!Current.Context.CanSwitch) { return; }

            _players[_currentIdx].SetActive(false);
            _currentIdx = (_currentIdx + 1) % _players.Length;
            _players[_currentIdx].SetActive(true);

            _camera.SetTargetPlayer(Current);
        }
    }
}