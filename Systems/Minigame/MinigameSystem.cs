using UnityEngine;

namespace Infra2DAction
{
    public class MinigameSystem : MonoBehaviour
    {
        private GameObject _activeInstance;
        private MinigameData _activeData;
        private PlayerSystem _player;

        private void OnEnable()
        {
            EventBus.Subscribe<MinigameStartRequestEvent>(OnMinigameStartRequested);
            EventBus.Subscribe<MinigameEndedEvent>(OnMinigameEnded);
            EventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeathDuringMinigame);
            EventBus.Subscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MinigameStartRequestEvent>(OnMinigameStartRequested);
            EventBus.Unsubscribe<MinigameEndedEvent>(OnMinigameEnded);
            EventBus.Unsubscribe<PlayerDeathEvent>(OnPlayerDeathDuringMinigame);
            EventBus.Unsubscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent e)
        {
            _player = e.PlayerObject.GetComponent<PlayerSystem>();
        }

        private void OnMinigameStartRequested(MinigameStartRequestEvent e)
        {
            if (e.Data == null || e.Data.Prefab == null) return;
            if (_activeInstance != null) return;

            _activeData = e.Data;

            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Minigame, "MinigameSystem"));

            _activeInstance = Instantiate(e.Data.Prefab);

            IMinigame minigame = _activeInstance.GetComponent<IMinigame>();
            if (minigame == null)
            {
                DebugSystem.LogError($"Prefab de minijuego {e.Data.MinigameID} no implementa IMinigame.",
                                     "Minigame", "MinigameSystem");
                CleanupInstance();
                EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Gameplay, "MinigameSystem"));
                _activeData = null;
                return;
            }

            minigame.StartGame(e.Data);
            EventBus.Raise(new MinigameStartedEvent());
        }

        private void OnMinigameEnded(MinigameEndedEvent e)
        {
            if (_activeData == null) return;

            ApplyResult(e.Success);
            CleanupInstance();

            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Gameplay, "MinigameSystem"));

            _activeData = null;
        }

        private void ApplyResult(bool success)
        {
            if (success)
            {
                switch (_activeData.RewardType)
                {
                    case MinigameRewardType.Heal:
                        if (_player != null && _player.HealthSystem != null)
                            _player.HealthSystem.HealAmount(_activeData.RewardAmount);
                        break;

                    case MinigameRewardType.GiveCurrency:
                        EventBus.Raise(new CurrencyAddRequestEvent(_activeData.RewardAmount, "MinigameSystem"));
                        break;

                    case MinigameRewardType.DamageBoss:
                        EventBus.Raise(new MinigameDamageBossRequestEvent(_activeData.RewardAmount));
                        break;
                }
            }
            else if (_player != null)
            {
                DamageReceiver receiver = _player.GetComponent<DamageReceiver>();
                if (receiver != null)
                    receiver.ReceiveDamage(_activeData.FailureDamage, _player.transform.position);
            }
        }

        private void OnPlayerDeathDuringMinigame(PlayerDeathEvent e)
        {
            if (_activeData == null) return;

            CleanupInstance();
            _activeData = null;
        }

        private void CleanupInstance()
        {
            if (_activeInstance == null) return;

            IMinigame minigame = _activeInstance.GetComponent<IMinigame>();
            if (minigame != null) minigame.EndGame();

            Destroy(_activeInstance);
            _activeInstance = null;
        }
    }
}
