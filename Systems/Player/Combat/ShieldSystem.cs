using UnityEngine;

namespace Infra2DAction
{
    public class ShieldSystem : MonoBehaviour
    {
        [Header("Duration")]
        [SerializeField] private float _initialDuration = 1f;
        [SerializeField] private float _maxHeldDuration = 5f;

        [Header("Break Condition")]
        [SerializeField] private int _maxHitsBeforeBreak = 3;
        [SerializeField] private int _damageThresholdPerHit = 2;
        [SerializeField] private float _breakKnockbackForce = 8f;
        [SerializeField] private float _breakCooldown = 1f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _shieldSprite;

        [Header("Shield Hitbox (direccional)")]
        [SerializeField] private Transform _shieldOrigin;
        [SerializeField] private Vector2 _shieldHitboxSize = new Vector2(0.6f, 1.2f);
        [SerializeField] private LayerMask _enemyAttackLayerMask;

        private bool _isActive;
        private bool _isHeld;
        private float _activeTimer;
        private int _hitsTaken;
        private float _breakCooldownTimer;

        private void OnEnable()
        {
            EventBus.Subscribe<ShieldPressedEvent>(OnShieldPressed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ShieldPressedEvent>(OnShieldPressed);
        }

        private void Update()
        {
            if (_breakCooldownTimer > 0f) _breakCooldownTimer -= Time.deltaTime;

            if (!_isActive) return;

            if (_isHeld)
            {
                _activeTimer = Mathf.Min(_activeTimer + Time.deltaTime, _maxHeldDuration);
            }
            else
            {
                _activeTimer -= Time.deltaTime;
                if (_activeTimer <= 0f) Deactivate();
            }
        }

        private void OnShieldPressed(ShieldPressedEvent e)
        {
            if (e.IsActive)
            {
                if (_breakCooldownTimer > 0f || _isActive) return;
                Activate();
            }
            else
            {
                _isHeld = false;
            }
        }

        private void Activate()
        {
            _isActive = true;
            _isHeld = true;
            _activeTimer = _initialDuration;
            _hitsTaken = 0;
            if (_shieldSprite != null) _shieldSprite.enabled = true;

            EventBus.Raise(new PlayerMovementLockedEvent(true, "ShieldSystem"));
            EventBus.Raise(new ShieldActivatedEvent());
        }

        private void Deactivate()
        {
            _isActive = false;
            _isHeld = false;
            if (_shieldSprite != null) _shieldSprite.enabled = false;

            EventBus.Raise(new PlayerMovementLockedEvent(false, "ShieldSystem"));
            EventBus.Raise(new ShieldDeactivatedEvent());
        }

        public bool IsBlockingPoint(Vector2 attackOrigin)
        {
            if (!_isActive || _shieldOrigin == null) return false;

            Bounds shieldBounds = new Bounds(_shieldOrigin.position, _shieldHitboxSize);

            return shieldBounds.Contains(attackOrigin);
        }

        public void BlockIncomingAttack(int damage, Vector2 attackOrigin)
        {
            if (!_isActive) return;

            _hitsTaken++;
            EventBus.Raise(new SFXPlayRequestEvent("SFX_SHIELD_BLOCK", transform.position, "ShieldSystem"));

            if (_hitsTaken >= _maxHitsBeforeBreak || damage >= _damageThresholdPerHit)
                BreakShield(attackOrigin);
        }

        private void BreakShield(Vector2 attackOrigin)
        {
            Deactivate();
            _breakCooldownTimer = _breakCooldown;

            Vector2 knockbackDir = ((Vector2)transform.position - attackOrigin).normalized;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb?.AddForce(knockbackDir * _breakKnockbackForce, ForceMode2D.Impulse);

            EventBus.Raise(new ShieldBrokenEvent());
        }

        public bool IsShieldActive() => _isActive;

        private void OnDrawGizmosSelected()
        {
            if (_shieldOrigin == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(_shieldOrigin.position, _shieldHitboxSize);
        }
    }
}
