using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Infraestructura2DAction/Content/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new List<ItemData>();
        private Dictionary<string, ItemData> _cache;

        public ItemData GetItem(string itemID)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<string, ItemData>();

                foreach (ItemData entry in _items)
                    if (entry != null && !string.IsNullOrEmpty(entry.ItemID))
                        _cache[entry.ItemID] = entry;
            }

            return _cache.TryGetValue(itemID, out ItemData item) ? item : null;
        }
    }
}
