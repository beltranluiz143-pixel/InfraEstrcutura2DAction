using UnityEngine;

namespace Infra2DAction
{
    public class EnemyProjectile : MonoBehaviour, IDamageSource
    {
        [SerializeField] private LayerMask _groundMask;

        private ProjectileData _data;
        private Vector2 _direction;
        private Transform _homingTarget;
        private float _lifeTimer;
        private int _bounces;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(ProjectileData data, Vector2 direction, Transform homingTarget)
        {
            _data = data;
            _direction = direction;
            _homingTarget = homingTarget;
            _lifeTimer = data.Lifetime;
            _bounces = 0;
        }

        private void Update()
        {
            if (_data == null) return;

            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f) { Despawn(); return; }

            if (_data.HomingStrength > 0f && _homingTarget != null)
            {
                Vector2 toTarget = ((Vector2)_homingTarget.position - (Vector2)transform.position).normalized;
                _direction = Vector2.Lerp(_direction, toTarget, _data.HomingStrength * Time.deltaTime).normalized;
            }

            if (_rb != null) _rb.linearVelocity = _direction * _data.Speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_data == null) return;

            if (other.CompareTag("Player"))
            {
                DamageReceiver receiver = other.GetComponent<DamageReceiver>();
                if (receiver != null) receiver.ReceiveDamage(_data.Damage, transform.position);
                Despawn();
                return;
            }

            bool isGround = ((1 << other.gameObject.layer) & _groundMask) != 0;
            if (!isGround) return;

            if (_bounces < _data.BounceCount)
            {
                _bounces++;
                _direction = Vector2.Reflect(_direction, Vector2.up);
            }
            else
            {
                Despawn();
            }
        }

        private void Despawn()
        {
            PooledObject pooled = GetComponent<PooledObject>();
            if (pooled != null) pooled.Despawn();
            else Destroy(gameObject);
        }

        public int GetDamage() => _data != null ? _data.Damage : 0;
        public Vector2 GetKnockbackOrigin() => transform.position;
    }
}
