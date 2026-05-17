using UnityEngine;

namespace MaruSikaku.Gameplay.Trial
{
    public class VerticalMovingPlatform : MonoBehaviour
    {
        [SerializeField] private float _lowerY = 0f;
        [SerializeField] private float _upperY = 3f;
        [SerializeField] private float _speed = 1f;

        private Vector3 _startPosition;
        private float _timer;

        private void Awake()
        {
            _startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            float distance = Mathf.Abs(_upperY - _lowerY);

            if (distance <= 0.001f)
            {
                return;
            }

            _timer += Time.deltaTime * _speed / distance;

            // 0 → 1 → 0 → 1 ... を繰り返す
            float t = Mathf.PingPong(_timer, 1f);

            // 上限・下限付近で減速する
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            float y = Mathf.Lerp(_lowerY, _upperY, easedT);

            transform.position = new Vector3(
                _startPosition.x,
                y,
                _startPosition.z
            );
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            Vector3 current = transform.position;

            Vector3 lower = new Vector3(current.x, _lowerY, current.z);
            Vector3 upper = new Vector3(current.x, _upperY, current.z);

            Gizmos.DrawLine(lower, upper);
            Gizmos.DrawWireSphere(lower, 0.1f);
            Gizmos.DrawWireSphere(upper, 0.1f);
        }
#endif
    }
}