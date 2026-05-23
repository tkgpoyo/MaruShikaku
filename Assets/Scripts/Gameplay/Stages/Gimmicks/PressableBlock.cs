using System.Collections;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    public enum EBlockMoveDirection
    {
        Right,
        Left
    }
    [RequireComponent(typeof(Collider2D))]
    public class PressableBlock : MonoBehaviour
    {
        /// <summary>ブロックが動くはやさ</summary>
        [SerializeField] private float _speed = 1f;
        /// <summary>押す際の障害物レイヤー</summary>
        [SerializeField] private LayerMask _obstacleLayer;
        /// <summary>動かす距離</summary>
        [SerializeField] private float _moveDistance = 1f;

        private Collider2D _collider;

        void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.TryGetComponent<FragileBlock>(out var block)) { return; }     // 壊れるブロック以外は無視
            if (!IsBelowCollision(collision)) { return; }                                           // 下向きの接触でない場合は無視

            block.Break();      // ブロックを壊す
        }

        /// <summary>
        /// ブロックを動かします．
        /// </summary>
        /// <param name="dir">動かす方向</param>
        public void Move(EBlockMoveDirection dir)
        {
            var direction = dir switch
            {
                EBlockMoveDirection.Right => Vector2.right,
                EBlockMoveDirection.Left => Vector2.left,
                _ => Vector2.zero
            };

            if (!CanMove(direction, _moveDistance))
            {
                return;
            }

            var targetX = ((Vector2)transform.position + direction * _moveDistance).x;
            StartCoroutine(MoveCoroutine(targetX));
        }

        /// <summary>
        /// ブロックを動かすコルーチンです．
        /// </summary>
        /// <param name="targetX">目標X座標</param>
        /// <returns></returns>
        private IEnumerator MoveCoroutine(float targetX)
        {
            var currentPos = (Vector2)transform.position;               // 現在の位置を取得
            var targetPos = new Vector2(targetX, currentPos.y);         // 目標位置
            while (!Mathf.Approximately(currentPos.x, targetPos.x))     // 目標位置に達するまで
            {
                currentPos = Vector2.MoveTowards(
                    currentPos,
                    targetPos,
                    _speed * Time.fixedDeltaTime
                );                                                      // 現在の位置を更新
                transform.position = currentPos;                        // 位置を更新

                yield return new WaitForFixedUpdate();                  // 1フレーム待つ
            }

            transform.position = targetPos;                             // 最終位置を目標位置に揃える
        }

        /// <summary>
        /// 下向きの接触であるかどうかを判定します．
        /// </summary>
        /// <param name="collision">衝突情報</param>
        /// <returns>下向きの衝突であるかどうか</returns>
        private bool IsBelowCollision(Collision2D collision)
        {
            var contactCount = collision.contactCount;                  // 接触数
            for (var i = 0; i < contactCount; i++)
            {
                var contact = collision.GetContact(i);                  // 接触情報を取得
                if (contact.normal.y > 0.5f)                            // 法線が下向きであれば
                {
                    return true;                                        // 下で接触したとする
                }
            }
            return false;                                               // 下で接触していないとする
        }

        /// <summary>
        /// 動かせるかどうかを返します．
        /// </summary>
        /// <param name="direction">動かす方向</param>
        /// <param name="distance">動かす距離</param>
        /// <returns></returns>
        private bool CanMove(Vector2 direction, float distance)
        {
            var hits = new RaycastHit2D[8];     // ヒット情報の配列
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _obstacleLayer,
                useTriggers = false
            };                                  //  衝突判定のFilter

            var hitCount = _collider.Cast(
                direction,
                filter,
                hits,
                distance
            );                                  // 移動先の障害物の個数

            return hitCount == 0;               // 移動先に障害物がなければ動かせるとする
        }
    }
}