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
        private Vector2Int _sikakuInitPos = new(1, 0);
        public Vector2Int SikakuInitPos => _sikakuInitPos;
        /// <summary>丸キャラクターのゴール位置</summary>
        [SerializeField]
        private Vector2Int _maruGoalPos = new(8,0);
        public Vector2Int MaruGoalPos => _maruGoalPos;
        /// <summary>四角キャラクターのゴール位置</summary>
        [SerializeField]
        private Vector2Int _sikakuGoalPos = new(9,0);
        public Vector2Int SikakuGoalPos => _sikakuGoalPos;
        /// <summary>地形ブロックのリスト</summary>
        [SerializeField]
        private List<StageTerrainSaveData> _terrainCells = new();
        public List<StageTerrainSaveData> TerrainCells => _terrainCells;
        /// <summary>ステージオブジェクトのリスト</summary>
        [SerializeField]
        private List<StageObjectSaveData> _stageObjects = new();
        public List<StageObjectSaveData> StageObjects => _stageObjects;

        public StageSaveData(Vector2Int size, Vector2Int maruStart, Vector2Int sikakuStart, 
                            Vector2Int maruGoal, Vector2Int sikakuGoal,
                            List<StageTerrainSaveData> terrainCells, List<StageObjectSaveData> stageObjects)
        {
            _size = size;
            _maruInitPos = maruStart;
            _sikakuInitPos = sikakuStart;
            _maruGoalPos = maruGoal;
            _sikakuGoalPos = sikakuGoal;
            _terrainCells = terrainCells;
            _stageObjects = stageObjects;
        }
    }
}