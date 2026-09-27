using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class EventDebugger : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _maxHistorySize = 50;

        private readonly List<EventDebugData> _history = new List<EventDebugData>();
        private ConsoleOverlay _overlay;

        private void Awake()
        {
            _overlay = GetComponent<ConsoleOverlay>();
            EventBus.RegisterDebugger(this);
            EventBus.Subscribe<DebugLogEvent>(OnDebugLog);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DebugLogEvent>(OnDebugLog);
        }

        public void RegisterEvent(string eventName, string sourceID, int listenerCount)
        {
            var data = new EventDebugData(eventName, sourceID, Time.time, listenerCount);
            _history.Add(data);

            if (_history.Count > _maxHistorySize)
                _history.RemoveAt(0);

            _overlay?.AddEntry(data.ToString());

#if UNITY_EDITOR
            Debug.Log($"[EVENTDEBUGGER] {data}");
#endif
        }

        private void OnDebugLog(DebugLogEvent e)
        {
            string prefix = e.Severity switch
            {
                DebugLogEvent.LogSeverity.Warning => "[WARN]",
                DebugLogEvent.LogSeverity.Error => "[ERROR]",
                _ => "[INFO]"
            };

            string message = $"{prefix} [{e.Category}] {e.Message}";

            switch (e.Severity)
            {
                case DebugLogEvent.LogSeverity.Warning: Debug.LogWarning(message); break;
                case DebugLogEvent.LogSeverity.Error: Debug.LogError(message); break;
                default: Debug.Log(message); break;
            }

            _overlay?.AddEntry(message);
        }

        public IReadOnlyList<EventDebugData> GetHistory() => _history.AsReadOnly();

        public void ClearHistory()
        {
            _history.Clear();
            _overlay?.ClearEntries();
        }
    }
}
