using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public enum EnemyMovementPattern
    {
        Stationary_Idle, Stationary_Rotating, Stationary_Camouflaged,
        PatrolPath_Linear, PatrolPath_Loop, PatrolPath_Pause, PatrolPath_Reactive,
        Flying_Direct, Flying_Diving, Flying_Ascending,
        GroundChase_Direct, GroundChase_Jumping, GroundChase_LosesInterest
    }

    public enum EnemyAttackPattern
    {
        MeleeContact_Pulse, MeleeContact_Knockback, MeleeContact_SelfDestruct,
        MeleeHitbox_SingleSwing, MeleeHitbox_ComboSwing, MeleeHitbox_WideArc,
        MeleeHitbox_Overhead, MeleeHitbox_Delayed,
        RangedProjectile_StraightShot, RangedProjectile_Burst, RangedProjectile_Bouncing,
        RangedProjectile_Spread, RangedProjectile_Homing,
        AreaHazard_Telegraphed, AreaHazard_Persistent, AreaHazard_Expanding, AreaHazard_Multiple,
        Charge_Straight, Charge_WallBounce
    }

    public enum AttackSelectionMode { Random, Conditional }

    [CreateAssetMenu(fileName = "EnemyData_", menuName = "Infraestructura2DAction/AI/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Serializable]
        public class AttackEntry
        {
            public EnemyAttackPattern Pattern;

            [Tooltip("Asignar solo si el Pattern es de tipo Melee/AreaHazard.")]
            public HitboxData HitboxData;

            [Tooltip("Asignar solo si el Pattern es de tipo RangedProjectile.")]
            public ProjectileData ProjectileData;

            [Header("Condicion de distancia (solo si SelectionMode = Conditional)")]
            public float MinRange = 0f;
            public float MaxRange = 999f;

            [Tooltip("Peso relativo si SelectionMode = Random. Mayor = mas probable.")]
            public float RandomWeight = 1f;
        }

        [Header("Identification")]
        public string EnemyID;

        [Header("Stats")]
        public int MaxHP = 10;
        public float MoveSpeed = 2f;

        [Header("Detection")]
        public float DetectionRadius = 4f;

        [Header("Movement")]
        public EnemyMovementPattern MovementPattern = EnemyMovementPattern.Stationary_Idle;
        public List<Vector2> PathPoints = new List<Vector2>();
        public bool PathPingPong = false;
        public float AttackSpeedMultiplier = 1f;

        [Header("Attacks")]
        [Tooltip("Random: elige al azar segun RandomWeight. " +
                 "Conditional: elige segun la distancia real al jugador (MinRange/MaxRange).")]
        public AttackSelectionMode SelectionMode = AttackSelectionMode.Random;
        public List<AttackEntry> AttackEntries = new List<AttackEntry>();

        public float AttackRange = 1.5f;
        public float AttackCooldown = 1.5f;

        [Header("Pacification")]
        [Range(0f, 1f)] public float PacificationThreshold = 0.25f;

        [Header("Death / Pacify")]
        public bool DropsLoot = false;
        public string DropPoolID;

        [Header("Animation")]
        public int AnimationStateIndex = 0;
    }
}
