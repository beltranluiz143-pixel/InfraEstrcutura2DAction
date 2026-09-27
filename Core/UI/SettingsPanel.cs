using System;
using UnityEngine;
using UnityEngine.UI;

namespace Infra2DAction
{
    public class SettingsPanel : UIPanel
    {
        [Serializable]
        public class VolumeButtons
        {
            public Button Low;
            public Button Med;
            public Button High;
        }

        [Serializable]
        public class ToggleButtons
        {
            public Button Off;
            public Button On;
        }

        [Header("Volume")]
        [SerializeField] private VolumeButtons _master;
        [SerializeField] private VolumeButtons _music;
        [SerializeField] private VolumeButtons _sfx;
        [SerializeField] private VolumeButtons _ui;

        [Header("Toggles")]
        [SerializeField] private ToggleButtons _colorblind;
        [SerializeField] private ToggleButtons _vibration;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Volume Presets")]
        [SerializeField] private float _lowValue = 0.3f;
        [SerializeField] private float _medValue = 0.6f;
        [SerializeField] private float _highValue = 1.0f;

        private void Awake()
        {
            BindVolume(_master, "MasterVolume");
            BindVolume(_music, "MusicVolume");
            BindVolume(_sfx, "SFXVolume");
            BindVolume(_ui, "UIVolume");

            BindToggle(_colorblind, "Accessibility", "ColorblindMode");
            BindToggle(_vibration, "Controls", "VibrationEnabled");

            if (_backButton != null) _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AudioSettingsChangedEvent>(OnAudioSettingsChanged);
            EventBus.Subscribe<AccessibilitySettingsChangedEvent>(OnAccessibilitySettingsChanged);
            EventBus.Subscribe<ControlsUpdatedEvent>(OnControlsUpdated);

            EventBus.Raise(new SettingsRequestedEvent("SettingsPanel"));
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AudioSettingsChangedEvent>(OnAudioSettingsChanged);
            EventBus.Unsubscribe<AccessibilitySettingsChangedEvent>(OnAccessibilitySettingsChanged);
            EventBus.Unsubscribe<ControlsUpdatedEvent>(OnControlsUpdated);
        }

        private void BindVolume(VolumeButtons group, string key)
        {
            if (group == null) return;
            if (group.Low != null) group.Low.onClick.AddListener(() => RaiseSetting("Audio", key, _lowValue));
            if (group.Med != null) group.Med.onClick.AddListener(() => RaiseSetting("Audio", key, _medValue));
            if (group.High != null) group.High.onClick.AddListener(() => RaiseSetting("Audio", key, _highValue));
        }

        private void BindToggle(ToggleButtons group, string category, string key)
        {
            if (group == null) return;
            if (group.Off != null) group.Off.onClick.AddListener(() => RaiseSetting(category, key, 0f));
            if (group.On != null) group.On.onClick.AddListener(() => RaiseSetting(category, key, 1f));
        }

        private void OnAudioSettingsChanged(AudioSettingsChangedEvent e)
        {
            HighlightVolume(_master, e.MasterVolume);
            HighlightVolume(_music, e.MusicVolume);
            HighlightVolume(_sfx, e.SFXVolume);
            HighlightVolume(_ui, e.UIVolume);
        }

        private void OnAccessibilitySettingsChanged(AccessibilitySettingsChangedEvent e)
        {
            HighlightToggle(_colorblind, e.ColorblindMode);
        }

        private void OnControlsUpdated(ControlsUpdatedEvent e)
        {
            HighlightToggle(_vibration, e.VibrationEnabled);
        }

        private void HighlightVolume(VolumeButtons group, float currentValue)
        {
            if (group == null || group.Low == null || group.Med == null || group.High == null) return;

            float distLow = Mathf.Abs(currentValue - _lowValue);
            float distMed = Mathf.Abs(currentValue - _medValue);
            float distHigh = Mathf.Abs(currentValue - _highValue);

            group.Low.interactable = true;
            group.Med.interactable = true;
            group.High.interactable = true;

            if (distLow <= distMed && distLow <= distHigh)
                group.Low.interactable = false;
            else if (distMed <= distLow && distMed <= distHigh)
                group.Med.interactable = false;
            else
                group.High.interactable = false;
        }

        private void HighlightToggle(ToggleButtons group, bool isOn)
        {
            if (group == null || group.Off == null || group.On == null) return;

            group.Off.interactable = isOn;
            group.On.interactable = !isOn;
        }

        private void RaiseSetting(string category, string key, float value)
        {
            EventBus.Raise(new SettingsChangedEvent(category, key, value, "SettingsPanel"));
        }

        private void OnBackClicked()
        {
            EventBus.Raise(new UIClosePanelRequestEvent("SettingsPanel"));
        }
    }
}
