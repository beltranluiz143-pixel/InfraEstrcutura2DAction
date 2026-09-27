using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "CameraData_", menuName = "Infraestructura2DAction/Camera/Camera Data")]
    public class CameraData : ScriptableObject
    {
        [Header("Damping (suavizado normal)")]
        public float HorizontalDamping = 1f;
        public float VerticalDamping = 1f;

        [Header("Look Ahead")]
        public float LookAheadDistance = 2f;
        public float LookAheadSmoothing = 0.5f;

        [Header("Zoom")]
        public float OrthographicSize = 6f;

        [Header("Eventos Bruscos (excepciones explicitas)")]
        public bool AllowAbruptMovement = false;
        public float AbruptDamping = 0.1f;
    }
}
