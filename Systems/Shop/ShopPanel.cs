using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Infra2DAction
{
    public class ShopPanel : UIPanel
    {
        [Header("Vendor")]
        [SerializeField] private TMP_Text _vendorDialogueText;
        [SerializeField] private Image _vendorPortraitImage;

        [Header("Catalog")]
        [SerializeField] private Transform _catalogContainer;
        [SerializeField] private GameObject _catalogEntryPrefab;

        [Header("Navigation")]
        [SerializeField] private Button _closeButton;

        private ShopData _currentShop;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(() => EventBus.Raise(new UIClosePanelRequestEvent("ShopPanel")));
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ShopOpenedEvent>(OnShopOpened);
            EventBus.Subscribe<ShopPurchaseSuccessEvent>(OnPurchaseSuccess);
            EventBus.Subscribe<ShopPurchaseFailedEvent>(OnPurchaseFailed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ShopOpenedEvent>(OnShopOpened);
            EventBus.Unsubscribe<ShopPurchaseSuccessEvent>(OnPurchaseSuccess);
            EventBus.Unsubscribe<ShopPurchaseFailedEvent>(OnPurchaseFailed);
        }

        private void OnShopOpened(ShopOpenedEvent e)
        {
            _currentShop = e.Data;

            if (_vendorDialogueText != null)
                _vendorDialogueText.text = e.Data.WelcomeLine;

            if (_vendorPortraitImage != null && e.Data.VendorPortrait != null)
                _vendorPortraitImage.sprite = e.Data.VendorPortrait;

            BuildCatalog();
        }

        private void BuildCatalog()
        {
            if (_catalogContainer == null || _catalogEntryPrefab == null) return;

            foreach (Transform child in _catalogContainer) Destroy(child.gameObject);

            foreach (var entry in _currentShop.Catalog)
            {
                if (entry.Item == null) continue;

                GameObject entryObject = Instantiate(_catalogEntryPrefab, _catalogContainer);
                Transform iconTransform = entryObject.transform.Find("Icon");
                Transform nameTransform = entryObject.transform.Find("Name");
                Image icon = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
                TMP_Text nameText = nameTransform != null ? nameTransform.GetComponent<TMP_Text>() : null;
                Button buyButton = entryObject.GetComponentInChildren<Button>();

                if (icon != null) icon.sprite = entry.Item.Icon;
                if (nameText != null) nameText.text = entry.Item.DisplayName;

                ItemData itemRef = entry.Item;
                if (buyButton != null)
                    buyButton.onClick.AddListener(() =>
                        EventBus.Raise(new ShopPurchaseRequestEvent(itemRef, "ShopPanel")));
            }
        }

        private void OnPurchaseSuccess(ShopPurchaseSuccessEvent e)
        {
            if (_vendorDialogueText != null && _currentShop != null)
                _vendorDialogueText.text = _currentShop.PurchaseSuccessLine;
        }

        private void OnPurchaseFailed(ShopPurchaseFailedEvent e)
        {
            if (_vendorDialogueText != null && _currentShop != null)
                _vendorDialogueText.text = _currentShop.PurchaseFailLine;
        }
    }
}
