using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "ShopData_", menuName = "Infraestructura2DAction/Content/Shop Data")]
    public class ShopData : ScriptableObject
    {
        [Serializable]
        public class ShopEntry
        {
            public ItemData Item;
            [Tooltip("Si es -1, usa el BasePrice del ItemData sin modificar.")]
            public int OverridePrice = -1;
        }

        [Serializable]
        public class RoutePriceModifier
        {
            public string RouteID;
            [Tooltip("1 = precio normal. <1 = mas barato. >1 = mas caro.")]
            public float Multiplier = 1f;
        }

        [Header("Identification")]
        public string ShopID;
        public string VendorName;

        [Header("Vendor Art")]
        public Sprite VendorPortrait;

        [Header("Catalog")]
        public List<ShopEntry> Catalog = new List<ShopEntry>();

        [Header("Vendor Dialogue")]
        [TextArea] public string WelcomeLine;
        [TextArea] public string PurchaseSuccessLine;
        [TextArea] public string PurchaseFailLine;

        [Header("Route Price Modifiers")]
        public List<RoutePriceModifier> RoutePriceModifiers = new List<RoutePriceModifier>();

        private Dictionary<string, float> _multiplierCache;

        public float GetMultiplierForRoute(string routeID)
        {
            if (string.IsNullOrEmpty(routeID)) return 1f;

            if (_multiplierCache == null)
            {
                _multiplierCache = new Dictionary<string, float>();
                foreach (RoutePriceModifier modifier in RoutePriceModifiers)
                    if (!string.IsNullOrEmpty(modifier.RouteID))
                        _multiplierCache[modifier.RouteID] = modifier.Multiplier;
            }

            return _multiplierCache.TryGetValue(routeID, out float multiplier) ? multiplier : 1f;
        }

        private void OnValidate()
        {
            _multiplierCache = null;
        }
    }
}
