using UnityEngine;

namespace Infra2DAction
{
    public class AbilityPickup : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _abilityFlagKey = "ABILITY_DASH_UNLOCKED";
        [SerializeField] private string _sfxOnPickup = "SFX_ABILITY_UNLOCKED";

        private bool _consumed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed || !other.CompareTag("Player")) return;
            _consumed = true;

            EventBus.Raise(new AbilityUnlockRequestEvent(_abilityFlagKey, "AbilityPickup"));
            EventBus.Raise(new SFXPlayRequestEvent(_sfxOnPickup, transform.position, "AbilityPickup"));

            gameObject.SetActive(false);
        }
    }
}
