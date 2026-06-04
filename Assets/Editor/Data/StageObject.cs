using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaruSikaku.Editor.Data
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
        private static int _nextId = 0;
        private static object _lockId = new();

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

        public StageObject(Vector2Int pos, bool isDeleted = false)
        {
            Id = GetNextId();

            Pos = pos;
            IsDeleted = isDeleted;
        }

        private static int GetNextId()
        {
            int id;
            lock(_lockId)
            {
                id = _nextId++;
            }
            return id;
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

    public class SpringObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Spring;
        public SpringObject(Vector2Int pos, bool isDeleted = false) : base(pos, isDeleted)
        {
        }
    }

    public class FragileObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Fragile;
        public FragileObject(Vector2Int pos, bool isDeleted = false) : base(pos, isDeleted)
        {
        }
    }

    public class MovableObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Movable;
        public MovableObject(Vector2Int pos, bool isDeleted = false) : base(pos, isDeleted)
        {
        }
    }

    public class SwitchObject : StageObject
    {
        public override EStageObjectType Type => EStageObjectType.Switch;
        public SwitchObject(Vector2Int pos, bool isDeleted = false) : base(pos, isDeleted)
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

        public WallObject(Vector2Int pos, int switchId = -1, bool isDeleted = false) : base(pos, isDeleted)
        {
            SwitchId = switchId;
        }
    }
}