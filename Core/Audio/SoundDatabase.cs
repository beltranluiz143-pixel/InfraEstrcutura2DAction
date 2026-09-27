using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "SoundDatabase", menuName = "Infraestructura2DAction/Audio/Sound Database")]
    public class SoundDatabase : ScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            public string ID;
            public AudioClip Clip;
            [Range(0f, 1f)] public float DefaultVolume = 1f;
            public bool IsMusic;
        }

        [SerializeField] private List<SoundEntry> _entries = new List<SoundEntry>();

        private Dictionary<string, SoundEntry> _cache;

        public void Initialize()
        {
            _cache = new Dictionary<string, SoundEntry>();

            foreach (SoundEntry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.ID) || entry.Clip == null)
                {
                    DebugSystem.LogWarning($"SoundEntry invalida ignorada: {entry.ID}", "Audio", "SoundDatabase");
                    continue;
                }

                _cache[entry.ID] = entry;
            }

            DebugSystem.Log($"SoundDatabase initialized: {_cache.Count} sounds.", "Audio", "SoundDatabase");
        }

        public SoundEntry GetEntry(string soundID)
        {
            if (_cache == null) Initialize();

            if (_cache.TryGetValue(soundID, out SoundEntry entry))
                return entry;

            DebugSystem.LogWarning($"Sound no encontrado: {soundID}", "Audio", "SoundDatabase");
            return null;
        }
    }
}
