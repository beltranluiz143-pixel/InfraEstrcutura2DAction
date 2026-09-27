using UnityEngine;

namespace Infra2DAction
{
    public class CurrencyPickup : MonoBehaviour
    {
        [SerializeField] private int _amount = 1;
        [SerializeField] private string _sfxOnCollect = "SFX_CURRENCY_COLLECT";

        private bool _consumed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed || !other.CompareTag("Player")) return;
            _consumed = true;

            PlayerCurrencySystem currencySystem = other.GetComponent<PlayerCurrencySystem>();
            currencySystem?.AddCurrency(_amount);

            EventBus.Raise(new SFXPlayRequestEvent(_sfxOnCollect, transform.position, "CurrencyPickup"));
            gameObject.SetActive(false);
        }
    }
}
