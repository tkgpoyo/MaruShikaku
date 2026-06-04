using UnityEngine;
namespace MaruSikaku.Editor.Data
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
}