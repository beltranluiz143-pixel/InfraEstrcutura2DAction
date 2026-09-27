namespace Infra2DAction
{
    public class ItemCollectedEvent : BaseEvent
    {
        public string ItemID { get; private set; }

        public ItemCollectedEvent(string itemID, string sourceID = "Unknown")
            : base(sourceID)
        {
            ItemID = itemID;
        }
    }

    public class ItemRemoveRequestEvent : BaseEvent
    {
        public string ItemID { get; private set; }
        public int Amount { get; private set; }

        public ItemRemoveRequestEvent(string itemID, int amount = 1, string sourceID = "Unknown")
            : base(sourceID)
        {
            ItemID = itemID;
            Amount = amount;
        }
    }

    public class InventoryUpdatedEvent : BaseEvent
    {
        public InventoryUpdatedEvent(string sourceID = "PlayerInventorySystem") : base(sourceID) { }
    }

    public class ItemEquippedEvent : BaseEvent
    {
        public string ItemID { get; private set; }
        public int SlotIndex { get; private set; }

        public ItemEquippedEvent(string itemID, int slotIndex, string sourceID = "PlayerInventorySystem")
            : base(sourceID)
        {
            ItemID = itemID;
            SlotIndex = slotIndex;
        }
    }

    public class ItemUnequippedEvent : BaseEvent
    {
        public int SlotIndex { get; private set; }

        public ItemUnequippedEvent(int slotIndex, string sourceID = "PlayerInventorySystem")
            : base(sourceID)
        {
            SlotIndex = slotIndex;
        }
    }

    public class EquipItemRequestEvent : BaseEvent
    {
        public string ItemID { get; private set; }
        public int SlotIndex { get; private set; }

        public EquipItemRequestEvent(string itemID, int slotIndex, string sourceID = "InventoryPanel")
            : base(sourceID)
        {
            ItemID = itemID;
            SlotIndex = slotIndex;
        }
    }
}
