using UnityEngine;

namespace Infra2DAction
{
    public class PlayerCurrencySystem : MonoBehaviour
    {
        private int _currentAmount;
        private SaveSystem _saveSystem;

        public int CurrentAmount => _currentAmount;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;

            SaveData data = saveSystem != null ? saveSystem.GetCurrentSave() : null;
            if (data?.Player != null) _currentAmount = data.Player.Coins;

            EventBus.Raise(new CurrencyChangedEvent(_currentAmount));
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Subscribe<CurrencyAddRequestEvent>(OnAddRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Unsubscribe<CurrencyAddRequestEvent>(OnAddRequested);
        }

        private void OnAddRequested(CurrencyAddRequestEvent e)
        {
            if (e.Amount >= 0) AddCurrency(e.Amount);
            else TrySpend(-e.Amount);
        }

        public void AddCurrency(int amount)
        {
            _currentAmount += amount;
            EventBus.Raise(new CurrencyChangedEvent(_currentAmount));
        }

        public bool TrySpend(int amount)
        {
            if (_currentAmount < amount) return false;

            _currentAmount -= amount;
            EventBus.Raise(new CurrencySpentEvent(amount));
            EventBus.Raise(new CurrencyChangedEvent(_currentAmount));
            return true;
        }

        private void OnCollectSaveData(CollectSaveDataEvent e)
        {
            if (_saveSystem == null) return;

            PlayerSaveData data = _saveSystem.GetCurrentSave().Player;
            data.Coins = _currentAmount;
            _saveSystem.UpdatePlayerData(data);
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            if (e.Data?.Player == null) return;

            _currentAmount = e.Data.Player.Coins;
            EventBus.Raise(new CurrencyChangedEvent(_currentAmount));
        }
    }
}
