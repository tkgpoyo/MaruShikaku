using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaruSikaku.Stage
{    
    public enum EStageObjectType
    {
        Fragile,
        Movable,
        Spring,
        Switch,
        Wall,
    }

    public abstract class StageObject : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        [CreateProperty]
        public int Id
        {
            get => _id;
            private set
            {
                if (_id == value) { return; }
                _id = value;
                Notify();
            }
        }
        private int _id;

        [CreateProperty]
        public Vector2Int Pos
        {
            get => _pos;
            set
            {
                if (_pos == value) { return; }
                _pos = value;
                Notify();
            }
        }
        private Vector2Int _pos;

        [CreateProperty]
        public bool IsDeleted
        {
            get => _isDeleted;
            set
            {
                if (_isDeleted == value) { return; }
                _isDeleted = value;
                Notify();
            }
        }
        private bool _isDeleted;

        public abstract EStageObjectType Type { get; }

        public StageObject(int id, Vector2Int pos, bool isDeleted = false)
        {
            Id = id;
            Pos = pos;
            IsDeleted = isDeleted;
        }

        /// <summary>
        /// 変更通知を行います．
        /// </summary>
        /// <param name="property">プロパティ名</param>
        protected void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }

    [Serializable]
    public class StageObjectSaveData
    {
        [SerializeField]
        private int _id;
        public int Id => _id;

        [SerializeField]
        private Vector2Int _pos;
        public Vector2Int Pos => _pos;

        [SerializeField]
        private EStageObjectType _type;
        public EStageObjectType Type => _type;

        [SerializeField]
        private int _switchId = -1;
        public int SwitchId => _switchId;

        public StageObjectSaveData(int id, Vector2Int pos, EStageObjectType type, int switchId)
        {
            _id = id;
            _pos = pos;
            _type = type;
            _switchId = switchId;
        }
    }

    public class SpringObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Spring;
        public SpringObject(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class FragileObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Fragile;
        public FragileObject(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class MovableObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Movable;
        public MovableObject(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class SwitchObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Switch;
        public SwitchObject(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class WallObject : StageObject
    {
        [CreateProperty]
        public int SwitchId
        {
            get => _switchId;
            set
            {
                if (_switchId == value) { return; }
                _switchId = value;
                Notify();
            }
        }
        private int _switchId;

        public override EStageObjectType Type => EStageObjectType.Wall;

        public WallObject(int id, Vector2Int pos, int switchId = -1, bool isDeleted = false) : base(id, pos, isDeleted)
        {
            SwitchId = switchId;
        }
    }
}