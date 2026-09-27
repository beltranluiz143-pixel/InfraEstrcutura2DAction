using UnityEngine;

namespace Infra2DAction
{
    public class PlayerSystem : MonoBehaviour
    {
        [Header("Player Components")]
        [SerializeField] private PlayerAbilityUnlocks _abilityUnlocks;
        [SerializeField] private PlayerHealthSystem _healthSystem;
        [SerializeField] private PlayerResourceSystem _resourceSystem;
        [SerializeField] private PlayerInventorySystem _inventorySystem;
        [SerializeField] private PlayerCurrencySystem _currencySystem;

        public PlayerInventorySystem InventorySystem => _inventorySystem;
        public PlayerResourceSystem ResourceSystem => _resourceSystem;
        public PlayerHealthSystem HealthSystem => _healthSystem;
        public PlayerCurrencySystem CurrencySystem => _currencySystem;

        private void OnEnable()
        {
            EventBus.Subscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Raise(new PlayerRegisteredEvent(gameObject));
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CoreInitializedEvent>(OnCoreInitialized);
        }

        private void OnCoreInitialized(CoreInitializedEvent e)
        {
            EventBus.Raise(new PlayerRegisteredEvent(gameObject));
        }

        public void Initialize(GlobalVariablesSystem globalVars, SaveSystem saveSystem, ItemDatabase itemDatabase)
        {
            _abilityUnlocks?.Initialize(globalVars);
            _healthSystem?.Initialize(saveSystem);
            _resourceSystem?.Initialize(saveSystem);
            _inventorySystem?.Initialize(itemDatabase, saveSystem);
            _currencySystem?.Initialize(saveSystem);

            DebugSystem.Log("PlayerSystem initialized.", "Player", "PlayerSystem");
        }
    }
}
