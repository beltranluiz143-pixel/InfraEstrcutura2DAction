using System;

namespace Infra2DAction
{
    [Serializable]
    public class SaveData
    {
        public PlayerSaveData Player = new PlayerSaveData();
        public WorldSaveData World = new WorldSaveData();
        public SettingsSaveData Settings = new SettingsSaveData();
        public QuestSaveData Quests = new QuestSaveData();

        public int SaveVersion = 1;
        public string LastSaveTime = "";
        public float TotalPlaytimeSeconds = 0f;
    }
}
