using UnityEngine;

namespace Infra2DAction
{
    public class SurfaceDetector : MonoBehaviour
    {
        [SerializeField] private SurfaceData _surfaceData;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            SurfacePhysicsSystem system = other.GetComponent<SurfacePhysicsSystem>();
            system?.EnterSurface(_surfaceData);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            SurfacePhysicsSystem system = other.GetComponent<SurfacePhysicsSystem>();
            system?.ExitSurface(_surfaceData);
        }
    }
}
