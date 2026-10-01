using System.Collections;
using System.Collections.Generic;
using System.IO;
using MaruSikaku.Gameplay.Players;
using MaruSikaku.Gameplay.Players.Inputs;
using NUnit.Framework.Constraints;
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
        private PlayerGoal[] _goals;
        private HashSet<PlayerController> _goalPlayers = new();
        private bool _isGameOver;

        public IReadOnlyCollection<PlayerController> Players => _players;
        public PlayerController Current => _players[_currentIdx];

        void Start()
        {
            _isGameOver = false;

            // ステージの構築
            _loader.LoadStage(Path.Join(Application.dataPath, "Stages/stage_sample.json"), out _players, out _goals);

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
            foreach (var goal in _goals)
            {
                goal.OnGoalReached += OnGoalReached;
                goal.OnGoalExited += OnGoalExited;
            }

            _handler.OnSwitch += Switch;
            _camera.Initialize(Current);
        }

        void Update()
        {
            if (_isGameOver || _players == null) { return; }

            foreach (var player in _players)
            {
                if (_loader.StageBounds.Contains(player.transform.position)) { continue; }

                StartCoroutine(GameOver(false));
                return;
            }
        }

        private void Switch()
        {
            if (!Current.CanSwitch) { return; }

            _players[_currentIdx].SetActive(false);
            _currentIdx = (_currentIdx + 1) % _players.Length;
            _players[_currentIdx].SetActive(true);

            _camera.SetTargetPlayer(Current);
        }

        private void OnGoalReached(PlayerController player)
        {
            _goalPlayers.Add(player);

            if (_isGameOver) { return; }
            if (_goalPlayers.Count == _players.Length)  // 全プレイヤーがゴールに到達した場合
            {
                // ゴール演出を再生
                _isGameOver = true;
                StartCoroutine(GameOver(true));
            }
        }

        private void OnGoalExited(PlayerController player)
        {
            _goalPlayers.Remove(player);
        }

        private IEnumerator GameOver(bool success)
        {
            if (success)    // 成功時
            {
                var effects = new List<Coroutine>();
                // 全てのゴールに対して演出を再生
                foreach (var goal in _goals)
                {
                    effects.Add(StartCoroutine(goal.PlayEffect()));
                }
                // 全ての演出が終わるまで待機
                foreach (var effect in effects)
                {
                    yield return effect;
                }
                // TODO:ゲーム終了表示
            }
            else            // 失敗時
            {
                Debug.Log("failed");
            }
        }
    }
}