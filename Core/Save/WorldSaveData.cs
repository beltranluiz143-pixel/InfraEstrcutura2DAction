using System;
using System.Collections.Generic;

namespace Infra2DAction
{
    [Serializable]
    public class WorldSaveData
    {
        [Serializable]
        public class FlagEntry
        {
            public string Key;
            public bool Value;

            public FlagEntry(string key, bool value)
            {
                Key = key;
                Value = value;
            }
        }

        [Serializable]
        public class VariableEntry
        {
            public string Key;
            public float Value;

            public VariableEntry(string key, float value)
            {
                Key = key;
                Value = value;
            }
        }

        public List<FlagEntry> GlobalFlags = new List<FlagEntry>();
        public List<VariableEntry> GameVariables = new List<VariableEntry>();
        public List<string> VisitedScenes = new List<string>();
        public List<MapPinSaveData> Pins = new List<MapPinSaveData>();

        public bool GetFlag(string key, bool defaultValue = false)
        {
            FlagEntry entry = GlobalFlags.Find(f => f.Key == key);
            return entry != null ? entry.Value : defaultValue;
        }

        public void SetFlag(string key, bool value)
        {
            FlagEntry entry = GlobalFlags.Find(f => f.Key == key);

            if (entry != null)
                entry.Value = value;
            else
                GlobalFlags.Add(new FlagEntry(key, value));
        }

        public float GetVariable(string key, float defaultValue = 0f)
        {
            VariableEntry entry = GameVariables.Find(v => v.Key == key);
            return entry != null ? entry.Value : defaultValue;
        }

        public void SetVariable(string key, float value)
        {
            VariableEntry entry = GameVariables.Find(v => v.Key == key);

            if (entry != null)
                entry.Value = value;
            else
                GameVariables.Add(new VariableEntry(key, value));
        }

        public bool HasVisitedScene(string sceneName)
            => VisitedScenes.Contains(sceneName);

        public void MarkSceneVisited(string sceneName)
        {
            if (!VisitedScenes.Contains(sceneName))
                VisitedScenes.Add(sceneName);
        }
    }
}
