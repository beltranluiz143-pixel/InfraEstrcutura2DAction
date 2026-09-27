using UnityEngine;

namespace Infra2DAction
{
    public class ResourcePickup : MonoBehaviour, IInteractable
    {
        [Header("Configuration")]
        [SerializeField] private float _amount = 20f;
        [SerializeField] private string _sfxOnCollect = "SFX_RESOURCE_COLLECT";

        private bool _consumed;

        public void OnInteract()
        {
            if (_consumed) return;
            _consumed = true;

            EventBus.Raise(new ResourceCollectRequestEvent(_amount, "ResourcePickup"));

            if (!string.IsNullOrEmpty(_sfxOnCollect))
                EventBus.Raise(new SFXPlayRequestEvent(_sfxOnCollect, transform.position, "ResourcePickup"));

            gameObject.SetActive(false);
        }
    }
}
