using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "HitboxData_", menuName = "Infraestructura2DAction/AI/Hitbox Data")]
    public class HitboxData : ScriptableObject
    {
        public enum Shape { Circle, Box, Cone, Line }

        [Header("Identification")]
        public string HitboxID;

        [Header("Geometry")]
        public Shape HitboxShape = Shape.Box;
        public Vector2 HitboxSize = new Vector2(1f, 1f);
        public Vector2 HitboxOffset = Vector2.zero;

        [Header("Timing")]
        public float WindupDuration = 0.3f;
        public float ActiveDuration = 0.2f;
        public float RecoveryDuration = 0.3f;

        [Header("Damage")]
        public int Damage = 1;
        public float KnockbackForce = 4f;
        public float HitstopDuration = 0.05f;

        [Header("Telegraph")]
        public int TelegraphVisualIndex = 0;
    }
}
