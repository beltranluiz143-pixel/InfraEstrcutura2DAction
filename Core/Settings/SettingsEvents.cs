namespace Infra2DAction
{
    public class SettingsRequestedEvent : BaseEvent
    {
        public SettingsRequestedEvent(string sourceID = "Unknown") : base(sourceID) { }
    }

    public class SettingsChangedEvent : BaseEvent
    {
        public string Category { get; private set; }
        public string Key { get; private set; }
        public float Value { get; private set; }

        public SettingsChangedEvent(string category, string key, float value, string sourceID = "SettingsSystem")
            : base(sourceID)
        {
            Category = category;
            Key = key;
            Value = value;
        }
    }

    public class AudioSettingsChangedEvent : BaseEvent
    {
        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public float UIVolume { get; private set; }

        public AudioSettingsChangedEvent(float master, float music, float sfx, float ui, string sourceID = "SettingsSystem")
            : base(sourceID)
        {
            MasterVolume = master;
            MusicVolume = music;
            SFXVolume = sfx;
            UIVolume = ui;
        }
    }

    public class ControlsUpdatedEvent : BaseEvent
    {
        public bool VibrationEnabled { get; private set; }

        public ControlsUpdatedEvent(bool vibrationEnabled, string sourceID = "SettingsSystem")
            : base(sourceID)
        {
            VibrationEnabled = vibrationEnabled;
        }
    }

    public class AccessibilitySettingsChangedEvent : BaseEvent
    {
        public bool ColorblindMode { get; private set; }
        public float UIScale { get; private set; }
        public float DialogueSpeed { get; private set; }

        public AccessibilitySettingsChangedEvent(bool colorblind, float uiScale, float dialogueSpeed, string sourceID = "SettingsSystem")
            : base(sourceID)
        {
            ColorblindMode = colorblind;
            UIScale = uiScale;
            DialogueSpeed = dialogueSpeed;
        }
    }
}
