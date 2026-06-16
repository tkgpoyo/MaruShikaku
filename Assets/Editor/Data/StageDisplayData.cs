using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using Unity.Properties;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using MaruSikaku.Stage;

namespace MaruSikaku.Editor.Data
{
    public class StageDisplayData : INotifyBindablePropertyChanged
    {
        /// <summary>ステージの最小サイズのX</summary>
        private const int STAGE_MIN_SIZE_X = 10;
        /// <summary>ステージの最小サイズのY</summary>
        private const int STAGE_MIN_SIZE_Y = 10;

        /// <summary>セル座標と地形タイルの対応</summary>
        private Dictionary<Vector2Int, StageTerrainCell> _terrainDic;
        /// <summary>セル座標とステージオブジェクトの対応</summary>
        private Dictionary<Vector2Int, StageObject> _stageObjectDic;
        /// <summary>地形タイルのリスト</summary>
        private NotifyList<StageTerrainCell> _terrainCells;

        /// <summary>ステージ上のオブジェクトのリスト</summary>
        private NotifyList<StageObject> _stageObjects;

        public StageDisplayData()
        {
            _terrainDic = new();
            _stageObjectDic = new();
            _terrainCells = new();
            _stageObjects = new();

            _terrainCells.propertyChanged += OnTerrainCellsChanged;
            _stageObjects.propertyChanged += OnStageObjectsChanged;
        }

        #region プロパティ
        /// <summary>丸キャラクターの初期位置</summary>
        public Vector2Int MaruStart
        {
            get => _maruStart;
            set
            {
                if (_maruStart == value) { return; }
                _maruStart = value;
                Notify(nameof(MaruStartX));
                Notify(nameof(MaruStartY));
            }
        }
        private Vector2Int _maruStart = new (2, 0);

        /// <summary>四角キャラクターの初期位置</summary>
        public Vector2Int SikakuStart
        {
            get => _SikakuStart;
            set
            {
                if (_SikakuStart == value) { return; }
                _SikakuStart = value;
                Notify(nameof(SikakuStartX));
                Notify(nameof(SikakuStartY));
            }
        }
        private Vector2Int _SikakuStart = new (1, 0);

        /// <summary>ステージのサイズ</summary>
        public Vector2Int Size
        {
            get => _size;
            set
            {
                if (_size == value) { return; }
                _size = value;
                Notify(nameof(SizeX));
                Notify(nameof(SizeY));
            }
        }
        private Vector2Int _size = new (10, 10);

        /// <summary>地形タイルのリスト</summary>
        public IReadOnlyList<StageTerrainCell> TerrainCells => (IReadOnlyList<StageTerrainCell>)_terrainCells;
        /// <summary>ステージオブジェクトのリスト</summary>
        public IReadOnlyList<StageObject> StageObjects => (IReadOnlyList<StageObject>)_stageObjects;
        #endregion プロパティ

        #region 画面表示用プロパティ
        /// <summary>丸キャラクターの初期位置のX座標</summary>
        [CreateProperty]
        public int MaruStartX
        {
            get => _maruStart.x;
            set
            {
                value = Mathf.Clamp(value, 0, SizeX);
                if (_maruStart.x == value) { return; }
                _maruStart.x = value;
                Notify();
            }
        }
        /// <summary>丸キャラクターの初期位置のY座標</summary>
        [CreateProperty]
        public int MaruStartY
        {
            get => _maruStart.y;
            set
            {
                value = Mathf.Clamp(value, 0, SizeY);
                if (_maruStart.y == value) { return; }
                _maruStart.y = value;
                Notify();
            }
        }

        /// <summary>四角キャラクターの初期位置のX座標</summary>
        [CreateProperty]
        public int SikakuStartX
        {
            get => _SikakuStart.x;
            set
            {
                value = Mathf.Clamp(value, 0, SizeX);
                if (_SikakuStart.x == value) { return; }
                _SikakuStart.x = value;
                Notify();
            }
        }
        /// <summary>四角キャラクターの初期位置のY座標</summary>
        [CreateProperty]
        public int SikakuStartY
        {
            get => _SikakuStart.y;
            set
            {
                value = Mathf.Clamp(value, 0, SizeY);
                if (_SikakuStart.y == value) { return; }
                _SikakuStart.y = value;
                Notify();
            }
        }

        /// <summary>ステージのX方向のサイズ</summary>
        [CreateProperty]
        public int SizeX
        {
            get => _size.x;
            set
            {
                value = Mathf.Max(value, STAGE_MIN_SIZE_X);
                if (_size.x == value) { return; }
                _size.x = value;
                Notify();
            }
        }
        /// <summary>ステージのY方向のサイズ</summary>
        [CreateProperty]
        public int SizeY
        {
            get => _size.y;
            set
            {
                value = Mathf.Max(value, STAGE_MIN_SIZE_Y);
                if (_size.y == value) { return; }
                _size.y = value;
                Notify();
            }
        }
        #endregion 画面表示用プロパティ

        #region イベント
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        private void OnTerrainCellsChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(nameof(TerrainCells));
        }

        private void OnStageObjectsChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(nameof(StageObjects));
        }
        #endregion イベント

        #region メソッド
        public void AddTerrainCell(StageTerrainCell terrainCell)
        {
            if (HasAnyStageElement(terrainCell.Pos)) { return; }
            _terrainDic.Add(terrainCell.Pos, terrainCell);
            _terrainCells.Add(terrainCell);
        }

        public void RemoveTerrainCell(StageTerrainCell terrainCell)
        {
            if (!_terrainDic.ContainsKey(terrainCell.Pos)) { return; }
            _terrainDic.Remove(terrainCell.Pos);
            _terrainCells.Remove(terrainCell);
        }

        public void AddStageObject(StageObject stageObject)
        {
            if (HasAnyStageElement(stageObject.Pos)) { return; }
            _stageObjectDic.Add(stageObject.Pos, stageObject);
            _stageObjects.Add(stageObject);
        }

        public void RemoveStageObject(StageObject stageObject)
        {
            if (!_stageObjectDic.ContainsKey(stageObject.Pos)) { return; }
            _stageObjectDic.Remove(stageObject.Pos);
            _stageObjects.Remove(stageObject);
        }

        public bool TryGetTerrainCell(Vector2Int pos, out StageTerrainCell stageTerrainCell)
        {
            if (_terrainDic.TryGetValue(pos, out stageTerrainCell)) { return true; }

            stageTerrainCell = null;
            return false;
        }

        public bool TryGetStageObject(Vector2Int pos, out StageObject stageObject)
        {
            if (_stageObjectDic.TryGetValue(pos, out stageObject)) { return true; }

            stageObject = null;
            return false;
        }

        public bool HasAnyStageElement(Vector2Int pos)
        {
            if (_terrainDic.ContainsKey(pos) || _stageObjectDic.ContainsKey(pos)) { return true; }
            
            return false;
        }

        public void SetTerrainCells(IEnumerable<StageTerrainCell> stageTerrainCells)
        {
            _terrainCells.propertyChanged -= OnTerrainCellsChanged;
            _terrainCells = new NotifyList<StageTerrainCell>(stageTerrainCells);
            _terrainCells.propertyChanged += OnTerrainCellsChanged;
            _terrainDic = _terrainCells.ToDictionary(terrainCell => terrainCell.Pos);

            Notify(nameof(TerrainCells));
        }

        public void SetStageObjects(IEnumerable<StageObject> stageObjects)
        {
            _stageObjects.propertyChanged -= OnStageObjectsChanged;
            _stageObjects = new NotifyList<StageObject>(stageObjects);
            _stageObjects.propertyChanged += OnStageObjectsChanged;
            _stageObjectDic = _stageObjects.ToDictionary(stageObject => stageObject.Pos);

            Notify(nameof(StageObjects));
        }
        #endregion メソッド

        #region 内部関数
        /// <summary>
        /// 変更通知を行います．
        /// </summary>
        /// <param name="property">プロパティ名</param>
        private void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
        #endregion 内部関数
    }
}