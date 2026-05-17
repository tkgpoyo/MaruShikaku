using System;
using UnityEngine;

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    [RequireComponent(typeof(Animator))]
    public class Spring : MonoBehaviour
    {
        private enum ESpringState
        {
            Idle,
            Compress,
            Stretch
        }

        private const string TRIG_COMPRESS = "Compress";

        [SerializeField] private float _stretchPower;
        public float StretchPower => _stretchPower;

        private Animator _anim;
        private ESpringState _state = ESpringState.Idle;

        public event Action OnStretch;

        void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public bool Compress()
        {
            switch (_state)
            {
            case ESpringState.Idle:
                _state = ESpringState.Compress;
                _anim.SetTrigger(TRIG_COMPRESS);
                return true;
            default:
                return false;
            }
        }

        public void CompressAnimEnd()
        {
            _state = ESpringState.Stretch;
            OnStretch?.Invoke();
        }

        public void StretchAnimEnd()
        {
            _state = ESpringState.Idle;
        }
    }
}