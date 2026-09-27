using UnityEngine;

namespace Infra2DAction
{
    [RequireComponent(typeof(Collider2D))]
    public class DamageReceiver : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LayerMask _hazardLayerMask;

        private PlayerHealthSystem _healthSystem;
        private ShieldSystem _shieldSystem;

        private void Awake()
        {
            _healthSystem = GetComponent<PlayerHealthSystem>();
            _shieldSystem = GetComponent<ShieldSystem>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & _hazardLayerMask) == 0) return;

            IDamageSource source = other.GetComponent<IDamageSource>();
            if (source == null) return;

            ReceiveDamage(source.GetDamage(), source.GetKnockbackOrigin());
        }

        public void ReceiveDamage(int damage, Vector2 knockbackOrigin)
        {
            if (_shieldSystem != null && _shieldSystem.IsBlockingPoint(knockbackOrigin))
            {
                _shieldSystem.BlockIncomingAttack(damage, knockbackOrigin);
                return;
            }

            Vector2 knockbackDirection = ((Vector2)transform.position - knockbackOrigin).normalized;
            _healthSystem?.TakeDamage(damage, knockbackDirection);
        }
    }
}
