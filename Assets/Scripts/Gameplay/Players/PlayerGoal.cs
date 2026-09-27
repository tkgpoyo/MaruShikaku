using System;
using MaruSikaku.Gameplay.Players.Visuals;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerGoal : MonoBehaviour
    {
        /// <summary>ゴールの表示を制御するインスタンス</summary>
        [SerializeField] private GoalVisual _visual;

        /// <summary>ゴール判定するプレイヤー</summary>
        private PlayerController _targetPlayer;

        /// <summary>ゴール地点にいるかどうか</summary>
        public bool IsGoal { get; private set; }

        /// <summary>ゴール到達時のイベント</summary>
        public event Action OnGoalReached;
        /// <summary>ゴールから離れた時のイベント</summary>
        public event Action OnGoalExited;

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsCollideWithTarget(collision)) { return; }    // 指定のプレイヤーとの接触でない場合は，処理しない
            IsGoal = true;
            _visual.LitGoal();
            OnGoalReached?.Invoke();
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (!IsCollideWithTarget(collision)) { return; }
            IsGoal = false;
            _visual.UnlitGoal();
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