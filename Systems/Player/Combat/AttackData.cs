using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "AttackData_", menuName = "Infraestructura2DAction/Player/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [Header("Identification")]
        public AttackVariant Variant;

        [Header("Damage")]
        public int Damage = 1;

        [Header("Hitbox")]
        public Vector2 HitboxSize = new Vector2(1f, 1f);
        public Vector2 HitboxOffset = new Vector2(0.8f, 0f);
        [Range(0.05f, 0.5f)] public float HitboxActiveDuration = 0.2f;

        [Header("Timing")]
        [Range(0f, 0.3f)] public float RecoveryDuration = 0.2f;

        [Header("Special")]
        public float PogoForce = 0f;
    }
}
