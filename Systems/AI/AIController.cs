using System.Collections;
using UnityEngine;

namespace Infra2DAction
{
    public class AIController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EnemyData _enemyData;
        [SerializeField] private BossData _bossData;

        [Header("Boss Predefined Path (solo si MovementPattern = PredefinedPath_HitboxGuided)")]
        [SerializeField] private Transform[] _predefinedPathPoints;

        [Header("Boss Flying Settings")]
        [SerializeField] private float _erraticRadius = 3f;
        [SerializeField] private float _erraticChangeInterval = 1.5f;
        [SerializeField] private float _divingHeight = 4f;
        [SerializeField] private float _divingSpeed = 12f;
        [SerializeField] private float _orbitRadius = 3f;
        [SerializeField] private float _orbitSpeed = 60f;

        [Header("Boss Charge Settings")]
        [SerializeField] private int _chargeComboCount = 3;
        [SerializeField] private float _chargeComboPause = 0.4f;

        [Header("References")]
        [SerializeField] private Transform _detectionOrigin;
        [SerializeField] private LayerMask _playerLayerMask;

        private AIState _currentState = AIState.Idle;
        private EnemySystem _enemySystem;
        private AttackHandler _attackHandler;
        private Rigidbody2D _rb;
        private Animator _animator;

        private Transform _playerTransform;
        private Vector3 _originPosition;
        private int _currentPathIndex;
        private bool _pathReversing;
        private float _attackCooldownTimer;

        private int _currentPhaseIndex;
        private bool _isBoss;

        private Vector3 _erraticTarget;
        private float _erraticTimer;

        private bool _isDiving;

        private float _orbitAngle;

        private bool _isCharging;

        public bool IsBoss => _isBoss;
        public int CurrentPhaseIndex => _currentPhaseIndex;

        private float AttackRange => _isBoss ? _bossData.AttackRange : _enemyData.AttackRange;
        private float MoveSpeed => _isBoss ? _bossData.MoveSpeed : _enemyData.MoveSpeed;

        private void Awake()
        {
            _isBoss = _bossData != null;

            if (!_isBoss && _enemyData == null)
            {
                DebugSystem.LogError($"AIController '{gameObject.name}' sin EnemyData ni BossData.", "AI", "AIController");
                enabled = false;
                return;
            }

            _enemySystem = GetComponent<EnemySystem>();
            _attackHandler = GetComponent<AttackHandler>();
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();

            _originPosition = transform.position;
            _currentState = StartingState();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
        }

        private AIState StartingState()
        {
            if (_isBoss) return AIState.Patrol;
            return _enemyData.PathPoints.Count > 0 ? AIState.Patrol : AIState.Idle;
        }

        private void OnSceneLoaded(SceneLoadedEvent e)
        {
            if (_currentState != AIState.Pacifiable)
                TransitionTo(StartingState());
        }

        private void Update()
        {
            if (_currentState == AIState.Dead || _currentState == AIState.Pacifiable) return;

            DetectPlayer();
            UpdateStateBehavior();

            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;

            if (_isBoss) CheckPhaseTransition();
        }

        private void DetectPlayer()
        {
            float detectionRadius = _isBoss ? _bossData.DetectionRadius : _enemyData.DetectionRadius;
            Transform origin = _detectionOrigin != null ? _detectionOrigin : transform;

            Collider2D hit = Physics2D.OverlapCircle(origin.position, detectionRadius, _playerLayerMask);

            if (hit != null)
            {
                _playerTransform = hit.transform;
                if (_currentState == AIState.Idle || _currentState == AIState.Patrol)
                    TransitionTo(AIState.Chase);
            }
        }

        private void UpdateStateBehavior()
        {
            switch (_currentState)
            {
                case AIState.Patrol: ApplyPatrolMovement(); break;
                case AIState.Chase: ApplyChaseState(); break;
                case AIState.Attack: TryAttack(); break;
            }
        }

        private void ApplyPatrolMovement()
        {
            if (_isBoss)
            {
                ApplyBossMovement();
                return;
            }

            if (_enemyData.PathPoints.Count == 0) return;

            Vector3 target = _originPosition + (Vector3)_enemyData.PathPoints[_currentPathIndex];
            transform.position = Vector3.MoveTowards(transform.position, target, _enemyData.MoveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f)
                AdvancePathIndex(_enemyData.PathPoints.Count, _enemyData.PathPingPong);
        }

        private void ApplyChaseState()
        {
            if (_playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, _playerTransform.position);
            if (distance <= AttackRange)
            {
                TransitionTo(AIState.Attack);
                return;
            }

            if (_isBoss) ApplyBossMovement();
            else ApplyChaseMovement();
        }

        private void ApplyBossMovement()
        {
            if (_bossData.Phases.Count == 0) return;

            BossMovementPattern pattern = _bossData.Phases[_currentPhaseIndex].MovementPattern;

            switch (pattern)
            {
                case BossMovementPattern.Stationary_Throne:
                case BossMovementPattern.Stationary_Pedestal:
                case BossMovementPattern.SummonAnchor_Fixed:
                case BossMovementPattern.PhaseShift_Vanish:
                    break;

                case BossMovementPattern.PredefinedPath_HitboxGuided:
                case BossMovementPattern.Patrol_ArenaPerimeter:
                case BossMovementPattern.Patrol_ZoneToZone:
                    ApplyPredefinedPathMovement();
                    break;

                case BossMovementPattern.Viewing_DirectApproach:
                case BossMovementPattern.GroundChase_Relentless:
                    ApplyChaseMovement();
                    break;

                case BossMovementPattern.GroundChase_Teleporting:
                    ApplyTeleportingChase();
                    break;

                case BossMovementPattern.Flying_Erratic:
                    ApplyFlyingErratic();
                    break;

                case BossMovementPattern.Flying_Diving:
                    ApplyFlyingDiving();
                    break;

                case BossMovementPattern.Flying_Orbiting:
                    ApplyFlyingOrbiting();
                    break;

                case BossMovementPattern.Charge_MultiDirectional:
                    ApplyChargeMultiDirectional();
                    break;

                case BossMovementPattern.Charge_Combo:
                    ApplyChargeCombo();
                    break;
            }
        }

        private void ApplyFlyingErratic()
        {
            _erraticTimer -= Time.deltaTime;
            if (_erraticTimer <= 0f)
            {
                _erraticTarget = _originPosition + (Vector3)(Random.insideUnitCircle * _erraticRadius);
                _erraticTimer = _erraticChangeInterval;
            }

            transform.position = Vector3.MoveTowards(transform.position, _erraticTarget, _bossData.MoveSpeed * Time.deltaTime);
        }

        private void ApplyFlyingDiving()
        {
            if (!_isDiving)
            {
                Vector3 ascendTarget = _originPosition + Vector3.up * _divingHeight;
                transform.position = Vector3.MoveTowards(transform.position, ascendTarget, _bossData.MoveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, ascendTarget) < 0.2f)
                    _isDiving = true;
            }
            else
            {
                if (_playerTransform == null) { _isDiving = false; return; }

                Vector3 direction = (_playerTransform.position - transform.position).normalized;
                transform.position += direction * _divingSpeed * Time.deltaTime;

                float distance = Vector3.Distance(transform.position, _playerTransform.position);
                if (distance < 0.3f) _isDiving = false;
            }
        }

        private void ApplyFlyingOrbiting()
        {
            _orbitAngle += _orbitSpeed * Time.deltaTime;
            float radians = _orbitAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * _orbitRadius;
            transform.position = _originPosition + offset;
        }

        private void ApplyChargeMultiDirectional()
        {
            if (_isCharging) return;
            if (_playerTransform == null) { ApplyChaseMovement(); return; }

            float distance = Vector2.Distance(transform.position, _playerTransform.position);

            if (distance <= _bossData.AttackRange * 2f)
                StartCoroutine(ExecuteSingleCharge());
            else
                ApplyChaseMovement();
        }

        private void ApplyChargeCombo()
        {
            if (_isCharging) return;
            if (_playerTransform == null) { ApplyChaseMovement(); return; }

            float distance = Vector2.Distance(transform.position, _playerTransform.position);

            if (distance <= _bossData.AttackRange * 2f)
                StartCoroutine(ExecuteChargeComboSequence());
            else
                ApplyChaseMovement();
        }

        private IEnumerator ExecuteSingleCharge()
        {
            _isCharging = true;
            yield return ChargeTowardsPlayer();
            _isCharging = false;
        }

        private IEnumerator ExecuteChargeComboSequence()
        {
            _isCharging = true;

            for (int i = 0; i < _chargeComboCount; i++)
            {
                yield return ChargeTowardsPlayer();
                yield return new WaitForSeconds(_chargeComboPause);
            }

            _isCharging = false;
        }

        private IEnumerator ChargeTowardsPlayer()
        {
            if (_playerTransform == null || _rb == null) yield break;

            Vector2 direction = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
            float chargeSpeed = _bossData.MoveSpeed * 3f;
            float duration = 0.5f;
            float timer = 0f;

            while (timer < duration)
            {
                _rb.linearVelocity = direction * chargeSpeed;
                timer += Time.deltaTime;
                yield return null;
            }

            _rb.linearVelocity = Vector2.zero;
        }

        private void ApplyPredefinedPathMovement()
        {
            if (_predefinedPathPoints == null || _predefinedPathPoints.Length == 0) return;

            Transform target = _predefinedPathPoints[_currentPathIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, _bossData.MoveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
                AdvancePathIndex(_predefinedPathPoints.Length, false);
        }

        private void ApplyTeleportingChase()
        {
            if (_playerTransform == null) return;

            transform.position = _playerTransform.position + (Vector3)(Random.insideUnitCircle * 2f);
        }

        private void AdvancePathIndex(int count, bool pingPong)
        {
            if (pingPong)
            {
                if (!_pathReversing && _currentPathIndex >= count - 1) _pathReversing = true;
                else if (_pathReversing && _currentPathIndex <= 0) _pathReversing = false;
                _currentPathIndex += _pathReversing ? -1 : 1;
            }
            else
            {
                _currentPathIndex = (_currentPathIndex + 1) % count;
            }
        }

        private void ApplyChaseMovement()
        {
            if (_playerTransform == null) return;

            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            transform.position += direction * MoveSpeed * Time.deltaTime;
        }

        private void TryAttack()
        {
            if (_attackCooldownTimer > 0f) return;
            if (_playerTransform == null) { TransitionTo(AIState.Chase); return; }

            float distance = Vector2.Distance(transform.position, _playerTransform.position);

            if (distance > AttackRange)
            {
                TransitionTo(AIState.Chase);
                return;
            }

            if (_attackHandler != null)
            {
                if (_isBoss) _attackHandler.ExecuteBossAttack(_playerTransform, _currentPhaseIndex);
                else _attackHandler.ExecuteAttack(_playerTransform);
            }

            _attackCooldownTimer = _isBoss ? _bossData.AttackCooldown : _enemyData.AttackCooldown;
        }

        private void CheckPhaseTransition()
        {
            if (_bossData.Phases.Count == 0 || _enemySystem == null || _enemySystem.MaxHP <= 0) return;

            float hpRatio = (float)_enemySystem.CurrentHP / _enemySystem.MaxHP;

            int targetPhase = _currentPhaseIndex;
            for (int i = _bossData.Phases.Count - 1; i >= 0; i--)
            {
                if (hpRatio <= _bossData.Phases[i].HPThresholdPercent)
                {
                    targetPhase = i;
                    break;
                }
            }

            if (targetPhase == _currentPhaseIndex) return;

            _currentPhaseIndex = targetPhase;
            _currentPathIndex = 0;
            _isDiving = false;
            _isCharging = false;

            EventBus.Raise(new BossPhaseChangedEvent(_enemySystem.ID, targetPhase));

            if (_animator != null)
                _animator.SetInteger("StateIndex", _bossData.Phases[targetPhase].AnimationStateIndex);
        }

        public void TransitionTo(AIState newState)
        {
            if (_currentState == newState) return;

            AIState previous = _currentState;
            _currentState = newState;

            EventBus.Raise(new EnemyStateChangedEvent(previous, newState));
        }

        public AIState GetCurrentState() => _currentState;
    }
}
