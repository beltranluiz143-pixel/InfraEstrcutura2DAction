using UnityEngine;

namespace Infra2DAction
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemySystem : MonoBehaviour, IHittable, IPacifiable, IDamageSource
    {
        private const string ID_PLACEHOLDER = "{ID}";

        [Header("Data (asignar UNO de los dos)")]
        [SerializeField] private EnemyData _enemyData;
        [SerializeField] private BossData _bossData;

        [Header("Boss Persistence")]
        [Tooltip("Flag que marca al boss como derrotado. {ID} se sustituye por el BossID.")]
        [SerializeField] private string _defeatedFlagKeyFormat = "BOSS_{ID}_DEFEATED";

        [Header("Visual")]
        [SerializeField] private Animator _animator;

        private int _currentHP;
        private bool _isDead;
        private bool _isPacifiable;

        private AIController _aiController;

        public bool IsBoss => _bossData != null;
        public int CurrentHP => _currentHP;
        public int MaxHP => IsBoss ? _bossData.MaxHP : _enemyData.MaxHP;
        public string ID => IsBoss ? _bossData.BossID : _enemyData.EnemyID;
        public float PacificationThreshold => IsBoss ? _bossData.PacificationThreshold : _enemyData.PacificationThreshold;

        private void Awake()
        {
            if (_enemyData == null && _bossData == null)
            {
                DebugSystem.LogError($"EnemySystem '{gameObject.name}' sin EnemyData ni BossData.", "AI", "EnemySystem");
                enabled = false;
                return;
            }

            _aiController = GetComponent<AIController>();

            if (IsBoss && WasBossDefeated())
            {
                gameObject.SetActive(false);
                return;
            }

            _currentHP = MaxHP;

            int animIndex = IsBoss ? 0 : _enemyData.AnimationStateIndex;
            if (_animator != null) _animator.SetInteger("StateIndex", animIndex);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<MinigameDamageBossRequestEvent>(OnMinigameDamageBoss);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MinigameDamageBossRequestEvent>(OnMinigameDamageBoss);
        }

        private void OnMinigameDamageBoss(MinigameDamageBossRequestEvent e)
        {
            if (!IsBoss || _isDead) return;

            ReceiveHit(e.Amount, Vector2.zero);
        }

        private bool WasBossDefeated()
        {
            if (string.IsNullOrEmpty(_defeatedFlagKeyFormat)) return false;

            FlagQueryEvent query = new FlagQueryEvent(_defeatedFlagKeyFormat.Replace(ID_PLACEHOLDER, _bossData.BossID), "EnemySystem");
            EventBus.Raise(query);
            return query.Handled && query.Result;
        }

        public void ReceiveHit(int damage, Vector2 knockbackDirection)
        {
            if (_isDead) return;

            _currentHP = Mathf.Max(0, _currentHP - damage);

            if (_animator != null) _animator.SetTrigger("Pulse");

            CheckPacificationThreshold();

            if (_currentHP <= 0)
                Die();
        }

        private void CheckPacificationThreshold()
        {
            if (_isPacifiable || _isDead) return;
            if (PacificationThreshold <= 0f) return;

            float hpRatio = (float)_currentHP / MaxHP;
            if (hpRatio <= PacificationThreshold)
            {
                _isPacifiable = true;
                EventBus.Raise(new EnemyWeakenedEvent(ID));
                if (_aiController != null) _aiController.TransitionTo(AIState.Pacifiable);
            }
        }

        public void Pacify()
        {
            if (_isDead || !_isPacifiable) return;

            _isDead = true;
            DropLootIfConfigured();

            EventBus.Raise(new EnemyPacifiedEvent(ID));
            if (IsBoss) EventBus.Raise(new BossDefeatedEvent(ID));

            gameObject.SetActive(false);
        }

        private void Die()
        {
            _isDead = true;
            DropLootIfConfigured();

            if (IsBoss)
                EventBus.Raise(new BossDefeatedEvent(ID));
            else
                EventBus.Raise(new EnemyKilledEvent(ID));

            gameObject.SetActive(false);
        }

        private void DropLootIfConfigured()
        {
            bool dropsLoot = IsBoss ? _bossData.DropsLoot : _enemyData.DropsLoot;
            string poolID = IsBoss ? _bossData.DropPoolID : _enemyData.DropPoolID;

            if (!dropsLoot || string.IsNullOrEmpty(poolID)) return;

            EventBus.Raise(new PoolSpawnRequestEvent(poolID, transform.position, default, "EnemySystem"));
        }

        public int GetDamage() => 0;
        public Vector2 GetKnockbackOrigin() => transform.position;
    }
}
