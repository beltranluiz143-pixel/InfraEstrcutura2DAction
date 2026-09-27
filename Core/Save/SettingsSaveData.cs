using System;

namespace Infra2DAction
{
    [Serializable]
    public class SettingsSaveData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SFXVolume = 1f;
        public float UIVolume = 1f;

        public bool VibrationEnabled = true;

        public bool ColorblindMode = false;
        public float UIScale = 1f;
        public float DialogueSpeed = 1f;
    }
}
