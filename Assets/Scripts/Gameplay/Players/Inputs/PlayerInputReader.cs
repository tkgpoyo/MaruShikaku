using UnityEngine;
using UnityEngine.InputSystem;

namespace MaruSikaku.Gameplay.Players.Inputs
{
    /// <summary>
    /// プレイヤー操作に関する入力のデータを表すクラス
    /// </summary>
    public struct PlayerInputData
    {
        /// <summary>移動入力</summary>
        public Vector2 Move { get; set; }
        /// <summary>ジャンプ入力</summary>
        public bool Jump { get; set; }
        /// <summary>プレス入力</summary>
        public bool Press { get; set; }
        /// <summary>プレイヤーの切り替え入力</summary>
        public bool SwitchPlayer { get; set; }
        /// <summary>タックル入力</summary>
        public readonly bool Tackle => Jump;
        /// <summary>ダイブ入力</summary>
        public readonly bool Dive => Press;
    }

    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputReader : MonoBehaviour
    {
        private PlayerInputData _inputData;

        public void OnSwitchPlayer(InputAction.CallbackContext context)
        {
            if (!context.performed) { return; }
            var data = _inputData;
            data.SwitchPlayer = true;
            _inputData = data;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            var data = _inputData;
            data.Move = context.ReadValue<Vector2>();
            _inputData = data;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed) { return; }
            var data = _inputData;
            data.Jump = true;
            _inputData = data;
        }

        public void OnPress(InputAction.CallbackContext context)
        {
            if (!context.performed) { return; }
            var data = _inputData;
            data.Press = true;
            _inputData = data;
        }

        /// <summary>
        /// プレイヤーの操作に関する入力を取得します．
        /// </summary>
        /// <remarks>
        /// 読み出しの際に入力状態を削除するため，ある時間での入力状態を取得する際はこのメソッドを1度だけ呼び出し，複数回呼び出してはいけません．
        /// </remarks>
        /// <returns></returns>
        public PlayerInputData Consume()
        {
            var snapshot = _inputData;

            // ボタン系の入力は読み出しのたびにfalseに戻しておく
            _inputData.Jump = false;
            _inputData.Press = false;
            _inputData.SwitchPlayer = false;

            return snapshot;
        }
    }
}