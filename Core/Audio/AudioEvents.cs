namespace Infra2DAction
{
    public class MusicChangedEvent : BaseEvent
    {
        public string MusicID { get; private set; }

        public MusicChangedEvent(string musicID, string sourceID = "MusicManager")
            : base(sourceID)
        {
            MusicID = musicID;
        }
    }

    public class MusicPlayRequestEvent : BaseEvent
    {
        public string MusicID { get; private set; }

        public MusicPlayRequestEvent(string musicID, string sourceID = "Unknown")
            : base(sourceID)
        {
            MusicID = musicID;
        }
    }

    public class SnapshotChangedEvent : BaseEvent
    {
        public string SnapshotName { get; private set; }

        public SnapshotChangedEvent(string snapshotName, string sourceID = "SnapshotController")
            : base(sourceID)
        {
            SnapshotName = snapshotName;
        }
    }

    public class SFXPlayRequestEvent : BaseEvent
    {
        public string SoundID { get; private set; }
        public UnityEngine.Vector3 Position { get; private set; }

        public SFXPlayRequestEvent(string soundID, UnityEngine.Vector3 position, string sourceID = "Unknown")
            : base(sourceID)
        {
            SoundID = soundID;
            Position = position;
        }
    }
}
