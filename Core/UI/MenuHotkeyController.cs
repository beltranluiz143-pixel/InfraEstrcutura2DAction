using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class MenuHotkeyController : MonoBehaviour
    {
        public enum MenuHotkey { Inventory, Map }

        [Serializable]
        public class MenuEntry
        {
            public MenuHotkey Hotkey;
            public string PanelID;
        }

        [Header("Atajo -> Panel")]
        [SerializeField] private List<MenuEntry> _entries = new List<MenuEntry>();

        private string _openPanelID;
        private GameStateType _currentState = GameStateType.Gameplay;

        private void OnEnable()
        {
            EventBus.Subscribe<OpenInventoryEvent>(OnOpenInventory);
            EventBus.Subscribe<OpenMapEvent>(OnOpenMap);
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Subscribe<UIPanelClosedEvent>(OnPanelClosed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OpenInventoryEvent>(OnOpenInventory);
            EventBus.Unsubscribe<OpenMapEvent>(OnOpenMap);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Unsubscribe<UIPanelClosedEvent>(OnPanelClosed);
        }

        private void OnOpenInventory(OpenInventoryEvent e) => OnHotkey(MenuHotkey.Inventory);
        private void OnOpenMap(OpenMapEvent e) => OnHotkey(MenuHotkey.Map);

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            _currentState = e.NewState;
            if (e.NewState != GameStateType.Menu) _openPanelID = null;
        }

        private void OnHotkey(MenuHotkey hotkey)
        {
            MenuEntry entry = _entries.Find(x => x.Hotkey == hotkey);
            if (entry == null || string.IsNullOrEmpty(entry.PanelID)) return;

            if (_openPanelID == entry.PanelID)
            {
                EventBus.Raise(new UIClosePanelRequestEvent("MenuHotkeyController"));
                return;
            }

            if (_currentState != GameStateType.Gameplay) return;

            _openPanelID = entry.PanelID;
            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Menu, "MenuHotkeyController"));
            EventBus.Raise(new UIOpenPanelRequestEvent(entry.PanelID, "MenuHotkeyController"));
        }

        private void OnPanelClosed(UIPanelClosedEvent e)
        {
            if (_openPanelID == null || e.PanelID != _openPanelID) return;

            _openPanelID = null;

            if (_currentState == GameStateType.Menu)
                EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Gameplay, "MenuHotkeyController"));
        }
    }
}
