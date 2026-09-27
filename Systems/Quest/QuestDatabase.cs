using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "Infraestructura2DAction/Content/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        [SerializeField] private List<QuestData> _quests = new List<QuestData>();
        private Dictionary<string, QuestData> _cache;

        public QuestData GetQuest(string questID)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<string, QuestData>();

                foreach (QuestData entry in _quests)
                    if (entry != null && !string.IsNullOrEmpty(entry.QuestID))
                        _cache[entry.QuestID] = entry;
            }

            return _cache.TryGetValue(questID, out QuestData quest) ? quest : null;
        }
    }
}
