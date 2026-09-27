using UnityEngine;

namespace Infra2DAction
{
    public class SpawnPointRegisteredEvent : BaseEvent
    {
        public SpawnPoint SpawnPoint { get; private set; }

        public SpawnPointRegisteredEvent(SpawnPoint spawnPoint, string sourceID = "SpawnPoint")
            : base(sourceID)
        {
            SpawnPoint = spawnPoint;
        }
    }

    public class SpawnPointUnregisteredEvent : BaseEvent
    {
        public SpawnPoint SpawnPoint { get; private set; }

        public SpawnPointUnregisteredEvent(SpawnPoint spawnPoint, string sourceID = "SpawnPoint")
            : base(sourceID)
        {
            SpawnPoint = spawnPoint;
        }
    }

    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnPointID = "Default";

        public string SpawnPointID => _spawnPointID;

        private void OnEnable()
        {
            EventBus.Subscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Raise(new SpawnPointRegisteredEvent(this));
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Raise(new SpawnPointUnregisteredEvent(this));
        }

        private void OnCoreInitialized(CoreInitializedEvent e)
        {
            EventBus.Raise(new SpawnPointRegisteredEvent(this));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.6f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.35f);
            Gizmos.DrawLine(transform.position + Vector3.left * 0.5f, transform.position + Vector3.right * 0.5f);
            Gizmos.DrawLine(transform.position + Vector3.down * 0.5f, transform.position + Vector3.up * 0.5f);
        }
#endif
    }
}
