using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "MapPinIconDatabase", menuName = "Infraestructura2DAction/Content/Map Pin Icon Database")]
    public class MapPinIconDatabase : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string IconID;
            public string DisplayName;
            public Sprite Icon;
        }

        public List<Entry> Icons = new List<Entry>();

        public Sprite GetIcon(string iconID)
        {
            if (string.IsNullOrEmpty(iconID)) return null;
            Entry entry = Icons.Find(e => e.IconID == iconID);
            return entry != null ? entry.Icon : null;
        }
    }
}
