using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class UICore : MonoBehaviour
    {
        [Header("Registered Panels (la clave es el PanelID de cada UIPanel)")]
        [SerializeField] private List<UIPanel> _panels = new List<UIPanel>();

        private Dictionary<string, UIPanel> _panelLookup = new Dictionary<string, UIPanel>();
        private Stack<UIPanel> _panelStack = new Stack<UIPanel>();

        public void Initialize()
        {
            _panelLookup.Clear();
            _panelStack.Clear();

            foreach (UIPanel panel in _panels)
            {
                if (panel == null || string.IsNullOrEmpty(panel.PanelID))
                {
                    DebugSystem.LogWarning("Panel nulo o sin PanelID ignorado.", "UI", "UICore");
                    continue;
                }

                if (_panelLookup.ContainsKey(panel.PanelID))
                {
                    DebugSystem.LogWarning($"PanelID duplicado ignorado: {panel.PanelID}", "UI", "UICore");
                    continue;
                }

                panel.gameObject.SetActive(false);
                _panelLookup[panel.PanelID] = panel;
            }

            DebugSystem.Log($"UICore initialized. {_panelLookup.Count} panels registered.", "UI", "UICore");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<UIOpenPanelRequestEvent>(OnOpenPanelRequest);
            EventBus.Subscribe<UIClosePanelRequestEvent>(OnClosePanelRequest);
            EventBus.Subscribe<UIPanelCloseRequestEvent>(OnClosePanelByIDRequest);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<UIOpenPanelRequestEvent>(OnOpenPanelRequest);
            EventBus.Unsubscribe<UIClosePanelRequestEvent>(OnClosePanelRequest);
            EventBus.Unsubscribe<UIPanelCloseRequestEvent>(OnClosePanelByIDRequest);
        }

        private void OnOpenPanelRequest(UIOpenPanelRequestEvent e) => OpenPanel(e.PanelID);
        private void OnClosePanelRequest(UIClosePanelRequestEvent e) => CloseTopPanel();
        private void OnClosePanelByIDRequest(UIPanelCloseRequestEvent e) => CloseTopPanel();

        public void OpenPanel(string panelID)
        {
            if (!_panelLookup.TryGetValue(panelID, out UIPanel panel))
            {
                DebugSystem.LogWarning($"Panel no encontrado: {panelID}", "UI", "UICore");
                return;
            }

            if (_panelStack.Count > 0)
            {
                UIPanel current = _panelStack.Peek();
                if (current != panel)
                    current.Close();
            }

            panel.Open();
            _panelStack.Push(panel);
        }

        public void CloseTopPanel()
        {
            if (_panelStack.Count == 0) return;

            UIPanel closing = _panelStack.Pop();
            closing.Close();

            if (_panelStack.Count > 0)
                _panelStack.Peek().Open();
        }

        public void CloseAllPanels()
        {
            while (_panelStack.Count > 0)
                _panelStack.Pop().Close();
        }

        public bool IsAnyPanelOpen() => _panelStack.Count > 0;
    }
}
