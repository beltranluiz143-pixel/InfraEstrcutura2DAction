namespace Infra2DAction
{
    public class MinigameStartRequestEvent : BaseEvent
    {
        public MinigameData Data { get; private set; }

        public MinigameStartRequestEvent(MinigameData data, string sourceID = "Unknown")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class MinigameStartedEvent : BaseEvent
    {
        public MinigameStartedEvent(string sourceID = "MinigameSystem") : base(sourceID) { }
    }

    public class MinigameEndedEvent : BaseEvent
    {
        public bool Success { get; private set; }

        public MinigameEndedEvent(bool success, string sourceID = "Unknown")
            : base(sourceID)
        {
            Success = success;
        }
    }

    public class MinigameDamageBossRequestEvent : BaseEvent
    {
        public int Amount { get; private set; }

        public MinigameDamageBossRequestEvent(int amount, string sourceID = "MinigameSystem")
            : base(sourceID)
        {
            Amount = amount;
        }
    }
}
