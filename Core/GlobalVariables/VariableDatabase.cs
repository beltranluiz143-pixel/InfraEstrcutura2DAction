using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "VariableDatabase_", menuName = "Infraestructura2DAction/Flags/Variable Database")]
    public class VariableDatabase : ScriptableObject
    {
        [Serializable]
        public class VariableEntry
        {
            public string ID;
            public string Description;
            public float DefaultValue;
        }

        [SerializeField] private List<VariableEntry> _entries = new List<VariableEntry>();

        public List<(string Key, float DefaultValue)> GetAllVariables()
        {
            var result = new List<(string, float)>();

            foreach (VariableEntry entry in _entries)
            {
                if (!string.IsNullOrEmpty(entry.ID))
                    result.Add((entry.ID, entry.DefaultValue));
            }

            return result;
        }
    }
}
