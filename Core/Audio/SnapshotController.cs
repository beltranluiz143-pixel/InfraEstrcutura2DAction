using UnityEngine;
using UnityEngine.Audio;

namespace Infra2DAction
{
    public class SnapshotController : MonoBehaviour
    {
        [Header("Mixer Snapshots")]
        [SerializeField] private AudioMixerSnapshot _defaultSnapshot;
        [SerializeField] private AudioMixerSnapshot _pauseSnapshot;
        [SerializeField] private AudioMixerSnapshot _dialogueSnapshot;
        [SerializeField] private AudioMixerSnapshot _combatSnapshot;

        [SerializeField] private float _transitionTime = 0.5f;

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            switch (e.NewState)
            {
                case GameStateType.Pause:
                    Apply(_pauseSnapshot, "Pause");
                    break;
                case GameStateType.Dialogue:
                    Apply(_dialogueSnapshot, "Dialogue");
                    break;
                case GameStateType.Gameplay:
                case GameStateType.Dream:
                    Apply(_defaultSnapshot, "Default");
                    break;
            }
        }

        public void ApplyCombatSnapshot() => Apply(_combatSnapshot, "Combat");

        private void Apply(AudioMixerSnapshot snapshot, string name)
        {
            if (snapshot == null)
            {
                DebugSystem.LogWarning($"Snapshot '{name}' no asignado.", "Audio", "SnapshotController");
                return;
            }

            snapshot.TransitionTo(_transitionTime);
            EventBus.Raise(new SnapshotChangedEvent(name));
        }
    }
}
