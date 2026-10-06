using UnityEngine;

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    [RequireComponent(typeof(Collider2D))]
    public class FragileBlock : MonoBehaviour
    {
        [SerializeField] private GameObject _breakEffectPrefab;
        [SerializeField] private float _effectDestroyDelay = 1.0f;
        
        private Collider2D _collider;

        void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        public void Break()
        {
            if (!_collider.enabled) { return; }
            _collider.enabled = false;
            if (_breakEffectPrefab != null)
            {
                var effect = Instantiate(_breakEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, _effectDestroyDelay);
            }
            Destroy(gameObject);
        }
    }
}