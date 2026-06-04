using System;
using UnityEngine;
namespace MaruSikaku.Stage
{
    public enum ETerrainType
    {
        Ground
    }
    public class StageTerrainCell
    {
        public Vector2Int Pos { get; private set; }
        public ETerrainType Type{ get; private set; }

        public StageTerrainCell(Vector2Int pos, ETerrainType type)
        {
            Pos = pos;
            Type = type;
        }
    }
    
    [Serializable]
    public class StageTerrainSaveData
    {
        [SerializeField]
        private Vector2Int _pos;
        public Vector2Int Pos => _pos;
        [SerializeField]
        private ETerrainType _type;
        public ETerrainType Type => _type;

        public StageTerrainSaveData(Vector2Int pos, ETerrainType type)
        {
            _pos = pos;
            _type = type;
        }
    }
}