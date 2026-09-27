namespace Infra2DAction
{
    public class ShopOpenRequestEvent : BaseEvent
    {
        public ShopData Data { get; private set; }

        public ShopOpenRequestEvent(ShopData data, string sourceID = "Unknown")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class ShopOpenedEvent : BaseEvent
    {
        public ShopData Data { get; private set; }

        public ShopOpenedEvent(ShopData data, string sourceID = "ShopManager")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class ShopPurchaseRequestEvent : BaseEvent
    {
        public ItemData Item { get; private set; }

        public ShopPurchaseRequestEvent(ItemData item, string sourceID = "ShopPanel")
            : base(sourceID)
        {
            Item = item;
        }
    }

    public class ShopPurchaseSuccessEvent : BaseEvent
    {
        public string ItemID { get; private set; }

        public ShopPurchaseSuccessEvent(string itemID, string sourceID = "ShopManager")
            : base(sourceID)
        {
            ItemID = itemID;
        }
    }

    public class ShopPurchaseFailedEvent : BaseEvent
    {
        public ShopPurchaseFailedEvent(string sourceID = "ShopManager") : base(sourceID) { }
    }

    public class ShopClosedEvent : BaseEvent
    {
        public ShopClosedEvent(string sourceID = "ShopManager") : base(sourceID) { }
    }
}
