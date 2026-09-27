using UnityEngine;

namespace Infra2DAction
{
    [RequireComponent(typeof(Collider2D))]
    public class CutsceneTrigger : MonoBehaviour
    {
        [Header("Cutscene")]
        [SerializeField] private CutsceneData _cutscene;

        [Header("Condicion (opcional)")]
        [SerializeField] private FlagCondition _requiredCondition;

        [Header("Reproduccion")]
        [SerializeField] private bool _playOnce = true;
        [Tooltip("Flag que se marca al reproducirse y evita repetirla entre sesiones. Vacio = solo se controla en memoria.")]
        [SerializeField] private string _playedFlagKey;

        private bool _hasPlayed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || _cutscene == null) return;
            if (_playOnce && _hasPlayed) return;
            if (!IsConditionMet() || WasAlreadyPlayed()) return;

            _hasPlayed = true;

            if (!string.IsNullOrEmpty(_playedFlagKey))
                EventBus.Raise(new FlagSetRequestEvent(_playedFlagKey, true, "CutsceneTrigger"));

            EventBus.Raise(new CutsceneStartRequestEvent(_cutscene, "CutsceneTrigger"));
        }

        private bool IsConditionMet()
        {
            if (_requiredCondition == null) return true;

            ConditionQueryEvent query = new ConditionQueryEvent(_requiredCondition, "CutsceneTrigger");
            EventBus.Raise(query);
            return !query.Handled || query.Result;
        }

        private bool WasAlreadyPlayed()
        {
            if (string.IsNullOrEmpty(_playedFlagKey)) return false;

            FlagQueryEvent query = new FlagQueryEvent(_playedFlagKey, "CutsceneTrigger");
            EventBus.Raise(query);
            return query.Handled && query.Result;
        }
    }
}
