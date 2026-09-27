using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class ConsoleOverlay : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _maxVisibleEntries = 10;
        [SerializeField] private bool _visibleOnStart = true;

        private readonly List<string> _entries = new List<string>();
        private bool _isVisible;
        private GUIStyle _labelStyle;
        private bool _styleInitialized;

        private void Start()
        {
            _isVisible = _visibleOnStart;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DebugLogEvent>(OnDebugLog);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DebugLogEvent>(OnDebugLog);
        }

        private void OnDebugLog(DebugLogEvent e)
        {
            if (e.Severity == DebugLogEvent.LogSeverity.Warning ||
                e.Severity == DebugLogEvent.LogSeverity.Error)
            {
                AddEntry($"[{e.Category}] {e.Message}");
            }
        }

        public void AddEntry(string entry)
        {
            _entries.Add(entry);

            if (_entries.Count > _maxVisibleEntries)
                _entries.RemoveAt(0);
        }

        public void ClearEntries() => _entries.Clear();

        public void SetVisible(bool visible) => _isVisible = visible;

        public void Toggle() => _isVisible = !_isVisible;

        private void OnGUI()
        {
            if (!_isVisible) return;

            InitializeStyle();

            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(5, 5, 420, _maxVisibleEntries * 18 + 10), Texture2D.whiteTexture);
            GUI.color = Color.white;

            for (int i = 0; i < _entries.Count; i++)
            {
                GUI.Label(new Rect(10, 10 + i * 18, 410, 18), _entries[i], _labelStyle);
            }
        }

        private void InitializeStyle()
        {
            if (_styleInitialized) return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = Color.green },
                fontStyle = FontStyle.Bold
            };

            _styleInitialized = true;
        }
    }
}
