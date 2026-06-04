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

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

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
        private Vector2Int _maruStart = Vector2Int.zero;

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
        private Vector2Int _SikakuStart = Vector2Int.zero;

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

        /// <summary>セル座標と地形タイルの対応</summary>
        public Dictionary<Vector2Int, StageTerrainCell> TerrainDic { get; private set; } = new();
        /// <summary>セル座標とステージオブジェクトの対応</summary>
        public Dictionary<Vector2Int, StageObject> StageObjectDic { get; private set; } = new();

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

        /// <summary>地形タイルのリスト</summary>
        /// <remarks><see cref="StageTerrainCell"/>を追加する際は，<see cref="AddTerrainCell"/>メソッドを，削除する際は<see cref="RemoveTerrainCell"/>を使用してください．</remarks>
        public NotifyList<StageTerrainCell> TerrainCells
        {
            get => _terrainCells;
            set
            {
                if (_terrainCells == value) { return; }
                _terrainCells = value;
                TerrainDic = _terrainCells.ToDictionary(terrainCell => terrainCell.Pos);
                Notify();
            }
        }
        private NotifyList<StageTerrainCell> _terrainCells = new();

        /// <summary>ステージ上のオブジェクトのリスト</summary>
        /// <remarks><see cref="StageObject"/>を追加する際は，<see cref="AddStageObject"/>メソッドを，削除する際は<see cref="RemoveStageObject"/>を使用してください．</remarks>
        public NotifyList<StageObject> StageObjects
        {
            get => _stageObjects;
            set
            {
                if (_stageObjects == value) { return; }
                _stageObjects = value;
                StageObjectDic = _stageObjects.ToDictionary(stageObject => stageObject.Pos);
                Notify();
            }
        }
        private NotifyList<StageObject> _stageObjects = new();

        #endregion 画面表示用プロパティ

        public void AddTerrainCell(StageTerrainCell terrainCell)
        {
            if (TerrainDic.ContainsKey(terrainCell.Pos)) { return; }
            TerrainDic.Add(terrainCell.Pos, terrainCell);
            TerrainCells.Add(terrainCell);
        }

        public void RemoveTerrainCell(StageTerrainCell terrainCell)
        {
            if (!TerrainDic.ContainsKey(terrainCell.Pos)) { return; }
            TerrainDic.Remove(terrainCell.Pos);
            TerrainCells.Remove(terrainCell);
        }

        public void AddStageObject(StageObject stageObject)
        {
            if (StageObjectDic.ContainsKey(stageObject.Pos)) { return; }
            StageObjectDic.Add(stageObject.Pos, stageObject);
            StageObjects.Add(stageObject);
        }

        public void RemoveStageObject(StageObject stageObject)
        {
            if (!StageObjectDic.ContainsKey(stageObject.Pos)) { return; }
            StageObjectDic.Remove(stageObject.Pos);
            StageObjects.Remove(stageObject);
        }

        /// <summary>
        /// 変更通知を行います．
        /// </summary>
        /// <param name="property">プロパティ名</param>
        private void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }
}