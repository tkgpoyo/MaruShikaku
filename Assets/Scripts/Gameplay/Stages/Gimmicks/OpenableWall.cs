using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    public class OpenableWall : MonoBehaviour
    {
        /// <summary>1秒でどのくらい壁が動くか</summary>
        [SerializeField, Tooltip("1秒あたりのY方向変化")] private float _speed = 1f;
        /// <summary>開ききった時のY方向の余白</summary>
        [SerializeField] private float _openMargin = 0.01f;
        /// <summary>閉まり切った時のY方向の余白</summary>
        [SerializeField] private float _closeMargin = 0.01f;
        /// <summary>Y方向の長さ</summary>
        [SerializeField] private float _yLength = 1f;
        /// <summary>下降を防ぐオブジェクトのレイヤー</summary>
        [SerializeField] private LayerMask _blockLayer;
        /// <summary>下端からどの程度までを下からの接触とみなすか</summary>
        [SerializeField]
        private float _bottomContactTolerance = 0.05f;
        /// <summary>壁のSprite Renderer</summary>
        [SerializeField]
        private SpriteRenderer _renderer;
        /// <summary>壁のBoxCollider</summary>
        [SerializeField]
        private BoxCollider2D _collider;

        private Coroutine _coroutine;
        private HashSet<Collider2D> _blockingColliders = new();

        void OnCollisionEnter2D(Collision2D collision)
        {
            UpdateCollisionState(collision);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            UpdateCollisionState(collision);
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (_blockingColliders.Contains(collision.collider))    // 下降を防いでいたcolliderの場合
            {
                _blockingColliders.Remove(collision.collider);      // 削除
            }
        }

        /// <summary>
        /// 壁を開きます．
        /// </summary>
        public void Open()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _coroutine = StartCoroutine(RunCoroutine(true));
        }

        /// <summary>
        /// 壁を閉じます．
        /// </summary>
        public void Close()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _coroutine = StartCoroutine(RunCoroutine(false));
        }

        /// <summary>
        /// 壁の開閉を処理するコルーチンです．
        /// </summary>
        /// <param name="isOpen">開く方向か</param>
        /// <returns></returns>
        private IEnumerator RunCoroutine(bool isOpen)
        {
            // Y方向のスケール変化によって壁の開閉を表す
            var currentHeight = _collider.size.y;                       // 現在のスケール
            var targetHeight = isOpen ? 0 : _yLength;                   // 目標のスケール
            targetHeight += isOpen ? _openMargin : -_closeMargin;       // 少しだけ天井や床と余白を設ける
            yield return new WaitForFixedUpdate();                      // FixedUpdateに同期させる
            while (!Mathf.Approximately(currentHeight, targetHeight))   // 現在のY方向スケールが目標に達するまで
            {
                if (!isOpen && _blockingColliders.Count > 0) {          // しまっている途中で，下降を防ぐColliderがある場合
                    yield return new WaitForFixedUpdate();              // 1フレーム待って
                    continue;                                           // ループの先頭に戻る
                }

                currentHeight = Mathf.MoveTowards(
                    currentHeight,
                    targetHeight,
                    _speed * Time.fixedDeltaTime
                );                                                      // 高さを更新

                SetHeight(currentHeight);                               // 高さを設定

                yield return new WaitForFixedUpdate();                  // FixedUpdateに同期
            }

            SetHeight(targetHeight);                                    // 高さを設定
        }

        /// <summary>
        /// 壁との接触状態を更新します．
        /// </summary>
        /// <param name="collision">衝突情報</param>
        private void UpdateCollisionState(Collision2D collision)
        {
            if (collision.collider == null)                     // 接触情報がない場合 
            {
                return;
            }
            if (!IsInBlockLayer(collision.gameObject))          // 壁の動きを止めるオブジェクトでない場合
            {
                return;
            }
            if (IsBelowCollision(collision))                    // 下からの接触である場合
            {
                _blockingColliders.Add(collision.collider);     // 壁を防ぐColliderリストに追加
            }
            else                                                // 下からの接触でない場合
            {
                _blockingColliders.Remove(collision.collider);  // 壁を防ぐColliderリストから削除
            }
        }

        /// <summary>
        /// 衝突が下からの衝突であるかどうかを判定します．
        /// </summary>
        /// <param name="collision">衝突情報</param>
        /// <returns>下からの衝突であるかどうか</returns>
        private bool IsBelowCollision(Collision2D collision)
        {
            var contactCount = collision.contactCount;                      // 接触数
            var bottomY = _collider.bounds.min.y;                           // 壁Colliderの下のY座標

            for (var i = 0; i < contactCount; i++)
            {
                var contact = collision.GetContact(i);                      // 接触情報を取得
                if (contact.point.y <= bottomY + _bottomContactTolerance)   // 接触位置が壁の下の場合
                {
                    return true;                                            // 下に接触しているとする
                }
            }
            return false;                                                   // 下に接触していないとする
        }

        /// <summary>
        /// 壁の下降を止めるオブジェクトかどうかを判定します．
        /// </summary>
        /// <param name="obj">接触しているオブジェクト</param>
        /// <returns>壁の下降を止めるかどうか</returns>
        private bool IsInBlockLayer(GameObject obj)
        {
            return (_blockLayer.value & (1 << obj.layer)) != 0;     // レイヤーによって判定
        }

        /// <summary>
        /// 壁の高さを設定します．
        /// </summary>
        /// <param name="height">高さ</param>
        private void SetHeight(float height)
        {
            if (_renderer == null) { return; }  // SpriteRendererがない場合は抜け出す
            if (_collider == null) { return; }  // Colliderがない場合は抜け出す

            // 最初に壁の見た目を変更
            var currentSize = _renderer.size;   // 現在のサイズを取得
            currentSize.y = height;             // Y方向スケールのみを変化
            _renderer.size = currentSize;       // サイズを変更

            // 次にColliderのoffsetとサイズを変更
            var colliderOffset = _collider.offset;  // Offsetを取得
            colliderOffset.y = -currentSize.y / 2;  // anchorがTop Centerなspriteに合わせるように調整
            _collider.offset = colliderOffset;      // Offset変更を適用
            var colliderSize = _collider.size;      // BoxColliderのサイズを取得
            colliderSize.y = height;                // Y方向のスケールを変化
            _collider.size = colliderSize;          // サイズ変更を適用
        }

#if UNITY_EDITOR
        private bool _isValidateQueued;

        private void OnValidate()
        {
            if (!Application.isPlaying && !_isValidateQueued)
            {
                _isValidateQueued = true;

                EditorApplication.delayCall += () =>
                {
                    _isValidateQueued = false;

                    if (this == null) return;
                    SetHeight(_yLength);
                };
            }
        }
#endif
    }
}