using UnityEngine;

namespace Infra2DAction
{
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private string _itemID;
        [SerializeField] private string _sfxOnCollect = "SFX_ITEM_PICKUP";

        private bool _consumed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed || string.IsNullOrEmpty(_itemID) || !other.CompareTag("Player")) return;
            _consumed = true;

            EventBus.Raise(new ItemCollectedEvent(_itemID, "ItemPickup"));

            if (!string.IsNullOrEmpty(_sfxOnCollect))
                EventBus.Raise(new SFXPlayRequestEvent(_sfxOnCollect, transform.position, "ItemPickup"));

            gameObject.SetActive(false);
        }
    }
}
