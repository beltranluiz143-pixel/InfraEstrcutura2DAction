using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Infra2DAction
{
    public class InventoryPanel : UIPanel
    {
        [Serializable]
        public class Tab
        {
            public Button Button;
            public GameObject Content;
        }

        [Header("Tabs (la primera es la de items)")]
        [SerializeField] private List<Tab> _tabs = new List<Tab>();

        [Header("Items Grid")]
        [SerializeField] private Transform _itemGridContainer;
        [SerializeField] private GameObject _itemSlotPrefab;

        private PlayerInventorySystem _inventorySystem;
        private ItemDatabase _itemDatabase;

        public void Initialize(PlayerInventorySystem inventorySystem, ItemDatabase itemDatabase)
        {
            _inventorySystem = inventorySystem;
            _itemDatabase = itemDatabase;
        }

        private void Awake()
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i;
                if (_tabs[i].Button != null)
                    _tabs[i].Button.onClick.AddListener(() => ShowTab(index));
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InventoryUpdatedEvent>(OnInventoryUpdated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventoryUpdatedEvent>(OnInventoryUpdated);
        }

        public override void Open()
        {
            base.Open();
            ShowTab(0);
            RefreshItemGrid();
        }

        private void OnInventoryUpdated(InventoryUpdatedEvent e) => RefreshItemGrid();

        private void RefreshItemGrid()
        {
            if (_inventorySystem == null || _itemGridContainer == null || _itemSlotPrefab == null) return;

            foreach (Transform child in _itemGridContainer) Destroy(child.gameObject);

            foreach (var kvp in _inventorySystem.GetAllItems())
            {
                ItemData itemData = _itemDatabase != null ? _itemDatabase.GetItem(kvp.Key) : null;
                if (itemData == null) continue;

                GameObject slot = Instantiate(_itemSlotPrefab, _itemGridContainer);
                Image icon = slot.GetComponentInChildren<Image>();
                TMP_Text countText = slot.GetComponentInChildren<TMP_Text>();

                if (icon != null) icon.sprite = itemData.Icon;
                if (countText != null) countText.text = kvp.Value.ToString();
            }
        }

        public void ShowTab(int index)
        {
            for (int i = 0; i < _tabs.Count; i++)
                if (_tabs[i].Content != null)
                    _tabs[i].Content.SetActive(i == index);
        }
    }
}
