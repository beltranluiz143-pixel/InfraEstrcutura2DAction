using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Infra2DAction
{
    public class SaveSlotPanel : UIPanel
    {
        [Serializable]
        public class SlotView
        {
            public Button Button;
            public TMP_Text Label;
        }

        [Header("Slots (el indice 0 es el slot 1)")]
        [SerializeField] private List<SlotView> _slots = new List<SlotView>();

        [Header("Texto ({0} = slot, {1} = minutos jugados)")]
        [SerializeField] private string _emptySlotFormat = "Slot {0} - Vacio";
        [SerializeField] private string _filledSlotFormat = "Slot {0} - {1} min";
        [SerializeField] private string _overwriteSuffix = " (se borrara)";

        [Header("Inicio de partida")]
        [SerializeField] private SceneReference _newGameScene;

        private SaveSystem _saveSystem;
        private bool _overwriteMode;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
        }

        private void Awake()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                int slotNumber = i + 1;
                if (_slots[i].Button != null)
                    _slots[i].Button.onClick.AddListener(() => OnSlotSelected(slotNumber));
            }

            EventBus.Subscribe<SaveSlotPanelRequestEvent>(OnRequested);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SaveSlotPanelRequestEvent>(OnRequested);
        }

        private void OnRequested(SaveSlotPanelRequestEvent e)
        {
            _overwriteMode = e.OverwriteMode;
            EventBus.Raise(new UIOpenPanelRequestEvent(PanelID, "SaveSlotPanel"));
        }

        public override void Open()
        {
            base.Open();
            RefreshSlotLabels();
        }

        private void RefreshSlotLabels()
        {
            if (_saveSystem == null) return;

            for (int i = 0; i < _slots.Count; i++)
                SetLabel(_slots[i].Label, i + 1);
        }

        private void SetLabel(TMP_Text label, int slot)
        {
            if (label == null) return;

            if (_saveSystem.IsSlotEmpty(slot))
            {
                label.text = string.Format(_emptySlotFormat, slot);
                return;
            }

            SaveSystem.SaveSlotSummary summary = _saveSystem.GetSlotSummary(slot);
            int minutes = Mathf.FloorToInt(summary.TotalPlaytimeSeconds / 60f);
            string suffix = _overwriteMode ? _overwriteSuffix : "";
            label.text = string.Format(_filledSlotFormat, slot, minutes) + suffix;
        }

        private void OnSlotSelected(int slot)
        {
            if (_saveSystem == null) return;

            if (_overwriteMode && !_saveSystem.IsSlotEmpty(slot))
                _saveSystem.DeleteSlot(slot);

            _saveSystem.SelectSlot(slot);

            EventBus.Raise(new UIClosePanelRequestEvent("SaveSlotPanel"));
            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Gameplay, "SaveSlotPanel"));
            EventBus.Raise(new StartGameRequestEvent(_newGameScene, "SaveSlotPanel"));
        }
    }
}
