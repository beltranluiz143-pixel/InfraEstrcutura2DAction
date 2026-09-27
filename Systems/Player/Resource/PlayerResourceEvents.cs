namespace Infra2DAction
{
    public class ResourceChangedEvent : BaseEvent
    {
        public float CurrentResource { get; private set; }
        public float MaxResource { get; private set; }

        public ResourceChangedEvent(float current, float max, string sourceID = "PlayerResourceSystem")
            : base(sourceID)
        {
            CurrentResource = current;
            MaxResource = max;
        }
    }

    public class ResourceConsumedEvent : BaseEvent
    {
        public string Key { get; private set; }
        public float Amount { get; private set; }

        public ResourceConsumedEvent(string key, float amount, string sourceID = "PlayerResourceSystem")
            : base(sourceID)
        {
            Key = key;
            Amount = amount;
        }
    }

    public class ResourceDepletedEvent : BaseEvent
    {
        public string AttemptedKey { get; private set; }

        public ResourceDepletedEvent(string attemptedKey, string sourceID = "PlayerResourceSystem")
            : base(sourceID)
        {
            AttemptedKey = attemptedKey;
        }
    }

    public class ResourceCollectedEvent : BaseEvent
    {
        public float Amount { get; private set; }

        public ResourceCollectedEvent(float amount, string sourceID = "PlayerResourceSystem")
            : base(sourceID)
        {
            Amount = amount;
        }
    }

    public class ResourceCollectRequestEvent : BaseEvent
    {
        public float Amount { get; private set; }

        public ResourceCollectRequestEvent(float amount, string sourceID = "Unknown")
            : base(sourceID)
        {
            Amount = amount;
        }
    }

    public class ResourceConsumeRequestEvent : BaseEvent
    {
        public float Amount { get; private set; }
        public string Reason { get; private set; }
        public bool Handled { get; private set; }
        public bool Success { get; private set; }

        public ResourceConsumeRequestEvent(float amount, string reason, string sourceID = "Unknown")
            : base(sourceID)
        {
            Amount = amount;
            Reason = reason;
        }

        public void SetResult(bool success)
        {
            Success = success;
            Handled = true;
        }
    }

    public class ResourceFullDrainRequestEvent : BaseEvent
    {
        public string Reason { get; private set; }

        public ResourceFullDrainRequestEvent(string reason, string sourceID = "Unknown")
            : base(sourceID)
        {
            Reason = reason;
        }
    }
}
