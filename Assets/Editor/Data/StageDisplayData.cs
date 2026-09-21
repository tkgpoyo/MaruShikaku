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

        private bool _isChanged = false;
        /// <summary>セル座標と地形タイルの対応</summary>
        private Dictionary<Vector2Int, StageTerrainCell> _terrainDic;
        /// <summary>セル座標とステージオブジェクトの対応</summary>
        private Dictionary<Vector2Int, StageObjectDisplayData> _stageObjectDic;
        /// <summary>地形タイルのリスト</summary>
        private NotifyList<StageTerrainCell> _terrainCells;

        /// <summary>ステージ上のオブジェクトのリスト</summary>
        private NotifyList<StageObjectDisplayData> _stageObjects;

        public StageDisplayData()
        {
            _terrainDic = new();
            _stageObjectDic = new();
            _terrainCells = new();
            _stageObjects = new();

            _terrainCells.propertyChanged += OnTerrainCellsChanged;
            _stageObjects.propertyChanged += OnStageObjectsChanged;
            _stageObjects.itemPropertyChanged += OnStageObjectPropertyChanged;
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
            get => _sikakuStart;
            set
            {
                if (_sikakuStart == value) { return; }
                _sikakuStart = value;
                Notify(nameof(SikakuStartX));
                Notify(nameof(SikakuStartY));
            }
        }
        private Vector2Int _sikakuStart = new (1, 0);

        /// <summary>丸キャラクターの初期位置</summary>
        public Vector2Int MaruGoal
        {
            get => _maruGoal;
            set
            {
                if (_maruGoal == value) { return; }
                _maruGoal = value;
                Notify(nameof(MaruGoalX));
                Notify(nameof(MaruGoalY));
            }
        }
        private Vector2Int _maruGoal = new (8, 0);

        /// <summary>四角キャラクターの初期位置</summary>
        public Vector2Int SikakuGoal
        {
            get => _sikakuGoal;
            set
            {
                if (_sikakuGoal == value) { return; }
                _sikakuGoal = value;
                Notify(nameof(SikakuGoalX));
                Notify(nameof(SikakuGoalY));
            }
        }
        private Vector2Int _sikakuGoal = new (9, 0);

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
        public IReadOnlyList<StageTerrainCell> TerrainCells => _terrainCells;
        /// <summary>ステージオブジェクトのリスト</summary>
        public IReadOnlyList<StageObjectDisplayData> StageObjects => _stageObjects;
        /// <summary>ステージが編集されたかどうか</summary>
        public bool IsChanged => _isChanged;
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
            get => _sikakuStart.x;
            set
            {
                value = Mathf.Clamp(value, 0, SizeX);
                if (_sikakuStart.x == value) { return; }
                _sikakuStart.x = value;
                Notify();
            }
        }
        /// <summary>四角キャラクターの初期位置のY座標</summary>
        [CreateProperty]
        public int SikakuStartY
        {
            get => _sikakuStart.y;
            set
            {
                value = Mathf.Clamp(value, 0, SizeY);
                if (_sikakuStart.y == value) { return; }
                _sikakuStart.y = value;
                Notify();
            }
        }

        /// <summary>丸キャラクターのゴール位置のX座標</summary>
        [CreateProperty]
        public int MaruGoalX
        {
            get => _maruGoal.x;
            set
            {
                value = Mathf.Clamp(value, 0, SizeX);
                if (_maruGoal.x == value) { return; }
                _maruGoal.x = value;
                Notify();
            }
        }

        /// <summary>丸キャラクターのゴール位置のY座標</summary>
        [CreateProperty]
        public int MaruGoalY
        {
            get => _maruGoal.y;
            set
            {
                value = Mathf.Clamp(value, 0, SizeY);
                if (_maruGoal.y == value) { return; }
                _maruGoal.y = value;
                Notify();
            }
        }

        /// <summary>四角キャラクターのゴール位置のX座標</summary>
        [CreateProperty]
        public int SikakuGoalX
        {
            get => _sikakuGoal.x;
            set
            {
                value = Mathf.Clamp(value, 0, SizeX);
                if (_sikakuGoal.x == value) { return; }
                _sikakuGoal.x = value;
                Notify();
            }
        }

        /// <summary>四角キャラクターのゴール位置のY座標</summary>
        [CreateProperty]
        public int SikakuGoalY
        {
            get => _sikakuGoal.y;
            set
            {
                value = Mathf.Clamp(value, 0, SizeY);
                if (_sikakuGoal.y == value) { return; }
                _sikakuGoal.y = value;
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

        private void OnStageObjectPropertyChanged(object sender, ItemPropertyChangedEventArgs<StageObjectDisplayData> e)
        {
            switch (e.PropertyName)
            {
                case nameof(StageObjectDisplayData.Region):
                    {
                        // 登録する領域内にすでに他のオブジェクトがあるなら例外を投げる
                        if (e.Item.Region.Any(
                            cell =>
                            (TryGetTerrainCell(cell, out _)) ||
                            (TryGetStageObject(cell, out var obj) && !ReferenceEquals(obj, e.Item))))
                        {
                            throw new InvalidOperationException($"ステージオブジェクトの領域が重複しています．");
                        }
                        UnregisterRegion(e.Item);               // 現在登録されている情報を削除
                        RegisterRegion(e.Item);                 // 新しく再登録
                    }
                    break;
            }
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

        public void AddStageObject(StageObjectDisplayData stageObject)
        {
            if (!CanPlaceRegion(stageObject.Region, stageObject)) { return; }   // おけない場合は追加しない
            RegisterRegion(stageObject);
            _stageObjects.Add(stageObject);
        }

        public void RemoveStageObject(StageObjectDisplayData stageObject)
        {
            if (!_stageObjects.Contains(stageObject)) { return; }
            UnregisterRegion(stageObject);
            _stageObjects.Remove(stageObject);
        }

        public bool TryGetTerrainCell(Vector2Int pos, out StageTerrainCell stageTerrainCell)
        {
            if (_terrainDic.TryGetValue(pos, out stageTerrainCell)) { return true; }

            stageTerrainCell = null;
            return false;
        }

        public bool TryGetStageObject(Vector2Int pos, out StageObjectDisplayData stageObject)
        {
            if (_stageObjectDic.TryGetValue(pos, out stageObject)) { return true; }

            stageObject = null;
            return false;
        }

        /// <summary>
        /// 指定の位置にオブジェクトを移動可能かを判定し，移動可能ならばその座標へ移動します．
        /// </summary>
        /// <param name="stageObj">判定するオブジェクト</param>
        /// <param name="targetPos">移動先の座標</param>
        /// <returns>移動可能かどうか</returns>
        public bool TryMoveObject(StageObjectDisplayData stageObj, Vector2Int targetPos)
        {
            if (_stageObjects.Contains(stageObj) &&                         // 指定のオブジェクトが登録されていて
                CanPlaceRegion(stageObj.GetRegionAt(targetPos), stageObj))  // 移動できるなら
            {
                stageObj.MoveTo(targetPos);                                 // オブジェクトを移動
                return true;
            }
            return false;
        }

        /// <summary>
        /// 指定の長さに壁オブジェクトが設定可能かを判定し，設定可能ならばその長さに設定します．
        /// </summary>
        /// <param name="wall">壁オブジェクト</param>
        /// <param name="targetLength">壁の長さ</param>
        /// <returns>壁の長さを設定可能か</returns>
        public bool TryChangeWallLength(WallObjectDisplayData wall, int targetLength)
        {
            if (targetLength < 1) { return false; }                             // 壁の長さが1未満なら不適切
            if (_stageObjects.Contains(wall) &&                                 // 指定の壁オブジェクトが登録されていて
                CanPlaceRegion(wall.GetRegionForLength(targetLength), wall))    // 長さを変更できるなら
            {
                wall.YLength = targetLength;                                    // 壁の長さを変更
                return true;
            }
            return false;
        }

        /// <summary>
        /// ステージオブジェクトや地面が指定の座標に存在するかどうかを判定します．
        /// </summary>
        /// <param name="pos">確認する座標</param>
        /// <returns>指定の座標にステージオブジェクトか地面が存在するかどうか</returns>
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

        public void SetStageObjects(IEnumerable<StageObjectDisplayData> stageObjects)
        {
            var objects = stageObjects?.ToList() ?? new List<StageObjectDisplayData>();
            var objectDic = objects.SelectMany(stageObj => stageObj.Region.Select(cell => (Cell: cell, Object: stageObj)))
                                   .ToDictionary(entry => entry.Cell, entry => entry.Object);
            
            _stageObjects.propertyChanged -= OnStageObjectsChanged;
            _stageObjects.itemPropertyChanged -= OnStageObjectPropertyChanged;
            _stageObjects.Clear();
            _stageObjects = new(objects);
            _stageObjectDic = objectDic;
            _stageObjects.propertyChanged += OnStageObjectsChanged;
            _stageObjects.itemPropertyChanged += OnStageObjectPropertyChanged;

            Notify(nameof(StageObjects));
        }

        public bool IsInsideStage(Vector2Int pos)
        {
            return  0 <= pos.x && pos.x < SizeX && 0 <= pos.y && pos.y < SizeY;
        }

        /// <summary>
        /// ステージが保存されたこととするメソッドです．
        /// </summary>
        /// <remarks>
        /// ステージ編集画面で保存ボタンが押された時に呼び出す．
        /// </remarks>
        public void SetAsSaved()
        {
            _isChanged = false;
        }
        #endregion メソッド

        #region 内部関数
        /// <summary>
        /// 変更通知を行います．
        /// </summary>
        /// <param name="property">プロパティ名</param>
        private void Notify([CallerMemberName] string property = "")
        {
            _isChanged = true;      // 変更されたら編集済みとする
            propertyChanged?.Invoke(this, new(property));
        }

        /// <summary>
        /// 現在辞書に登録されている領域情報を削除します．
        /// </summary>
        /// <param name="stageObj">登録解除するオブジェクト</param>
        private void UnregisterRegion(StageObjectDisplayData stageObj)
        {
            var oldCells = _stageObjectDic.Where(pair => ReferenceEquals(pair.Value, stageObj))
                                          .Select(pair => pair.Key).ToArray();      // 削除するセル
            foreach (var oldCell in oldCells)
            {
                _stageObjectDic.Remove(oldCell);
            }
        }

        /// <summary>
        /// 辞書にオブジェクトの領域情報を登録します．
        /// </summary>
        /// <param name="stageObj">登録するオブジェクト</param>
        private void RegisterRegion(StageObjectDisplayData stageObj)
        {
            foreach (var newCell in stageObj.Region)
            {
                _stageObjectDic.Add(newCell, stageObj);
            }
        }

        /// <summary>
        /// 領域が配置可能かどうかを判定します．
        /// </summary>
        /// <param name="region">領域</param>
        /// <param name="excludeObject">判定除外するオブジェクト</param>
        /// <returns></returns>
        private bool CanPlaceRegion(IEnumerable<Vector2Int> region, StageObjectDisplayData excludeObject = default)
        {
            foreach (var cell in region)
            {
                if (!IsInsideStage(cell) ||                                     // ステージ外か
                    _terrainDic.ContainsKey(cell) ||                            // 地面が存在するか
                    (_stageObjectDic.TryGetValue(cell, out var stageObj) &&     // すでにマス内にオブジェクトが存在して
                    !ReferenceEquals(stageObj, excludeObject)))                 // それが別のオブジェクトである場合
                {
                    return false;                                           // 配置不可能
                }
            }
            // 領域内の全てのマスに他のオブジェクトがない場合は配置可能
            return true;
        }
        #endregion 内部関数
    }
}