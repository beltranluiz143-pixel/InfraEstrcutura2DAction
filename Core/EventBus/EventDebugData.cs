namespace Infra2DAction
{
    public class EventDebugData
    {
        public string EventName { get; private set; }
        public string SourceID { get; private set; }
        public float RaisedTime { get; private set; }
        public int ListenerCount { get; private set; }

        public EventDebugData(string eventName, string sourceID, float raisedTime, int listenerCount)
        {
            EventName = eventName;
            SourceID = sourceID;
            RaisedTime = raisedTime;
            ListenerCount = listenerCount;
        }

        public override string ToString()
        {
            return $"[{RaisedTime:F2}s] {EventName} | Source: {SourceID} | Listeners: {ListenerCount}";
        }
    }
}
