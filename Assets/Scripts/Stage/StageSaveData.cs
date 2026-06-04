using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaruSikaku.Stage
{
    /// <summary>ステージデータを表すクラス</summary>
    [Serializable]
    public class StageSaveData
    {
        /// <summary>ステージサイズ</summary>
        [SerializeField]
        private Vector2Int _size = new(10, 10);
        public Vector2Int Size => _size;
        /// <summary>丸キャラクターの初期位置</summary>
        [SerializeField]
        private Vector2Int _maruInitPos = new(2, 0);
        public Vector2Int MaruInitPos => _maruInitPos;
        /// <summary>四角キャラクターの初期位置</summary>
        [SerializeField]
        private Vector2Int _SikakuInitPos = new(1, 0);
        public Vector2Int SikakuInitPos => _SikakuInitPos;
        /// <summary>地形ブロックのリスト</summary>
        [SerializeField]
        private List<StageTerrainSaveData> _terrainCells = new();
        public List<StageTerrainSaveData> TerrainCells => _terrainCells;
        /// <summary>ステージオブジェクトのリスト</summary>
        [SerializeField]
        private List<StageObjectSaveData> _stageObjects = new();
        public List<StageObjectSaveData> StageObjects => _stageObjects;

        public StageSaveData(Vector2Int size, Vector2Int maruStart, Vector2Int SikakuStart, List<StageTerrainSaveData> terrainCells, List<StageObjectSaveData> stageObjects)
        {
            _size = size;
            _maruInitPos = maruStart;
            _SikakuInitPos = SikakuStart;
            _terrainCells = terrainCells;
            _stageObjects = stageObjects;
        }
    }
}