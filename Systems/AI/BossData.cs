using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public enum BossMovementPattern
    {
        Stationary_Throne, Stationary_Pedestal,
        Patrol_ArenaPerimeter, Patrol_ZoneToZone,
        Flying_Erratic, Flying_Diving, Flying_Orbiting,
        GroundChase_Relentless, GroundChase_Teleporting,
        Charge_MultiDirectional, Charge_Combo,
        SummonAnchor_Fixed, PhaseShift_Vanish,
        PredefinedPath_HitboxGuided, Viewing_DirectApproach
    }

    public enum BossAttackPattern
    {
        Melee_GroundSlam, Melee_SweepingArc, Melee_MultiLimb,
        Ranged_Barrage, Ranged_Laser, Ranged_Summon,
        Area_FullArenaHazard, Area_RotatingHazard,
        MeleeHitbox_SingleSwing, MeleeHitbox_ComboSwing,
        RangedProjectile_StraightShot, RangedProjectile_Burst,
        RangedProjectile_Bouncing, RangedProjectile_Spread, RangedProjectile_Homing,
        Charge_Straight, Charge_WallBounce,
        AreaHazard_Telegraphed, AreaHazard_Persistent, AreaHazard_Expanding, AreaHazard_Multiple
    }

    [CreateAssetMenu(fileName = "BossData_", menuName = "Infraestructura2DAction/AI/Boss Data")]
    public class BossData : ScriptableObject
    {
        [Serializable]
        public class BossAttackEntry
        {
            public BossAttackPattern Pattern;
            public HitboxData HitboxData;
            public ProjectileData ProjectileData;

            [Header("Condicion de distancia (solo si SelectionMode = Conditional)")]
            public float MinRange = 0f;
            public float MaxRange = 999f;

            [Tooltip("Peso relativo si SelectionMode = Random.")]
            public float RandomWeight = 1f;
        }

        [Serializable]
        public class BossPhase
        {
            [Tooltip("La fase se activa cuando la vida restante cae por debajo de este porcentaje. " +
                     "Ordenar de mayor a menor (la fase 0 debe ser 1).")]
            [Range(0f, 1f)] public float HPThresholdPercent = 1f;
            public BossMovementPattern MovementPattern;

            [Header("Attacks")]
            public AttackSelectionMode SelectionMode = AttackSelectionMode.Random;
            public List<BossAttackEntry> AttackEntries = new List<BossAttackEntry>();

            public bool IsInvulnerableOnTransition = false;
            public bool SummonsReinforcements = false;
            public int AnimationStateIndex = 0;
        }

        [Header("Identification")]
        public string BossID;

        [Header("Base Stats")]
        public int MaxHP = 200;
        public float MoveSpeed = 3f;
        public float DetectionRadius = 8f;

        [Header("Phases (al menos 1, incluso bosses de fase unica)")]
        public List<BossPhase> Phases = new List<BossPhase>();

        [Header("Shared Combat")]
        public float AttackRange = 2f;
        public float AttackCooldown = 1.2f;

        [Header("Pacification")]
        [Range(0f, 1f)] public float PacificationThreshold = 0f;

        [Header("Death")]
        public bool DropsLoot = false;
        public string DropPoolID;
    }
}
