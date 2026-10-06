using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    public enum EBlockMoveDirection
    {
        Right,
        Left
    }

    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
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
        [SerializeField] private Vector2Int _size = Vector2Int.one;

        private BoxCollider2D _collider;
        private bool _isMoving = false;
        private HashSet<Collider2D> _groundColliders = new();
        private bool _isGroundBelow => _groundColliders.Count > 0;
        private SpriteRenderer _renderer;

        public Vector2Int Size
        {
            get => _size;
            set
            {
                _size = value;
                SetSize(_size);
            }
        }

        void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        void OnDisable()
        {
            StopAllCoroutines();
            _isMoving = false;
            _groundColliders.Clear();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // 下に地面があるかどうか確認
            if (collision.gameObject.layer == LayerMask.NameToLayer(MaruSikakuConsts.GROUND_LAYER_NAME) && 
                IsBelowCollision(collision))
            {
                _groundColliders.Add(collision.collider);
            }

            if (!collision.gameObject.TryGetComponent<FragileBlock>(out var block)) { return; }     // 壊れるブロック以外は無視
            if (!IsBelowCollision(collision)) { return; }                                           // 下向きの接触でない場合は無視

            block.Break();      // ブロックを壊す
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(MaruSikakuConsts.GROUND_LAYER_NAME))
            {
                if (IsBelowCollision(collision))
                {
                    _groundColliders.Add(collision.collider);
                }
                else
                {
                    _groundColliders.Remove(collision.collider);
                }
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            _groundColliders.Remove(collision.collider);
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

            while (_isGroundBelow)                                      // 地面から離れるまで
            {
                var currentPos = transform.position;
                var nextX = Mathf.MoveTowards(
                    currentPos.x,
                    targetX,
                    _speed * Time.fixedDeltaTime
                );                                                      // 次のX座標
                var moveVec = new Vector2(nextX - currentPos.x, 0f);    // 移動方向
                if (!CanMove(moveVec.normalized, moveVec.magnitude))    // 動かせない場合
                {
                    break;                                              // 移動を終了
                }
                currentPos.x = nextX;                                   // X座標を更新

                if (Mathf.Approximately(currentPos.x, targetX))         // 目標に到達したら
                {
                    break;                                              // 処理を終了
                }

                transform.position = currentPos;                        // 位置を更新

                yield return new WaitForFixedUpdate();                  // 1フレーム待つ
            }

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

        private void SetSize(Vector2Int size)
        {
            const float DELTA = 0.005f;
#if UNITY_EDITOR
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }
            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider2D>();
            }
#endif
            _renderer.size = new(size.x, size.y);
            _collider.size = new(size.x - DELTA, size.y - DELTA);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // スケールによってシーン上の大きさを変更
                EditorApplication.delayCall += () =>
                {
                    if (this == null) { return; }
                    SetSize(_size);
                };
            }
        }
#endif
    }
}