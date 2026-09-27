using UnityEngine;

namespace Infra2DAction
{
    [RequireComponent(typeof(SpawnPoint))]
    public class SavePoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _sfxOnSave = "SFX_SAVE_POINT";

        private SpawnPoint _spawnPoint;

        private void Awake() => _spawnPoint = GetComponent<SpawnPoint>();

        public void OnInteract()
        {
            EventBus.Raise(new CheckpointActivatedEvent(_spawnPoint.SpawnPointID, "SavePoint"));

            if (!string.IsNullOrEmpty(_sfxOnSave))
                EventBus.Raise(new SFXPlayRequestEvent(_sfxOnSave, transform.position, "SavePoint"));
        }
    }
}
