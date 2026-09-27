namespace Infra2DAction
{
    public class AudioSettings
    {
        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 0.8f;
        public float SFXVolume { get; private set; } = 1f;
        public float UIVolume { get; private set; } = 1f;

        public void Load(SettingsSaveData data)
        {
            MasterVolume = data.MasterVolume;
            MusicVolume = data.MusicVolume;
            SFXVolume = data.SFXVolume;
            UIVolume = data.UIVolume;
        }

        public void Save(SettingsSaveData data)
        {
            data.MasterVolume = MasterVolume;
            data.MusicVolume = MusicVolume;
            data.SFXVolume = SFXVolume;
            data.UIVolume = UIVolume;
        }

        public bool TrySetValue(string key, float value)
        {
            float clamped = UnityEngine.Mathf.Clamp01(value);

            switch (key)
            {
                case "MasterVolume": MasterVolume = clamped; return true;
                case "MusicVolume": MusicVolume = clamped; return true;
                case "SFXVolume": SFXVolume = clamped; return true;
                case "UIVolume": UIVolume = clamped; return true;
                default:
                    DebugSystem.LogWarning($"AudioSettings: clave desconocida '{key}'", "Settings", "AudioSettings");
                    return false;
            }
        }
    }
}
