using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MaruSikaku.Stage;

namespace MaruSikaku.Editor.Data
{
    [CreateAssetMenu(menuName = "MaruSikaku/Stage Object Database")]
    public class StageObjectDatabase : ScriptableObject
    {        
        [SerializeField]
        private List<Entry> _entries = new();
        private Dictionary<EStageObjectType, Entry> _map = new();

        [Serializable]
        private class Entry
        {
            public EStageObjectType Type { get; set; }
            public GameObject Prefab { get; set; }
            public Sprite EditorIcon { get; set; }
        }

        public GameObject GetPrefab(EStageObjectType type)
        {
            EnsureMap();
            return _map.TryGetValue(type, out var entry) ? entry.Prefab : null;
        }

        public Sprite GetSprite(EStageObjectType type)
        {
            EnsureMap();
            return _map.TryGetValue(type, out var entry) ? entry.EditorIcon : null;
        }

        private void EnsureMap()
        {
            if (_map != null) { return; }
            _map = _entries.ToDictionary(e => e.Type);
        }
    }
}