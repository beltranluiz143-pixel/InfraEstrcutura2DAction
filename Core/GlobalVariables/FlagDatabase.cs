using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "FlagDatabase_", menuName = "Infraestructura2DAction/Flags/Flag Database")]
    public class FlagDatabase : ScriptableObject
    {
        [Serializable]
        public class FlagEntry
        {
            public string ID;
            public string Description;
            public bool DefaultValue;
        }

        [Header("Modo 1 - Prefijo/Sufijo (dejar vacio para Modo 2)")]
        public string FamilyPrefix = "";
        public string FamilySuffix = "";

        [Header("Flags")]
        [SerializeField] private List<FlagEntry> _entries = new List<FlagEntry>();

        private Dictionary<string, FlagEntry> _cache;

        public void Initialize()
        {
            _cache = new Dictionary<string, FlagEntry>();

            foreach (FlagEntry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.ID))
                {
                    DebugSystem.LogWarning("FlagEntry con ID vacio ignorada.", "GlobalVars", "FlagDatabase");
                    continue;
                }

                string fullKey = BuildFlagKey(entry.ID);

                if (_cache.ContainsKey(fullKey))
                {
                    DebugSystem.LogWarning($"Flag duplicada ignorada: {fullKey}", "GlobalVars", "FlagDatabase");
                    continue;
                }

                _cache[fullKey] = entry;
            }
        }

        public string BuildFlagKey(string entryID)
        {
            if (string.IsNullOrEmpty(FamilyPrefix) && string.IsNullOrEmpty(FamilySuffix))
                return entryID;
            return $"{FamilyPrefix}{entryID}{FamilySuffix}";
        }

        public List<(string Key, bool DefaultValue)> GetAllFlags()
        {
            var result = new List<(string, bool)>();

            foreach (FlagEntry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;
                result.Add((BuildFlagKey(entry.ID), entry.DefaultValue));
            }

            return result;
        }

        public bool ContainsFlag(string fullKey)
        {
            if (_cache == null) Initialize();
            return _cache.ContainsKey(fullKey);
        }

        public FlagEntry GetEntry(string fullKey)
        {
            if (_cache == null) Initialize();
            return _cache.TryGetValue(fullKey, out FlagEntry entry) ? entry : null;
        }

        public int Count => _entries?.Count ?? 0;
    }
}
