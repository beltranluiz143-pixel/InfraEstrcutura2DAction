using UnityEngine;

namespace Infra2DAction
{
    public class CameraTrigger : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Collider2D _boundsCollider;
        [SerializeField] private CameraData _cameraData;

        [Header("Priority")]
        [SerializeField] private int _priority = 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            EventBus.Raise(new CameraZoneEnteredEvent(_boundsCollider, _cameraData, _priority, "CameraTrigger"));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            EventBus.Raise(new CameraZoneExitedEvent(_boundsCollider, "CameraTrigger"));
        }
    }
}
