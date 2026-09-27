using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [RequireComponent(typeof(PlayerStateSystem))]
    public class PlayerCombatSystem : MonoBehaviour
    {
        [Header("Attack Data por variante")]
        [SerializeField] private AttackData _normalAttack;
        [SerializeField] private AttackData _upAttack;
        [SerializeField] private AttackData _downAttack;
        [SerializeField] private AttackData _airAttack;

        [Header("Hitbox")]
        [SerializeField] private Transform _hitboxOrigin;
        [SerializeField] private LayerMask _enemyLayerMask;

        [Header("Pacify")]
        [SerializeField] private float _pacifyRange = 1.2f;
        [SerializeField] private LayerMask _pacifyLayerMask;

        private Rigidbody2D _rb;

        private bool _isAirborne;
        private bool _enemyInPacifyRange;
        private string _weakEnemyID;

        private float _hitboxTimer;
        private float _recoveryTimer;
        private AttackData _activeAttack;

        private float _externalDamageModifier = 0f;

        private readonly HashSet<IHittable> _hitThisAttack = new HashSet<IHittable>();
        private bool _pogoAppliedThisAttack;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AttackPressedEvent>(OnAttackPressed);
            EventBus.Subscribe<PacifyPressedEvent>(OnPacifyPressed);
            EventBus.Subscribe<PlayerLandedEvent>(OnPlayerLanded);
            EventBus.Subscribe<PlayerFallingEvent>(OnPlayerFalling);
            EventBus.Subscribe<EnemyWeakenedEvent>(OnEnemyWeakened);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AttackPressedEvent>(OnAttackPressed);
            EventBus.Unsubscribe<PacifyPressedEvent>(OnPacifyPressed);
            EventBus.Unsubscribe<PlayerLandedEvent>(OnPlayerLanded);
            EventBus.Unsubscribe<PlayerFallingEvent>(OnPlayerFalling);
            EventBus.Unsubscribe<EnemyWeakenedEvent>(OnEnemyWeakened);
        }

        private void Update()
        {
            UpdateHitboxTimer();
            UpdateRecoveryTimer();
        }

        private void OnPlayerLanded(PlayerLandedEvent e) => _isAirborne = false;
        private void OnPlayerFalling(PlayerFallingEvent e) => _isAirborne = true;

        private void OnAttackPressed(AttackPressedEvent e)
        {
            if (_recoveryTimer > 0f) return;

            AttackVariant variant = ResolveVariant(e.Direction, _isAirborne);
            ExecuteAttack(variant);
        }

        private AttackVariant ResolveVariant(AttackDirection inputDirection, bool airborne)
        {
            if (airborne)
            {
                if (inputDirection == AttackDirection.Down) return AttackVariant.Down;
                return AttackVariant.Air;
            }

            return inputDirection switch
            {
                AttackDirection.Up => AttackVariant.Up,
                AttackDirection.Down => AttackVariant.Down,
                _ => AttackVariant.Normal
            };
        }

        private void ExecuteAttack(AttackVariant variant)
        {
            _activeAttack = GetAttackData(variant);
            if (_activeAttack == null) return;

            _hitboxTimer = _activeAttack.HitboxActiveDuration;
            _recoveryTimer = _activeAttack.RecoveryDuration;
            _hitThisAttack.Clear();
            _pogoAppliedThisAttack = false;

            int finalDamage = _activeAttack.Damage + Mathf.RoundToInt(_externalDamageModifier);

            EventBus.Raise(new PlayerAttackEvent(variant, finalDamage));

            CheckHitboxImpact();
        }

        private AttackData GetAttackData(AttackVariant variant) => variant switch
        {
            AttackVariant.Up => _upAttack,
            AttackVariant.Down => _downAttack,
            AttackVariant.Air => _airAttack,
            _ => _normalAttack
        };

        private void UpdateHitboxTimer()
        {
            if (_hitboxTimer <= 0f) return;

            _hitboxTimer -= Time.deltaTime;
            CheckHitboxImpact();

            if (_hitboxTimer <= 0f)
                EventBus.Raise(new AttackFinishedEvent());
        }

        private void UpdateRecoveryTimer()
        {
            if (_recoveryTimer > 0f)
                _recoveryTimer -= Time.deltaTime;
        }

        private void CheckHitboxImpact()
        {
            if (_activeAttack == null || _hitboxOrigin == null) return;

            Vector2 facingOffset = new Vector2(
                _activeAttack.HitboxOffset.x * Mathf.Sign(transform.localScale.x),
                _activeAttack.HitboxOffset.y);

            Vector2 center = (Vector2)_hitboxOrigin.position + facingOffset;
            int finalDamage = _activeAttack.Damage + Mathf.RoundToInt(_externalDamageModifier);

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, _activeAttack.HitboxSize, 0f, _enemyLayerMask);

            foreach (Collider2D hit in hits)
            {
                IHittable target = hit.GetComponent<IHittable>();
                if (target != null)
                {
                    if (!_hitThisAttack.Add(target)) continue;

                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    target.ReceiveHit(finalDamage, knockbackDir);
                    EventBus.Raise(new EnemyHitEvent(hit.gameObject, finalDamage, knockbackDir));
                    continue;
                }

                if (!_pogoAppliedThisAttack && _activeAttack.Variant == AttackVariant.Down && _activeAttack.PogoForce > 0f)
                {
                    _pogoAppliedThisAttack = true;
                    ApplyPogoBounce();
                }
            }
        }

        private void ApplyPogoBounce()
        {
            if (_rb != null)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _activeAttack.PogoForce);
        }

        private void OnEnemyWeakened(EnemyWeakenedEvent e)
        {
            _enemyInPacifyRange = true;
            _weakEnemyID = e.EnemyID;
        }

        private void OnPacifyPressed(PacifyPressedEvent e)
        {
            if (!_enemyInPacifyRange) return;

            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, _pacifyRange, _pacifyLayerMask);

            foreach (Collider2D hit in nearby)
            {
                IPacifiable pacifiable = hit.GetComponent<IPacifiable>();
                if (pacifiable != null && pacifiable.ID == _weakEnemyID)
                {
                    pacifiable.Pacify();
                    _enemyInPacifyRange = false;
                    return;
                }
            }
        }

        public void ApplyExternalDamageModifier(float amount) => _externalDamageModifier += amount;
    }
}
