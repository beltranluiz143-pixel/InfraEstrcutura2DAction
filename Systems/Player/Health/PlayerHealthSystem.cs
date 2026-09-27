using System.Collections;
using UnityEngine;

namespace Infra2DAction
{
    public class PlayerHealthSystem : MonoBehaviour
    {
        private const string HEAL_COST_KEY = "Heal";

        [Header("Vida")]
        [SerializeField] private int _maxHP = 7;

        [Header("Dano")]
        [SerializeField] private float _invulnerabilityDuration = 1f;
        [SerializeField] private float _knockbackBaseForce = 4f;
        [SerializeField] private float _knockbackLowHPMultiplier = 1.5f;

        [Header("Curacion")]
        [SerializeField] private int _healAmount = 2;
        [SerializeField] private float _healChannelDuration = 1.5f;

        [Header("Estado Critico")]
        [SerializeField] private int _criticalHPThreshold = 2;

        private int _currentHP;
        private bool _isInvulnerable;
        private bool _isHealing;
        private bool _wasCritical;
        private Coroutine _healCoroutine;

        private float _externalHealSpeedModifier = 0f;

        private Rigidbody2D _rb;
        private PlayerStateSystem _stateSystem;
        private PlayerResourceSystem _resourceSystem;
        private SaveSystem _saveSystem;

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            ApplySave(saveSystem != null ? saveSystem.GetCurrentSave() : null);
            EventBus.Raise(new PlayerHealthChangedEvent(_currentHP, _maxHP));
            CheckCriticalState();
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stateSystem = GetComponent<PlayerStateSystem>();
            _resourceSystem = GetComponent<PlayerResourceSystem>();
            _currentHP = _maxHP;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<HealPressedEvent>(OnHealPressed);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Subscribe<CollectSaveDataEvent>(OnCollectSaveData);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HealPressedEvent>(OnHealPressed);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Unsubscribe<CollectSaveDataEvent>(OnCollectSaveData);
        }

        public void TakeDamage(int damage, Vector2 knockbackDirection)
        {
            if (_isInvulnerable || _currentHP <= 0) return;

            if (_isHealing) InterruptHeal();

            _currentHP = Mathf.Max(0, _currentHP - damage);

            EventBus.Raise(new PlayerDamagedEvent(_currentHP, damage));
            EventBus.Raise(new PlayerHealthChangedEvent(_currentHP, _maxHP));

            ApplyKnockback(knockbackDirection);
            CheckCriticalState();

            if (_currentHP <= 0)
                HandleDeath();
            else
                StartCoroutine(InvulnerabilityWindow());
        }

        private void ApplyKnockback(Vector2 direction)
        {
            float lifeRatio = 1f - ((float)_currentHP / _maxHP);
            float forceScale = Mathf.Lerp(1f, _knockbackLowHPMultiplier, lifeRatio);
            float finalForce = _knockbackBaseForce * forceScale;

            _rb?.AddForce(direction.normalized * finalForce, ForceMode2D.Impulse);
        }

        private IEnumerator InvulnerabilityWindow()
        {
            _isInvulnerable = true;
            EventBus.Raise(new PlayerInvulnerabilityStartedEvent());

            yield return new WaitForSeconds(_invulnerabilityDuration);

            _isInvulnerable = false;
        }

        private void OnHealPressed(HealPressedEvent e)
        {
            if (_isHealing || _currentHP >= _maxHP) return;
            if (_resourceSystem != null && !_resourceSystem.TryConsume(HEAL_COST_KEY)) return;

            _healCoroutine = StartCoroutine(HealChannel());
        }

        private IEnumerator HealChannel()
        {
            _isHealing = true;
            EventBus.Raise(new PlayerMovementLockedEvent(true, "PlayerHealthSystem"));

            float finalDuration = Mathf.Max(0.1f, _healChannelDuration - _externalHealSpeedModifier);
            yield return new WaitForSeconds(finalDuration);

            _currentHP = Mathf.Min(_maxHP, _currentHP + _healAmount);

            EventBus.Raise(new PlayerHealedEvent(_currentHP));
            EventBus.Raise(new PlayerHealthChangedEvent(_currentHP, _maxHP));

            CheckCriticalState();

            _isHealing = false;
            EventBus.Raise(new PlayerMovementLockedEvent(false, "PlayerHealthSystem"));
        }

        private void InterruptHeal()
        {
            if (_healCoroutine != null) StopCoroutine(_healCoroutine);

            _isHealing = false;
            EventBus.Raise(new PlayerMovementLockedEvent(false, "PlayerHealthSystem"));
        }

        private void CheckCriticalState()
        {
            bool isCritical = _currentHP <= _criticalHPThreshold && _currentHP > 0;

            if (isCritical != _wasCritical)
                EventBus.Raise(new PlayerCriticalHealthEvent(isCritical));

            _wasCritical = isCritical;
        }

        private void HandleDeath()
        {
            EventBus.Raise(new PlayerDeathEvent());
            _stateSystem?.TransitionTo(PlayerStateType.Dead, true);
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            ApplySave(e.Data);
            EventBus.Raise(new PlayerHealthChangedEvent(_currentHP, _maxHP));
            CheckCriticalState();
        }

        private void ApplySave(SaveData data)
        {
            if (data?.Player == null || data.Player.CurrentHP < 0f) return;
            _currentHP = Mathf.RoundToInt(data.Player.CurrentHP);
        }

        private void OnCollectSaveData(CollectSaveDataEvent e)
        {
            if (_saveSystem == null) return;

            PlayerSaveData currentData = _saveSystem.GetCurrentSave().Player;
            currentData.CurrentHP = _currentHP > 0 ? _currentHP : -1f;
            _saveSystem.UpdatePlayerData(currentData);
        }

        public void ResetToFull()
        {
            _currentHP = _maxHP;
            EventBus.Raise(new PlayerHealthChangedEvent(_currentHP, _maxHP));
        }

        public void HealAmount(int amount)
        {
            if (amount <= 0 || _currentHP <= 0) return;

            _currentHP = Mathf.Min(_maxHP, _currentHP + amount);

            EventBus.Raise(new PlayerHealedEvent(_currentHP));
            EventBus.Raise(new PlayerHealthChangedEvent(_currentHP, _maxHP));

            CheckCriticalState();
        }

        public void ApplyExternalMaxHPModifier(int amount, bool alsoAdjustCurrent = true)
        {
            _maxHP += amount;
            _currentHP = alsoAdjustCurrent
                ? Mathf.Min(_currentHP + amount, _maxHP)
                : Mathf.Min(_currentHP, _maxHP);
            EventBus.Raise(new PlayerHealthChangedEvent(_currentHP, _maxHP));
        }

        public void ApplyExternalHealSpeedModifier(float amount) => _externalHealSpeedModifier += amount;
    }
}
