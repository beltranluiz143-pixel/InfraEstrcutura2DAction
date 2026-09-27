using UnityEngine;
using UnityEngine.InputSystem;

namespace Infra2DAction
{
    public class DebugSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EventDebugger _eventDebugger;
        [SerializeField] private ConsoleOverlay _consoleOverlay;

        [Header("Configuration")]
        [SerializeField] private bool _enableOnStart = true;
        [SerializeField] private Key _toggleOverlayKey = Key.F1;
        [SerializeField] private Key _printStatusKey = Key.F2;
        [SerializeField] private Key _clearHistoryKey = Key.F3;

        private void Start()
        {
            if (!_enableOnStart)
            {
                if (_consoleOverlay != null) _consoleOverlay.SetVisible(false);
                return;
            }

            EventBus.Raise(new DebugLogEvent(
                message: "DebugSystem initialized.",
                severity: DebugLogEvent.LogSeverity.Info,
                category: "Core",
                sourceID: "DebugSystem"
            ));
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard[_toggleOverlayKey].wasPressedThisFrame && _consoleOverlay != null)
                _consoleOverlay.Toggle();
            if (keyboard[_printStatusKey].wasPressedThisFrame)
                EventBus.PrintStatus();
            if (keyboard[_clearHistoryKey].wasPressedThisFrame && _eventDebugger != null)
                _eventDebugger.ClearHistory();
#endif
        }

        public static void Log(string message, string category = "General", string sourceID = "Unknown")
        {
            EventBus.Raise(new DebugLogEvent(message, DebugLogEvent.LogSeverity.Info, category, sourceID));
        }

        public static void LogWarning(string message, string category = "General", string sourceID = "Unknown")
        {
            EventBus.Raise(new DebugLogEvent(message, DebugLogEvent.LogSeverity.Warning, category, sourceID));
        }

        public static void LogError(string message, string category = "General", string sourceID = "Unknown")
        {
            EventBus.Raise(new DebugLogEvent(message, DebugLogEvent.LogSeverity.Error, category, sourceID));
        }

        public EventDebugger GetEventDebugger()
        {
            return _eventDebugger;
        }
    }
}
