using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "ProjectileData_", menuName = "Infraestructura2DAction/AI/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        public enum Shape { Circle, Box }

        [Header("Identification")]
        public string ProjectileID;

        [Header("Movement")]
        public float Speed = 8f;
        public float Lifetime = 5f;
        public float HomingStrength = 0f;
        public float ArcHeight = 0f;
        public int BounceCount = 0;

        [Header("Damage")]
        public int Damage = 1;
        public Shape HitboxShape = Shape.Circle;
        public Vector2 HitboxSize = new Vector2(0.4f, 0.4f);

        [Header("Spread/Burst")]
        public float SpreadAngle = 0f;
        public int BurstCount = 1;

        [Header("Pooling")]
        public string PoolAssetID;
    }
}
