using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class AttackHandler : MonoBehaviour
    {
        private enum AttackCategory { Melee, Charge, Projectile }

        [Header("Data")]
        [SerializeField] private EnemyData _enemyData;
        [SerializeField] private BossData _bossData;

        [Header("Hitbox Origin")]
        [SerializeField] private Transform _hitboxOrigin;
        [SerializeField] private LayerMask _playerLayerMask;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ExecuteAttack(Transform target)
        {
            if (_enemyData == null || target == null) return;

            EnemyData.AttackEntry chosen = SelectEnemyAttack(target.position);
            if (chosen == null) return;

            ExecuteChosenAttack(chosen.Pattern, chosen.HitboxData, chosen.ProjectileData, target);
        }

        private EnemyData.AttackEntry SelectEnemyAttack(Vector3 targetPosition)
        {
            if (_enemyData.AttackEntries.Count == 0) return null;

            float distance = Vector3.Distance(transform.position, targetPosition);

            if (_enemyData.SelectionMode == AttackSelectionMode.Conditional)
            {
                List<EnemyData.AttackEntry> valid = new List<EnemyData.AttackEntry>();
                foreach (var entry in _enemyData.AttackEntries)
                    if (distance >= entry.MinRange && distance <= entry.MaxRange)
                        valid.Add(entry);

                if (valid.Count == 0) return null;
                return valid[Random.Range(0, valid.Count)];
            }

            float totalWeight = 0f;
            foreach (var entry in _enemyData.AttackEntries) totalWeight += entry.RandomWeight;

            float roll = Random.Range(0f, totalWeight);
            float accumulated = 0f;

            foreach (var entry in _enemyData.AttackEntries)
            {
                accumulated += entry.RandomWeight;
                if (roll <= accumulated) return entry;
            }

            return _enemyData.AttackEntries[_enemyData.AttackEntries.Count - 1];
        }

        public void ExecuteBossAttack(Transform target, int phaseIndex)
        {
            if (_bossData == null || target == null) return;
            if (_bossData.Phases.Count == 0 || phaseIndex >= _bossData.Phases.Count) return;

            BossData.BossPhase phase = _bossData.Phases[phaseIndex];
            BossData.BossAttackEntry chosen = SelectBossAttack(phase, target.position);
            if (chosen == null) return;

            ExecuteChosenAttack(MapBossPatternToEnemyEquivalent(chosen.Pattern),
                                chosen.HitboxData, chosen.ProjectileData, target);
        }

        private BossData.BossAttackEntry SelectBossAttack(BossData.BossPhase phase, Vector3 targetPosition)
        {
            if (phase.AttackEntries.Count == 0) return null;

            float distance = Vector3.Distance(transform.position, targetPosition);

            if (phase.SelectionMode == AttackSelectionMode.Conditional)
            {
                List<BossData.BossAttackEntry> valid = new List<BossData.BossAttackEntry>();
                foreach (var entry in phase.AttackEntries)
                    if (distance >= entry.MinRange && distance <= entry.MaxRange)
                        valid.Add(entry);

                if (valid.Count == 0) return null;
                return valid[Random.Range(0, valid.Count)];
            }

            float totalWeight = 0f;
            foreach (var entry in phase.AttackEntries) totalWeight += entry.RandomWeight;

            float roll = Random.Range(0f, totalWeight);
            float accumulated = 0f;

            foreach (var entry in phase.AttackEntries)
            {
                accumulated += entry.RandomWeight;
                if (roll <= accumulated) return entry;
            }

            return phase.AttackEntries[phase.AttackEntries.Count - 1];
        }

        private EnemyAttackPattern MapBossPatternToEnemyEquivalent(BossAttackPattern bossPattern)
        {
            return bossPattern switch
            {
                BossAttackPattern.MeleeHitbox_SingleSwing => EnemyAttackPattern.MeleeHitbox_SingleSwing,
                BossAttackPattern.MeleeHitbox_ComboSwing => EnemyAttackPattern.MeleeHitbox_ComboSwing,
                BossAttackPattern.RangedProjectile_StraightShot => EnemyAttackPattern.RangedProjectile_StraightShot,
                BossAttackPattern.RangedProjectile_Burst => EnemyAttackPattern.RangedProjectile_Burst,
                BossAttackPattern.RangedProjectile_Bouncing => EnemyAttackPattern.RangedProjectile_Bouncing,
                BossAttackPattern.RangedProjectile_Spread => EnemyAttackPattern.RangedProjectile_Spread,
                BossAttackPattern.RangedProjectile_Homing => EnemyAttackPattern.RangedProjectile_Homing,
                BossAttackPattern.Charge_Straight => EnemyAttackPattern.Charge_Straight,
                BossAttackPattern.Charge_WallBounce => EnemyAttackPattern.Charge_WallBounce,
                BossAttackPattern.AreaHazard_Telegraphed => EnemyAttackPattern.AreaHazard_Telegraphed,
                BossAttackPattern.AreaHazard_Persistent => EnemyAttackPattern.AreaHazard_Persistent,
                BossAttackPattern.AreaHazard_Expanding => EnemyAttackPattern.AreaHazard_Expanding,
                BossAttackPattern.AreaHazard_Multiple => EnemyAttackPattern.AreaHazard_Multiple,
                BossAttackPattern.Melee_GroundSlam
                or BossAttackPattern.Melee_SweepingArc
                or BossAttackPattern.Melee_MultiLimb
                or BossAttackPattern.Area_FullArenaHazard
                or BossAttackPattern.Area_RotatingHazard => EnemyAttackPattern.MeleeHitbox_WideArc,
                BossAttackPattern.Ranged_Barrage
                or BossAttackPattern.Ranged_Laser
                or BossAttackPattern.Ranged_Summon => EnemyAttackPattern.RangedProjectile_Burst,
                _ => EnemyAttackPattern.MeleeHitbox_SingleSwing
            };
        }

        private static AttackCategory Categorize(EnemyAttackPattern pattern)
        {
            switch (pattern)
            {
                case EnemyAttackPattern.Charge_Straight:
                case EnemyAttackPattern.Charge_WallBounce:
                    return AttackCategory.Charge;

                case EnemyAttackPattern.RangedProjectile_StraightShot:
                case EnemyAttackPattern.RangedProjectile_Burst:
                case EnemyAttackPattern.RangedProjectile_Bouncing:
                case EnemyAttackPattern.RangedProjectile_Spread:
                case EnemyAttackPattern.RangedProjectile_Homing:
                    return AttackCategory.Projectile;

                default:
                    return AttackCategory.Melee;
            }
        }

        private void ExecuteChosenAttack(EnemyAttackPattern pattern, HitboxData hitbox,
                                         ProjectileData projectile, Transform target)
        {
            switch (Categorize(pattern))
            {
                case AttackCategory.Charge:
                    StartCoroutine(ExecuteChargeAttack(target.position));
                    break;

                case AttackCategory.Melee:
                    if (hitbox != null) StartCoroutine(ExecuteMeleeAttack(hitbox));
                    else if (projectile != null) ExecuteProjectileAttack(projectile, target);
                    break;

                case AttackCategory.Projectile:
                    if (projectile != null) ExecuteProjectileAttack(projectile, target);
                    break;
            }
        }

        private IEnumerator ExecuteMeleeAttack(HitboxData data)
        {
            yield return new WaitForSeconds(data.WindupDuration);

            float timer = 0f;
            while (timer < data.ActiveDuration)
            {
                CheckHitboxImpact(data);
                timer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(data.RecoveryDuration);
        }

        private void CheckHitboxImpact(HitboxData data)
        {
            Transform origin = _hitboxOrigin != null ? _hitboxOrigin : transform;
            Vector2 center = (Vector2)origin.position + data.HitboxOffset;

            Collider2D hit = data.HitboxShape == HitboxData.Shape.Circle
                ? Physics2D.OverlapCircle(center, data.HitboxSize.x, _playerLayerMask)
                : Physics2D.OverlapBox(center, data.HitboxSize, 0f, _playerLayerMask);

            if (hit == null) return;

            DamageReceiver receiver = hit.GetComponent<DamageReceiver>();
            if (receiver == null) return;

            EventBus.Raise(new EnemyAttackEvent(data.Damage, transform.position));
            receiver.ReceiveDamage(data.Damage, transform.position);
        }

        private void ExecuteProjectileAttack(ProjectileData data, Transform target)
        {
            Vector2 baseDirection = (target.position - transform.position).normalized;
            int count = Mathf.Max(1, data.BurstCount);

            for (int i = 0; i < count; i++)
            {
                Vector2 direction = baseDirection;

                if (data.SpreadAngle > 0f && count > 1)
                {
                    float angleStep = data.SpreadAngle / (count - 1);
                    float angle = -data.SpreadAngle / 2f + angleStep * i;
                    direction = Quaternion.Euler(0, 0, angle) * baseDirection;
                }

                PoolSpawnRequestEvent request = new PoolSpawnRequestEvent(data.PoolAssetID, transform.position, default, "AttackHandler");
                EventBus.Raise(request);

                if (request.Result == null) continue;

                EnemyProjectile projectile = request.Result.GetComponent<EnemyProjectile>();
                if (projectile != null) projectile.Launch(data, direction, target);
            }
        }

        private IEnumerator ExecuteChargeAttack(Vector3 targetPosition)
        {
            if (_rb == null) yield break;

            Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;

            float chargeSpeed = _bossData != null ? _bossData.MoveSpeed * 3f
                               : _enemyData != null ? _enemyData.MoveSpeed * 3f : 10f;
            float chargeDuration = 0.6f;
            float timer = 0f;

            while (timer < chargeDuration)
            {
                _rb.linearVelocity = direction * chargeSpeed;
                timer += Time.deltaTime;
                yield return null;
            }

            _rb.linearVelocity = Vector2.zero;
        }
    }
}
