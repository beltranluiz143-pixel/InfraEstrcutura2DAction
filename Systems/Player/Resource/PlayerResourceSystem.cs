using UnityEngine;

namespace Infra2DAction
{
    public class PlayerResourceSystem : MonoBehaviour
    {
        private const string SHIELD_DRAIN_KEY = "ShieldPerSecond";

        [Header("Pool")]
        [SerializeField] private float _maxResource = 100f;
        [SerializeField] private float _regenPerSecond = 3f;
        [SerializeField] private float _regenDelayAfterUse = 1.5f;

        [Header("Consumption Table")]
        [SerializeField] private ResourceConsumptionTable _consumptionTable;

        private float _currentResource;
        private float _regenCooldownTimer;
        private bool _isShieldActive;

        private SaveSystem _saveSystem;

        public float CurrentResource => _currentResource;
        public float MaxResource => _maxResource;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            ApplySave(saveSystem != null ? saveSystem.GetCurrentSave() : null);
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));
        }

        private void Awake()
        {
            _currentResource = _maxResource;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ResourceCollectRequestEvent>(OnCollectRequested);
            EventBus.Subscribe<ResourceConsumeRequestEvent>(OnConsumeRequested);
            EventBus.Subscribe<ResourceFullDrainRequestEvent>(OnFullDrainRequested);
            EventBus.Subscribe<ShieldActivatedEvent>(OnShieldActivated);
            EventBus.Subscribe<ShieldDeactivatedEvent>(OnShieldDeactivated);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Subscribe<CollectSaveDataEvent>(OnCollectSaveData);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ResourceCollectRequestEvent>(OnCollectRequested);
            EventBus.Unsubscribe<ResourceConsumeRequestEvent>(OnConsumeRequested);
            EventBus.Unsubscribe<ResourceFullDrainRequestEvent>(OnFullDrainRequested);
            EventBus.Unsubscribe<ShieldActivatedEvent>(OnShieldActivated);
            EventBus.Unsubscribe<ShieldDeactivatedEvent>(OnShieldDeactivated);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Unsubscribe<CollectSaveDataEvent>(OnCollectSaveData);
        }

        private void Update()
        {
            UpdateRegen();
            UpdateShieldDrain();
        }

        public bool TryConsume(string key)
        {
            float cost = _consumptionTable != null ? _consumptionTable.GetCost(key) : 0f;
            return TryConsumeAmount(cost, key);
        }

        public bool TryConsumeAmount(float amount, string reason)
        {
            if (amount <= 0f) return true;

            if (_currentResource < amount)
            {
                EventBus.Raise(new ResourceDepletedEvent(reason));
                return false;
            }

            _currentResource -= amount;
            _regenCooldownTimer = _regenDelayAfterUse;

            EventBus.Raise(new ResourceConsumedEvent(reason, amount));
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));

            return true;
        }

        public void Collect(float amount)
        {
            _currentResource = Mathf.Min(_maxResource, _currentResource + amount);
            EventBus.Raise(new ResourceCollectedEvent(amount));
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));
        }

        private void OnCollectRequested(ResourceCollectRequestEvent e) => Collect(e.Amount);

        private void OnConsumeRequested(ResourceConsumeRequestEvent e) => e.SetResult(TryConsumeAmount(e.Amount, e.Reason));

        private void OnFullDrainRequested(ResourceFullDrainRequestEvent e)
        {
            _currentResource = 0f;
            _regenCooldownTimer = _regenDelayAfterUse;

            EventBus.Raise(new ResourceConsumedEvent($"FullDrain_{e.Reason}", _maxResource));
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));
        }

        private void UpdateRegen()
        {
            if (_regenCooldownTimer > 0f)
            {
                _regenCooldownTimer -= Time.deltaTime;
                return;
            }

            if (_currentResource >= _maxResource) return;

            _currentResource = Mathf.Min(_maxResource, _currentResource + _regenPerSecond * Time.deltaTime);
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));
        }

        private void OnShieldActivated(ShieldActivatedEvent e) => _isShieldActive = true;
        private void OnShieldDeactivated(ShieldDeactivatedEvent e) => _isShieldActive = false;

        private void UpdateShieldDrain()
        {
            if (!_isShieldActive || _consumptionTable == null) return;

            float costPerSecond = _consumptionTable.GetCost(SHIELD_DRAIN_KEY);
            if (costPerSecond <= 0f) return;

            _currentResource = Mathf.Max(0f, _currentResource - costPerSecond * Time.deltaTime);
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));

            if (_currentResource <= 0f)
                EventBus.Raise(new ResourceDepletedEvent(SHIELD_DRAIN_KEY));
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            ApplySave(e.Data);
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));
        }

        private void ApplySave(SaveData data)
        {
            if (data?.Player == null || data.Player.CurrentResource < 0f) return;
            _currentResource = data.Player.CurrentResource;
        }

        private void OnCollectSaveData(CollectSaveDataEvent e)
        {
            if (_saveSystem == null) return;

            PlayerSaveData currentData = _saveSystem.GetCurrentSave().Player;
            currentData.CurrentResource = _currentResource;
            _saveSystem.UpdatePlayerData(currentData);
        }

        public void ApplyExternalMaxResourceModifier(float amount)
        {
            _maxResource += amount;
            EventBus.Raise(new ResourceChangedEvent(_currentResource, _maxResource));
        }
    }
}
