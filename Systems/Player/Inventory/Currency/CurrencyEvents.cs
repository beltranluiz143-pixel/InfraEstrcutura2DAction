namespace Infra2DAction
{
    public class CurrencyChangedEvent : BaseEvent
    {
        public int CurrentAmount { get; private set; }

        public CurrencyChangedEvent(int currentAmount, string sourceID = "PlayerCurrencySystem")
            : base(sourceID)
        {
            CurrentAmount = currentAmount;
        }
    }

    public class CurrencyAddRequestEvent : BaseEvent
    {
        public int Amount { get; private set; }

        public CurrencyAddRequestEvent(int amount, string sourceID = "Unknown")
            : base(sourceID)
        {
            Amount = amount;
        }
    }

    public class CurrencySpentEvent : BaseEvent
    {
        public int Amount { get; private set; }

        public CurrencySpentEvent(int amount, string sourceID = "PlayerCurrencySystem")
            : base(sourceID)
        {
            Amount = amount;
        }
    }
}
