using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using MaruSikaku.Stage;

namespace MaruSikaku.Editor.Data
{
    [CreateAssetMenu(menuName = "MaruSikaku/Stage Terrain Database")]
    public class StageTerrainDatabase : ScriptableObject
    {
        [SerializeField]
        private List<Entry> _entries = new();
        private Dictionary<ETerrainType, Entry> _map = new();

        [Serializable]
        private class Entry
        {
            public ETerrainType Type { get; set; }
            public TileBase Tile { get; set; }
            public Color Color { get; set; }
        }

        public TileBase GetTile(ETerrainType type)
        {
            EnsureMap();
            return _map.TryGetValue(type, out var entry) ? entry.Tile : null;
        }

        public Color GetColor(ETerrainType type)
        {
            EnsureMap();
            return _map.TryGetValue(type, out var entry) ? entry.Color : Color.black;
        }

        private void EnsureMap()
        {
            if (_map != null) { return; }
            _map = _entries.ToDictionary(e => e.Type);
        }
    }
}