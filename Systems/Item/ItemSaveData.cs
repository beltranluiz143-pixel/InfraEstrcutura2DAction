using System;
using System.Collections.Generic;

namespace Infra2DAction
{
    [Serializable]
    public class ItemSaveData
    {
        [Serializable]
        public class ItemEntry
        {
            public string ItemID;
            public int Amount;
        }

        public List<ItemEntry> Items = new List<ItemEntry>();
        public string[] EquippedSlots = new string[2];
    }
}
