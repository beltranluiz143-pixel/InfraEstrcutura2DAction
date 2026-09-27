using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "AssetReferenceDatabase", menuName = "Infraestructura2DAction/Core/Asset Reference Database")]
    public class AssetReferenceDatabase : ScriptableObject
    {
        [Serializable]
        public class AssetEntry
        {
            public string ID;
            public GameObject Prefab;
            public int PoolSize = 0;
        }

        [SerializeField] private List<AssetEntry> _entries = new List<AssetEntry>();

        private Dictionary<string, AssetEntry> _cache;

        public void Initialize()
        {
            _cache = new Dictionary<string, AssetEntry>();

            foreach (AssetEntry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.ID))
                {
                    DebugSystem.LogWarning("AssetEntry con ID vacio ignorada.", "Resources", "AssetReferenceDatabase");
                    continue;
                }

                if (_cache.ContainsKey(entry.ID))
                {
                    DebugSystem.LogWarning($"ID duplicado ignorado: {entry.ID}", "Resources", "AssetReferenceDatabase");
                    continue;
                }

                _cache[entry.ID] = entry;
            }

            DebugSystem.Log($"AssetDatabase initialized: {_cache.Count} assets registered.", "Resources", "AssetReferenceDatabase");
        }

        public AssetEntry GetEntry(string id)
        {
            if (_cache == null) Initialize();
            return _cache.ContainsKey(id) ? _cache[id] : null;
        }

        public GameObject GetPrefab(string id)
        {
            AssetEntry entry = GetEntry(id);

            if (entry == null)
            {
                DebugSystem.LogWarning($"Asset no encontrado: {id}", "Resources", "AssetReferenceDatabase");
                return null;
            }

            return entry.Prefab;
        }

        public List<AssetEntry> GetPoolableEntries()
        {
            List<AssetEntry> poolable = new List<AssetEntry>();

            foreach (AssetEntry entry in _entries)
            {
                if (entry.PoolSize > 0 && entry.Prefab != null)
                    poolable.Add(entry);
            }

            return poolable;
        }
    }
}
