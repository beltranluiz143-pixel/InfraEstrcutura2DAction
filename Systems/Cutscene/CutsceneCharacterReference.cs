using UnityEngine;

namespace Infra2DAction
{
    public class CutsceneCharacterReference : MonoBehaviour
    {
        [SerializeField] private CharacterAnimationID _characterID;

        public string CharacterID => _characterID != null ? _characterID.CharacterID : null;
        public Transform CharacterTransform => transform;

        private void OnEnable()
        {
            EventBus.Subscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Raise(new CutsceneCharacterRegisteredEvent(this));
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Raise(new CutsceneCharacterUnregisteredEvent(this));
        }

        private void OnCoreInitialized(CoreInitializedEvent e)
        {
            EventBus.Raise(new CutsceneCharacterRegisteredEvent(this));
        }
    }
}
