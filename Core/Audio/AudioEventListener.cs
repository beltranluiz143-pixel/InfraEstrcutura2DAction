using UnityEngine;

namespace Infra2DAction
{
    public class AudioEventListener : MonoBehaviour
    {
        public enum ListenEventType
        {
            OnInteract,
            OnDestroyed,
            OnActivated
        }

        [Header("Configuration")]
        [SerializeField] private ListenEventType _eventType;
        [SerializeField] private string _soundID;

        private void OnEnable()
        {
            switch (_eventType)
            {
                case ListenEventType.OnInteract:
                    EventBus.Subscribe<InteractPressedEvent>(OnInteractTriggered);
                    break;
                case ListenEventType.OnDestroyed:
                    EventBus.Subscribe<EnemyKilledEvent>(OnDestroyedTriggered);
                    break;
                case ListenEventType.OnActivated:
                    EventBus.Subscribe<GlobalFlagChangedEvent>(OnActivatedTriggered);
                    break;
            }
        }

        private void OnDisable()
        {
            switch (_eventType)
            {
                case ListenEventType.OnInteract:
                    EventBus.Unsubscribe<InteractPressedEvent>(OnInteractTriggered);
                    break;
                case ListenEventType.OnDestroyed:
                    EventBus.Unsubscribe<EnemyKilledEvent>(OnDestroyedTriggered);
                    break;
                case ListenEventType.OnActivated:
                    EventBus.Unsubscribe<GlobalFlagChangedEvent>(OnActivatedTriggered);
                    break;
            }
        }

        private void OnInteractTriggered(InteractPressedEvent e) => PlayConfiguredSound();
        private void OnDestroyedTriggered(EnemyKilledEvent e) => PlayConfiguredSound();
        private void OnActivatedTriggered(GlobalFlagChangedEvent e) => PlayConfiguredSound();

        private void PlayConfiguredSound()
        {
            EventBus.Raise(new SFXPlayRequestEvent(_soundID, transform.position, "AudioEventListener"));
        }
    }
}
