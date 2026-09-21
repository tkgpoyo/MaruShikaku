using System;
using System.Collections.Generic;
using System.Linq;
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

    public abstract class StageObjectDisplayData : INotifyBindablePropertyChanged
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
            private set
            {
                if (_pos == value) { return; }
                _pos = value;
                UpdateRegion();
                Notify();
            }
        }
        private Vector2Int _pos;

        [CreateProperty]
        public bool IsDeleted
        {
            get => _isDeleted;
            private set
            {
                if (_isDeleted == value) { return; }
                _isDeleted = value;
                Notify();
            }
        }
        private bool _isDeleted;

        /// <summary>オブジェクトが占める領域</summary>
        public IReadOnlyCollection<Vector2Int> Region => _region ??= CreateRegionSnapshot();
        private IReadOnlyCollection<Vector2Int> _region;

        public abstract EStageObjectType Type { get; }

        /// <summary>
        /// 領域を更新し，変更を通知します．
        /// </summary>
        protected void UpdateRegion()
        {
            _region = CreateRegionSnapshot();
            Notify(nameof(Region));
        }

        /// <summary>
        /// 占有するセルを列挙します．
        /// </summary>
        /// <returns>領域のセル一覧</returns>
        protected virtual IEnumerable<Vector2Int> EnumerateRegionCells()
        {
            yield return Pos;
        }

        /// <summary>
        /// 領域のスナップショットを作成します．
        /// </summary>
        /// <returns>領域のスナップショット</returns>
        private IReadOnlyCollection<Vector2Int> CreateRegionSnapshot()
        {
            return new List<Vector2Int>(EnumerateRegionCells().Distinct()).AsReadOnly();
        }

        public StageObjectDisplayData(int id, Vector2Int pos, bool isDeleted = false)
        {
            Id = id;
            _pos = pos;
            IsDeleted = isDeleted;
        }

        public void MoveTo(Vector2Int pos)
        {
            Pos = pos;
        }

        /// <summary>
        /// 指定の位置にオブジェクトがある時の占有領域を計算します．
        /// </summary>
        /// <param name="pos">確認する位置</param>
        /// <returns>位置に対する占有領域</returns>
        public virtual IReadOnlyCollection<Vector2Int> GetRegionAt(Vector2Int pos)
        {
            var region = new List<Vector2Int>();
            region.Add(pos);
            return region;
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

        [SerializeField]
        private int _yLength = -1;
        public int YLength => _yLength;

        public StageObjectSaveData(int id, Vector2Int pos, EStageObjectType type, int switchId, int yLength)
        {
            _id = id;
            _pos = pos;
            _type = type;
            _switchId = switchId;
            _yLength = yLength;
        }
    }

    public class SpringObjectDisplayData : StageObjectDisplayData
    {
        public override EStageObjectType Type => EStageObjectType.Spring;
        public SpringObjectDisplayData(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class FragileObjectDisplayData : StageObjectDisplayData
    {
        public override EStageObjectType Type => EStageObjectType.Fragile;
        public FragileObjectDisplayData(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class MovableObjectDisplayData : StageObjectDisplayData
    {
        public override EStageObjectType Type => EStageObjectType.Movable;
        public MovableObjectDisplayData(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class SwitchObjectDisplayData : StageObjectDisplayData
    {
        public override EStageObjectType Type => EStageObjectType.Switch;
        public SwitchObjectDisplayData(int id, Vector2Int pos, bool isDeleted = false) : base(id, pos, isDeleted)
        {
        }
    }

    public class WallObjectDisplayData : StageObjectDisplayData
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

        [CreateProperty]
        public int YLength
        {
            get => _yLength;
            set
            {
                if (_yLength == value) { return; }
                _yLength = value;
                UpdateRegion();
                Notify();
            }
        }
        private int _yLength = 1;

        public override EStageObjectType Type => EStageObjectType.Wall;

        public WallObjectDisplayData(int id, Vector2Int pos, int switchId, int yLength, bool isDeleted = false) : base(id, pos, isDeleted)
        {
            SwitchId = switchId;
            YLength = yLength;
        }

        protected override IEnumerable<Vector2Int> EnumerateRegionCells()
        {
            for (int l = 0; l < YLength; l++)
            {
                yield return new Vector2Int(Pos.x, Pos.y - l);
            }
        }

        public override IReadOnlyCollection<Vector2Int> GetRegionAt(Vector2Int pos)
        {
            var region = new List<Vector2Int>();
            for (int l = 0; l < YLength; l++)
            {
                region.Add(new(pos.x, pos.y - l));
            }
            return region;
        }

        public IReadOnlyCollection<Vector2Int> GetRegionForLength(int length)
        {
            var region = new List<Vector2Int>();
            for (int l = 0; l < length; l++)
            {
                region.Add(new(Pos.x, Pos.y - l));
            }
            return region;
        }
    }
}
