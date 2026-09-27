using UnityEngine;

namespace Infra2DAction
{
    public class ShopInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private ShopData _shopData;

        public void OnInteract()
        {
            if (_shopData == null) return;
            EventBus.Raise(new ShopOpenRequestEvent(_shopData, "ShopInteractable"));
        }
    }
}
