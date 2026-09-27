using UnityEngine;

namespace Infra2DAction
{
    public class SettingsSystem : MonoBehaviour
    {
        private AudioSettings _audio;
        private ControlsSettings _controls;
        private AccessibilitySettings _accessibility;

        private SettingsSaveData _saveData;

        public void Initialize()
        {
            _audio = new AudioSettings();
            _controls = new ControlsSettings();
            _accessibility = new AccessibilitySettings();
            _saveData = new SettingsSaveData();

            LoadFromData(_saveData);

            DebugSystem.Log("SettingsSystem initialized.", "Settings", "SettingsSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SettingsChangedEvent>(OnSettingChanged);
            EventBus.Subscribe<SettingsRequestedEvent>(OnSettingsRequested);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SettingsChangedEvent>(OnSettingChanged);
            EventBus.Unsubscribe<SettingsRequestedEvent>(OnSettingsRequested);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        private void OnSettingsRequested(SettingsRequestedEvent e)
        {
            if (_audio == null) return;

            NotifyAudioChanged();
            NotifyControlsChanged();
            NotifyAccessibilityChanged();
        }

        public AudioSettings Audio => _audio;
        public ControlsSettings Controls => _controls;
        public AccessibilitySettings Accessibility => _accessibility;

        public SettingsSaveData GetSaveData()
        {
            _audio.Save(_saveData);
            _controls.Save(_saveData);
            _accessibility.Save(_saveData);
            return _saveData;
        }

        private void OnSettingChanged(SettingsChangedEvent e)
        {
            bool changed = false;

            switch (e.Category)
            {
                case "Audio":
                    changed = _audio.TrySetValue(e.Key, e.Value);
                    if (changed) NotifyAudioChanged();
                    break;

                case "Controls":
                    changed = _controls.TrySetValue(e.Key, e.Value);
                    if (changed) NotifyControlsChanged();
                    break;

                case "Accessibility":
                    changed = _accessibility.TrySetValue(e.Key, e.Value);
                    if (changed) NotifyAccessibilityChanged();
                    break;

                default:
                    DebugSystem.LogWarning($"Categoria desconocida: {e.Category}", "Settings", "SettingsSystem");
                    break;
            }

            if (changed)
                EventBus.Raise(new SaveRequestedEvent("SettingsSystem"));
        }

        private void NotifyAudioChanged()
        {
            EventBus.Raise(new AudioSettingsChangedEvent(
                _audio.MasterVolume,
                _audio.MusicVolume,
                _audio.SFXVolume,
                _audio.UIVolume
            ));
        }

        private void NotifyControlsChanged()
        {
            EventBus.Raise(new ControlsUpdatedEvent(_controls.VibrationEnabled));
        }

        private void NotifyAccessibilityChanged()
        {
            EventBus.Raise(new AccessibilitySettingsChangedEvent(
                _accessibility.ColorblindMode,
                _accessibility.UIScale,
                _accessibility.DialogueSpeed
            ));
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            if (e.Data?.Settings == null)
            {
                DebugSystem.LogWarning("SaveLoadedEvent sin Settings validos.", "Settings", "SettingsSystem");
                return;
            }

            LoadFromData(e.Data.Settings);

            NotifyAudioChanged();
            NotifyControlsChanged();
            NotifyAccessibilityChanged();

            DebugSystem.Log("Settings loaded from save.", "Settings", "SettingsSystem");
        }

        private void LoadFromData(SettingsSaveData data)
        {
            _audio.Load(data);
            _controls.Load(data);
            _accessibility.Load(data);
        }
    }
}
