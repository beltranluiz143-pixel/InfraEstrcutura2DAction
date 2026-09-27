namespace Infra2DAction
{
    public class AccessibilitySettings
    {
        public bool ColorblindMode { get; private set; } = false;
        public float UIScale { get; private set; } = 1f;
        public float DialogueSpeed { get; private set; } = 1f;

        public void Load(SettingsSaveData data)
        {
            ColorblindMode = data.ColorblindMode;
            UIScale = data.UIScale;
            DialogueSpeed = data.DialogueSpeed;
        }

        public void Save(SettingsSaveData data)
        {
            data.ColorblindMode = ColorblindMode;
            data.UIScale = UIScale;
            data.DialogueSpeed = DialogueSpeed;
        }

        public bool TrySetValue(string key, float value)
        {
            switch (key)
            {
                case "ColorblindMode": ColorblindMode = value > 0.5f; return true;
                case "UIScale": UIScale = UnityEngine.Mathf.Clamp(value, 0.5f, 2f); return true;
                case "DialogueSpeed": DialogueSpeed = UnityEngine.Mathf.Clamp(value, 0.25f, 3f); return true;
                default:
                    DebugSystem.LogWarning($"AccessibilitySettings: clave desconocida '{key}'", "Settings", "AccessibilitySettings");
                    return false;
            }
        }
    }
}
