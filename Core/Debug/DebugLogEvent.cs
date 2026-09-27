namespace Infra2DAction
{
    public class DebugLogEvent : BaseEvent
    {
        public enum LogSeverity { Info, Warning, Error }

        public string Message { get; private set; }
        public LogSeverity Severity { get; private set; }
        public string Category { get; private set; }

        public DebugLogEvent(string message,
                             LogSeverity severity = LogSeverity.Info,
                             string category = "General",
                             string sourceID = "Unknown")
            : base(sourceID)
        {
            Message = message;
            Severity = severity;
            Category = category;
        }
    }
}
