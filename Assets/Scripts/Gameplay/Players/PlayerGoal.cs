using System;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerGoal : MonoBehaviour
    {
        /// <summary>ゴール時に点灯するランプの色</summary>
        [SerializeField] private Color _rampColor;

        /// <summary>ゴール地点にいるかどうか</summary>
        private bool _isGoal = false;
        /// <summary>ゴール判定するプレイヤー</summary>
        private PlayerController _targetPlayer;

        /// <summary>ゴール到達時のイベント</summary>
        public event Action OnGoalReached;
        /// <summary>ゴールから離れた時のイベント</summary>
        public event Action OnGoalExited;

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsCollideWithTarget(collision)) { return; }    // 指定のプレイヤーとの接触でない場合は，処理しない
            _isGoal = true;
            OnGoalReached?.Invoke();
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (!IsCollideWithTarget(collision)) { return; }
            _isGoal = false;
            OnGoalExited?.Invoke();
        }

        public void Initialize(PlayerController player)
        {
            _targetPlayer = player;
        }

        /// <summary>
        /// ゴール判定するプレイヤーと接触したかどうかを判定します．
        /// </summary>
        /// <param name="collision"></param>
        /// <returns></returns>
        private bool IsCollideWithTarget(Collider2D collision)
        {
            if (_targetPlayer == null ||                                        // プレイヤーが設定されていないか
                !collision.TryGetComponent<PlayerController>(out var player) || // 接触相手がPlayerControllerを持っていないか
                !ReferenceEquals(player, _targetPlayer))                        // 指定のプレイヤーとの接触でない場合
            {
                return false;
            }

            return true;
        }
    }
}