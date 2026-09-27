namespace Infra2DAction
{
    public abstract class BaseEvent
    {
        public float RaisedTime { get; private set; }
        public string SourceID { get; private set; }

        protected BaseEvent(string sourceID = "Unknown")
        {
            RaisedTime = UnityEngine.Time.time;
            SourceID = sourceID;
        }
    }
}
