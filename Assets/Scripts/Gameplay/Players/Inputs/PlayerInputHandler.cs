using System;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Inputs
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerInputData _fixedInput;

        [SerializeField] private PlayerInputReader _reader;
        [SerializeField] private GameController _game;

        public event Action OnSwitch;

        void Update()
        {
            var input = _reader.Consume();

            // FixedUpdate用の入力
            _fixedInput.Move = input.Move;      // 上書き
            _fixedInput.Jump |= input.Jump;     // 次のFixedUpdateまでに一回でも入力されていればOK
            _fixedInput.Press |= input.Press;   // 次のFixedUpdateまでに一回でも入力されていればOK

            if (input.Switch)
            {
                OnSwitch?.Invoke();

                input.Jump = false;
                input.Press = false;

                ResetFixedInput();
            }

            var current = _game.Current;
            if (current != null)
            {
                current.Tick(input);
            }

            // 見た目系の更新は全てのプレイヤーに対して
            foreach (var player in _game.Players)
            {
                player.VisualTick();
            }
        }

        void FixedUpdate()
        {
            var current = _game.Current;
            if (current == null) { return; }
            current.FixedTick(_fixedInput);

            ResetFixedInput();
        }

        private void ResetFixedInput()
        {
            _fixedInput.Jump = false;
            _fixedInput.Press = false;
        }
    }
}