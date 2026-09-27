namespace Infra2DAction
{
    public class SaveRequestedEvent : BaseEvent
    {
        public SaveRequestedEvent(string sourceID = "Unknown")
            : base(sourceID) { }
    }

    public class CollectSaveDataEvent : BaseEvent
    {
        public CollectSaveDataEvent(string sourceID = "SaveSystem") : base(sourceID) { }
    }

    public class GameSavedEvent : BaseEvent
    {
        public GameSavedEvent(string sourceID = "SaveSystem")
            : base(sourceID) { }
    }

    public class SaveLoadedEvent : BaseEvent
    {
        public SaveData Data { get; private set; }

        public SaveLoadedEvent(SaveData data, string sourceID = "SaveSystem")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class CheckpointActivatedEvent : BaseEvent
    {
        public string SpawnPointID { get; private set; }

        public CheckpointActivatedEvent(string spawnPointID, string sourceID = "SavePoint")
            : base(sourceID)
        {
            SpawnPointID = spawnPointID;
        }
    }
}
