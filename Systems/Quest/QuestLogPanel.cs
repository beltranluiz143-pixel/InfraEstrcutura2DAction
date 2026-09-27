using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Infra2DAction
{
    public class QuestLogPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _questEntryContainer;
        [SerializeField] private GameObject _questEntryPrefab;

        [Header("Colors")]
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _completedColor = Color.green;

        private QuestSystem _questSystem;

        public void Initialize(QuestSystem questSystem)
        {
            _questSystem = questSystem;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<QuestStartedEvent>(OnQuestChanged);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestChanged);
            Rebuild();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<QuestStartedEvent>(OnQuestChanged);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestChanged);
        }

        private void OnQuestChanged(BaseEvent e) => Rebuild();

        private void Rebuild()
        {
            if (_questEntryContainer == null || _questEntryPrefab == null) return;

            foreach (Transform child in _questEntryContainer)
                Destroy(child.gameObject);

            if (_questSystem == null) return;

            List<QuestSystem.QuestLogEntry> log = _questSystem.GetQuestLog();

            foreach (QuestSystem.QuestLogEntry entry in log)
            {
                GameObject entryObject = Instantiate(_questEntryPrefab, _questEntryContainer);
                TMP_Text text = entryObject.GetComponentInChildren<TMP_Text>();
                if (text == null) continue;

                text.text = entry.Data.DisplayText;
                text.color = entry.Completed ? _completedColor : _activeColor;
            }
        }
    }
}
