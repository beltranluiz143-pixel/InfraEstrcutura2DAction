using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class PlayerInventorySystem : MonoBehaviour
    {
        [Header("Equipment")]
        [SerializeField] private int _equipmentSlots = 2;

        [Header("Stat Targets (para aplicar efectos de equipamiento)")]
        [SerializeField] private PlayerCombatSystem _combatSystem;
        [SerializeField] private PlayerHealthSystem _healthSystem;
        [SerializeField] private PlayerResourceSystem _resourceSystem;

        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
        private string[] _equippedItems;

        private ItemDatabase _itemDatabase;
        private SaveSystem _saveSystem;

        public void Initialize(ItemDatabase itemDatabase, SaveSystem saveSystem)
        {
            _itemDatabase = itemDatabase;
            _saveSystem = saveSystem;
            _equippedItems = new string[_equipmentSlots];

            ApplySave(saveSystem != null ? saveSystem.GetCurrentSave() : null);
            EventBus.Raise(new InventoryUpdatedEvent());
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            EventBus.Subscribe<ItemRemoveRequestEvent>(OnItemRemoveRequested);
            EventBus.Subscribe<EquipItemRequestEvent>(OnEquipRequested);
            EventBus.Subscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
            EventBus.Unsubscribe<ItemRemoveRequestEvent>(OnItemRemoveRequested);
            EventBus.Unsubscribe<EquipItemRequestEvent>(OnEquipRequested);
            EventBus.Unsubscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        private void OnItemCollected(ItemCollectedEvent e) => AddItem(e.ItemID, 1);

        private void OnItemRemoveRequested(ItemRemoveRequestEvent e) => RemoveItem(e.ItemID, e.Amount);

        public void AddItem(string itemID, int amount)
        {
            if (!_items.ContainsKey(itemID)) _items[itemID] = 0;
            _items[itemID] += amount;

            EventBus.Raise(new InventoryUpdatedEvent());
        }

        public bool RemoveItem(string itemID, int amount)
        {
            if (!_items.ContainsKey(itemID) || _items[itemID] < amount) return false;

            _items[itemID] -= amount;
            if (_items[itemID] <= 0) _items.Remove(itemID);

            EventBus.Raise(new InventoryUpdatedEvent());
            return true;
        }

        public int GetItemCount(string itemID) => _items.TryGetValue(itemID, out int count) ? count : 0;
        public bool HasItem(string itemID) => GetItemCount(itemID) > 0;
        public IReadOnlyDictionary<string, int> GetAllItems() => _items;

        private void OnEquipRequested(EquipItemRequestEvent e)
        {
            if (e.SlotIndex < 0 || e.SlotIndex >= _equipmentSlots) return;
            if (!HasItem(e.ItemID)) return;

            if (!string.IsNullOrEmpty(_equippedItems[e.SlotIndex]))
                RevertItemEffect(_equippedItems[e.SlotIndex]);

            _equippedItems[e.SlotIndex] = e.ItemID;
            ApplyItemEffect(e.ItemID);

            EventBus.Raise(new ItemEquippedEvent(e.ItemID, e.SlotIndex));
        }

        public void UnequipSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _equipmentSlots) return;
            if (string.IsNullOrEmpty(_equippedItems[slotIndex])) return;

            RevertItemEffect(_equippedItems[slotIndex]);
            _equippedItems[slotIndex] = null;

            EventBus.Raise(new ItemUnequippedEvent(slotIndex));
        }

        public string GetEquippedItem(int slotIndex) =>
            slotIndex >= 0 && slotIndex < _equipmentSlots ? _equippedItems[slotIndex] : null;

        private void ApplyItemEffect(string itemID, bool alsoAdjustCurrent = true)
        {
            ItemData item = _itemDatabase != null ? _itemDatabase.GetItem(itemID) : null;
            if (item == null || item.Type != ItemType.Equipable) return;
            if (string.IsNullOrEmpty(item.EffectTargetVariable)) return;

            switch (item.EffectTargetVariable)
            {
                case "AttackDamage":
                    _combatSystem?.ApplyExternalDamageModifier(item.EffectAmount);
                    break;
                case "MaxHP":
                    _healthSystem?.ApplyExternalMaxHPModifier(Mathf.RoundToInt(item.EffectAmount), alsoAdjustCurrent);
                    break;
                case "HealRecoveryTime":
                    _healthSystem?.ApplyExternalHealSpeedModifier(item.EffectAmount);
                    break;
                case "MaxResource":
                    _resourceSystem?.ApplyExternalMaxResourceModifier(item.EffectAmount);
                    break;
                default:
                    DebugSystem.LogWarning($"EffectTargetVariable desconocido: {item.EffectTargetVariable}",
                                           "Inventory", "PlayerInventorySystem");
                    break;
            }
        }

        private void RevertItemEffect(string itemID)
        {
            ItemData item = _itemDatabase != null ? _itemDatabase.GetItem(itemID) : null;
            if (item == null || item.Type != ItemType.Equipable) return;

            switch (item.EffectTargetVariable)
            {
                case "AttackDamage":
                    _combatSystem?.ApplyExternalDamageModifier(-item.EffectAmount);
                    break;
                case "MaxHP":
                    _healthSystem?.ApplyExternalMaxHPModifier(-Mathf.RoundToInt(item.EffectAmount));
                    break;
                case "HealRecoveryTime":
                    _healthSystem?.ApplyExternalHealSpeedModifier(-item.EffectAmount);
                    break;
                case "MaxResource":
                    _resourceSystem?.ApplyExternalMaxResourceModifier(-item.EffectAmount);
                    break;
            }
        }

        private void OnCollectSaveData(CollectSaveDataEvent e)
        {
            if (_saveSystem == null || _equippedItems == null) return;

            ItemSaveData saveData = new ItemSaveData();

            foreach (var kvp in _items)
                saveData.Items.Add(new ItemSaveData.ItemEntry { ItemID = kvp.Key, Amount = kvp.Value });

            for (int i = 0; i < _equipmentSlots; i++)
                saveData.EquippedSlots[i] = _equippedItems[i];

            PlayerSaveData data = _saveSystem.GetCurrentSave().Player;
            data.Inventory = saveData;
            _saveSystem.UpdatePlayerData(data);
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            ApplySave(e.Data);
            EventBus.Raise(new InventoryUpdatedEvent());
        }

        private void ApplySave(SaveData data)
        {
            if (_equippedItems == null || data?.Player?.Inventory == null) return;

            _items.Clear();
            foreach (var entry in data.Player.Inventory.Items)
                _items[entry.ItemID] = entry.Amount;

            for (int i = 0; i < _equipmentSlots && i < data.Player.Inventory.EquippedSlots.Length; i++)
            {
                string equippedID = data.Player.Inventory.EquippedSlots[i];
                if (!string.IsNullOrEmpty(equippedID))
                {
                    _equippedItems[i] = equippedID;
                    ApplyItemEffect(equippedID, false);
                }
            }
        }
    }
}
