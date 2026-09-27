using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "ResourceConsumptionTable", menuName = "Infraestructura2DAction/Player/Resource Consumption Table")]
    public class ResourceConsumptionTable : ScriptableObject
    {
        [Serializable]
        public class CostEntry
        {
            public string Key;
            public float Cost;
        }

        [SerializeField]
        private List<CostEntry> _entries = new List<CostEntry>
        {
            new CostEntry { Key = "Heal", Cost = 20f },
            new CostEntry { Key = "ShieldPerSecond", Cost = 5f }
        };

        private Dictionary<string, float> _cache;

        public float GetCost(string key)
        {
            if (_cache == null) BuildCache();

            if (_cache.TryGetValue(key, out float cost))
                return cost;

            DebugSystem.LogWarning($"Coste de recurso no encontrado: {key}", "Resource", "ResourceConsumptionTable");
            return 0f;
        }

        private void BuildCache()
        {
            _cache = new Dictionary<string, float>();
            foreach (CostEntry entry in _entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                    _cache[entry.Key] = entry.Cost;
            }
        }
    }
}
