using UnityEngine;

namespace Infra2DAction
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private string _shopPanelID = "Shop";

        private ShopData _activeShop;
        private PlayerSystem _player;
        private RouteSystem _routeSystem;

        public void Initialize(RouteSystem routeSystem)
        {
            _routeSystem = routeSystem;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ShopOpenRequestEvent>(OnShopOpenRequested);
            EventBus.Subscribe<ShopPurchaseRequestEvent>(OnPurchaseRequested);
            EventBus.Subscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
            EventBus.Subscribe<UIPanelClosedEvent>(OnPanelClosed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ShopOpenRequestEvent>(OnShopOpenRequested);
            EventBus.Unsubscribe<ShopPurchaseRequestEvent>(OnPurchaseRequested);
            EventBus.Unsubscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
            EventBus.Unsubscribe<UIPanelClosedEvent>(OnPanelClosed);
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent e)
        {
            _player = e.PlayerObject.GetComponent<PlayerSystem>();
        }

        private void OnShopOpenRequested(ShopOpenRequestEvent e)
        {
            if (e.Data == null || _activeShop != null) return;

            _activeShop = e.Data;

            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Menu, "ShopManager"));
            EventBus.Raise(new UIOpenPanelRequestEvent(_shopPanelID, "ShopManager"));
            EventBus.Raise(new ShopOpenedEvent(e.Data));
        }

        private void OnPanelClosed(UIPanelClosedEvent e)
        {
            if (_activeShop == null || e.PanelID != _shopPanelID) return;

            _activeShop = null;
            EventBus.Raise(new ShopClosedEvent());
            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Gameplay, "ShopManager"));
        }

        private void OnPurchaseRequested(ShopPurchaseRequestEvent e)
        {
            if (_activeShop == null || e.Item == null) return;

            PlayerCurrencySystem currency = _player != null ? _player.CurrencySystem : null;
            PlayerInventorySystem inventory = _player != null ? _player.InventorySystem : null;

            if (currency == null || inventory == null)
            {
                EventBus.Raise(new ShopPurchaseFailedEvent());
                return;
            }

            int finalPrice = CalculatePrice(e.Item);

            if (!currency.TrySpend(finalPrice))
            {
                EventBus.Raise(new ShopPurchaseFailedEvent());
                return;
            }

            inventory.AddItem(e.Item.ItemID, 1);
            EventBus.Raise(new ShopPurchaseSuccessEvent(e.Item.ItemID));
        }

        public int CalculatePrice(ItemData item)
        {
            int basePrice = item.BasePrice;

            ShopData.ShopEntry entry = FindCatalogEntry(item);
            if (entry != null && entry.OverridePrice >= 0)
                basePrice = entry.OverridePrice;

            float multiplier = 1f;
            if (_routeSystem != null && _activeShop != null)
                multiplier = _activeShop.GetMultiplierForRoute(_routeSystem.GetDominantRoute());

            return Mathf.RoundToInt(basePrice * multiplier);
        }

        private ShopData.ShopEntry FindCatalogEntry(ItemData item)
        {
            foreach (var entry in _activeShop.Catalog)
                if (entry.Item == item) return entry;
            return null;
        }
    }
}
