namespace Infra2DAction
{
    public class ControlsSettings
    {
        public bool VibrationEnabled { get; private set; } = true;

        public void Load(SettingsSaveData data)
        {
            VibrationEnabled = data.VibrationEnabled;
        }

        public void Save(SettingsSaveData data)
        {
            data.VibrationEnabled = VibrationEnabled;
        }

        public bool TrySetValue(string key, float value)
        {
            switch (key)
            {
                case "VibrationEnabled": VibrationEnabled = value > 0.5f; return true;
                default:
                    DebugSystem.LogWarning($"ControlsSettings: clave desconocida '{key}'", "Settings", "ControlsSettings");
                    return false;
            }
        }
    }
}
