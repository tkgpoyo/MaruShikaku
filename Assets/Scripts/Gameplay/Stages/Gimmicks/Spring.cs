using System.Collections;
using MaruSikaku.Gameplay.Players;
using UnityEngine;

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    [RequireComponent(typeof(Animator))]
    public class Spring : MonoBehaviour
    {
        private const string TRIG_COMPRESS = "Compress";

        [SerializeField] private float _stretchVelocity = 12f;

        private Animator _anim;
        private bool _isCompressing;
        private bool _isCompressAnimEnd;
        private bool _isStretchAnimEnd;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public void Compress(PlayerContext context)
        {
            if (_isCompressing) return;

            StartCoroutine(CompressCoroutine(context));
        }

        // Animation Event
        public void CompressAnimEnd()
        {
            _isCompressAnimEnd = true;
        }

        // Animation Event
        public void StretchAnimEnd()
        {
            _isStretchAnimEnd = true;
        }

        private IEnumerator CompressCoroutine(PlayerContext context)
        {
            _isCompressing = true;

            _isCompressAnimEnd = false;
            _isStretchAnimEnd = false;

            _anim.SetTrigger(TRIG_COMPRESS);

            while (!_isCompressAnimEnd)
            {
                yield return null;
            }

            if (context == null || context.RigidBody == null)
            {
                _isCompressing = false;
                yield break;
            }

            yield return new WaitForFixedUpdate();

            var velocity = context.RigidBody.linearVelocity;
            velocity.y = _stretchVelocity;
            context.RigidBody.linearVelocity = velocity;

            while (!_isStretchAnimEnd)
            {
                yield return null;
            }

            _isCompressing = false;
        }
    }
}