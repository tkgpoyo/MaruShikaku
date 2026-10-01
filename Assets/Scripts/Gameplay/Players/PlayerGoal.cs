using System;
using System.Collections;
using MaruSikaku.Gameplay.Players.Visuals;
using UnityEngine;

namespace MaruSikaku.Gameplay.Players
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerGoal : MonoBehaviour
    {
        /// <summary>ゴールの表示を制御するインスタンス</summary>
        [SerializeField] private GoalVisual _visual;
        /// <summary>ゴール時のエフェクト</summary>
        [SerializeField] private ParticleSystem _goalEffect;

        /// <summary>ゴール判定するプレイヤー</summary>
        public PlayerController Target { get; private set; }

        /// <summary>ゴール地点にいるかどうか</summary>
        public bool IsGoal { get; private set; }

        /// <summary>ゴール到達時のイベント</summary>
        public event Action<PlayerController> OnGoalReached;
        /// <summary>ゴールから離れた時のイベント</summary>
        public event Action<PlayerController> OnGoalExited;

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsCollideWithTarget(collision)) { return; }    // 指定のプレイヤーとの接触でない場合は，処理しない
            IsGoal = true;
            _visual.LitGoal();
            OnGoalReached?.Invoke(Target);
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (!IsCollideWithTarget(collision)) { return; }
            IsGoal = false;
            _visual.UnlitGoal();
            OnGoalExited?.Invoke(Target);
        }

        public void Initialize(PlayerController player)
        {
            Target = player;
        }

        /// <summary>
        /// ゴール演出を再生します．
        /// </summary>
        /// <returns></returns>
        public IEnumerator PlayEffect()
        {
            _goalEffect.Play();             // ゴール演出の再生
            yield return null;              // 1フレーム待つ(エフェクトが再生されるのを保証)

            while (_goalEffect.isPlaying)   // エフェクトが終了するまで
            {
                yield return null;          // 1フレーム待つ
            }
        }

        /// <summary>
        /// ゴール判定するプレイヤーと接触したかどうかを判定します．
        /// </summary>
        /// <param name="collision"></param>
        /// <returns></returns>
        private bool IsCollideWithTarget(Collider2D collision)
        {
            if (Target == null ||                                               // プレイヤーが設定されていないか
                !collision.TryGetComponent<PlayerController>(out var player) || // 接触相手がPlayerControllerを持っていないか
                !ReferenceEquals(player, Target))                               // 指定のプレイヤーとの接触でない場合
            {
                return false;
            }

            return true;
        }
    }
}