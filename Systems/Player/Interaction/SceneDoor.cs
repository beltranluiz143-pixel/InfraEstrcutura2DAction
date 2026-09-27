using UnityEngine;

namespace Infra2DAction
{
    [RequireComponent(typeof(Collider2D))]
    public class SceneDoor : MonoBehaviour, IInteractable
    {
        [Header("Destino")]
        [SerializeField] private SceneReference _targetScene;
        [SerializeField] private string _targetSpawnPointID = "Default";
        [SerializeField] private Color _fadeColor = Color.black;

        [Header("Comportamiento")]
        [Tooltip("Activo: basta con tocar la puerta (collider como Trigger). " +
                 "Desactivo: hay que interactuar y el GameObject debe estar en la layer Interactable.")]
        [SerializeField] private bool _triggerOnTouch = true;

        [Header("Bloqueo (opcional)")]
        [SerializeField] private FlagCondition _requiredCondition;
        [Tooltip("Si es mayor que 0, cruzar la puerta cuesta esta cantidad de recurso del jugador.")]
        [SerializeField] private float _resourceCost = 0f;
        [SerializeField] private string _sfxOnLocked = "SFX_DOOR_LOCKED";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_triggerOnTouch || !other.CompareTag("Player")) return;
            Cross();
        }

        public void OnInteract() => Cross();

        private void Cross()
        {
            if (_targetScene == null)
            {
                DebugSystem.LogError($"SceneDoor '{gameObject.name}' sin escena de destino asignada.",
                                     "SceneFlow", "SceneDoor");
                return;
            }

            if (!IsConditionMet())
            {
                if (!string.IsNullOrEmpty(_sfxOnLocked))
                    EventBus.Raise(new SFXPlayRequestEvent(_sfxOnLocked, transform.position, "SceneDoor"));
                return;
            }

            if (_resourceCost > 0f && !TryPayResourceCost())
                return;

            EventBus.Raise(new SceneTransitionRequestEvent(_targetScene, _targetSpawnPointID, _fadeColor, "SceneDoor"));
        }

        private bool IsConditionMet()
        {
            if (_requiredCondition == null) return true;

            ConditionQueryEvent query = new ConditionQueryEvent(_requiredCondition, "SceneDoor");
            EventBus.Raise(query);
            return !query.Handled || query.Result;
        }

        private bool TryPayResourceCost()
        {
            ResourceConsumeRequestEvent request = new ResourceConsumeRequestEvent(_resourceCost, $"SceneDoor_{gameObject.name}", "SceneDoor");
            EventBus.Raise(request);

            if (!request.Handled)
            {
                DebugSystem.LogWarning($"SceneDoor '{gameObject.name}' requiere recurso pero no hay PlayerResourceSystem activo.",
                                       "SceneFlow", "SceneDoor");
                return false;
            }

            return request.Success;
        }
    }
}
