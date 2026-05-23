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
        /// <summary>ブロック前方に障害物があるかどうかを確認する際の余白</summary>
        private const float CHECK_MARGIN = 0.1f;

        /// <summary>ブロックが動くはやさ</summary>
        [SerializeField] private float _speed = 1f;
        /// <summary>押す際の障害物レイヤー</summary>
        [SerializeField] private LayerMask _obstacleLayer;
        /// <summary>動かす距離</summary>
        [SerializeField] private float _moveDistance = 1f;

        private Collider2D _collider;
        private bool _isMoving = false;

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
            if (_isMoving) { return; }

            var direction = dir switch
            {
                EBlockMoveDirection.Right => Vector2.right,
                EBlockMoveDirection.Left => Vector2.left,
                _ => Vector2.zero
            };

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
            _isMoving = true;

            var currentPos = (Vector2)transform.position;               // 現在の位置を取得
            var targetPos = new Vector2(targetX, currentPos.y);         // 目標位置
            while (!Mathf.Approximately(currentPos.x, targetPos.x))     // 目標位置に達するまで
            {
                var nextPos = Vector2.MoveTowards(
                    currentPos,
                    targetPos,
                    _speed * Time.fixedDeltaTime
                );                                                      // 現在の位置を更新
                var moveVec = nextPos - currentPos;                     // 移動方向
                if (!CanMove(moveVec.normalized, moveVec.magnitude))    // 動かせない場合
                {
                    break;                                              // 移動を終了
                }
                currentPos = nextPos;
                transform.position = currentPos;                        // 位置を更新

                yield return new WaitForFixedUpdate();                  // 1フレーム待つ
            }

            transform.position = currentPos;                            // 最終位置を目標位置に揃える
            _isMoving = false;
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
                if (contact.normal.y > 0.5f)                            // 法線が上向きであれば
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
        /// <returns>動かせるかどうか</returns>
        private bool CanMove(Vector2 direction, float distance)
        {
            var bounds = _collider.bounds;
            var size = new Vector2(CHECK_MARGIN, bounds.size.y * 0.8f);

            var originX = direction.x > 0 ? 
                bounds.max.x + CHECK_MARGIN * 0.5f :
                bounds.min.x - CHECK_MARGIN * 0.5f;     // 前方の障害物判定の当たり判定Boxの中心X座標
            var origin = new Vector2(originX, bounds.center.y);

            var hits = Physics2D.BoxCastAll(
                origin,
                size,
                0f,
                direction,
                distance,
                _obstacleLayer
            );

            foreach (var hit in hits)
            {
                if (hit.collider == null) { continue; }
                if (hit.collider == _collider) { continue; }
                if (hit.collider.transform.IsChildOf(transform)) { continue; }

                return false;
            }

            return true;
        }
    }
}