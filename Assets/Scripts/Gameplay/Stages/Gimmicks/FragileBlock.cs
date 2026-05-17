using UnityEngine;

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    public class FragileBlock : MonoBehaviour
    {
        [SerializeField] private GameObject _breakEffectPrefab;
        [SerializeField] private float _effectDestroyDelay = 1.0f;

        public void Break()
        {
            if (_breakEffectPrefab != null)
            {
                var effect = Instantiate(_breakEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, _effectDestroyDelay);
            }
            Destroy(gameObject);
        }
    }
}