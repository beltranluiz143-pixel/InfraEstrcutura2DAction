using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "SurfaceData_", menuName = "Infraestructura2DAction/Player/Surface Data")]
    public class SurfaceData : ScriptableObject
    {
        public enum SurfaceBehavior
        {
            Standard,
            Water,
            Directional,
            Bouncy
        }

        [Header("Identification")]
        public string SurfaceID;
        public SurfaceBehavior Behavior = SurfaceBehavior.Standard;

        [Header("Multipliers")]
        [Range(0f, 2f)] public float FrictionMultiplier = 1f;
        [Range(0f, 2f)] public float GravityMultiplier = 1f;
        [Range(0f, 2f)] public float MaxSpeedMultiplier = 1f;

        [Header("Directional (solo si Behavior = Directional)")]
        public Vector2 DirectionalForce = Vector2.zero;

        [Header("Bouncy (solo si Behavior = Bouncy)")]
        public float BounceForce = 0f;
    }
}
