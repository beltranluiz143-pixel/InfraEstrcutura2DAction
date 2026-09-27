using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class UIStateController : MonoBehaviour
    {
        [Serializable]
        public class StatePanelMapping
        {
            public GameStateType State;
            public string PanelID;
        }

        [Header("State to Panel Mapping")]
        [SerializeField] private List<StatePanelMapping> _mappings = new List<StatePanelMapping>();

        [Header("References")]
        [SerializeField] private UICore _uiCore;

        private Dictionary<GameStateType, string> _stateToPanelID;

        public void Initialize()
        {
            if (_uiCore == null)
            {
                DebugSystem.LogError("UICore no asignado en UIStateController.", "UI", "UIStateController");
                return;
            }

            _stateToPanelID = new Dictionary<GameStateType, string>();

            foreach (StatePanelMapping mapping in _mappings)
            {
                if (string.IsNullOrEmpty(mapping.PanelID)) continue;
                _stateToPanelID[mapping.State] = mapping.PanelID;
            }

            DebugSystem.Log("UIStateController initialized.", "UI", "UIStateController");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (_uiCore == null || _stateToPanelID == null) return;

            if (_stateToPanelID.TryGetValue(e.NewState, out string panelID))
            {
                _uiCore.OpenPanel(panelID);
                return;
            }

            if (e.NewState == GameStateType.Gameplay || e.NewState == GameStateType.Dream)
                _uiCore.CloseAllPanels();
        }
    }
}
