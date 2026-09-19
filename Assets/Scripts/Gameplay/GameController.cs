using System.Collections.Generic;
using System.IO;
using MaruSikaku.Gameplay.Players;
using MaruSikaku.Gameplay.Players.Inputs;
using UnityEngine;

namespace MaruSikaku.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private StageLoader _loader;
        [SerializeField] private PlayerInputHandler _handler;
        [SerializeField] private CameraChaseController _camera;

        private int _currentIdx = 0;
        private PlayerController[] _players;
        public IReadOnlyCollection<PlayerController> Players => _players;
        public PlayerController Current => _players[_currentIdx];

        void Start()
        {
            // ステージの構築
            _loader.LoadStage(Path.Join(Application.dataPath, "Stages/stage1.json"), out _players);

            // その他初期化
            for (var i = 0; i < _players.Length; i++)
            {
                if (i == _currentIdx)
                {
                    _players[i].SetInitialActive(true);
                }
                else
                {
                    _players[i].SetInitialActive(false);
                }
            }

            _handler.OnSwitch += Switch;
            _camera.Initialize(Current);
        }

        private void Switch()
        {
            if (!Current.CanSwitch) { return; }

            _players[_currentIdx].SetActive(false);
            _currentIdx = (_currentIdx + 1) % _players.Length;
            _players[_currentIdx].SetActive(true);

            _camera.SetTargetPlayer(Current);
        }
    }
}