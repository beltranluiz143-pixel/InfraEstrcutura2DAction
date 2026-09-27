using UnityEngine;

namespace Infra2DAction
{
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float _detectionRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayerMask;

        [Header("Prompt Visual")]
        [SerializeField] private GameObject _promptRoot;
        [SerializeField] private Vector3 _promptWorldOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Performance")]
        [SerializeField] private int _detectionFrameInterval = 3;

        private readonly Collider2D[] _detectionBuffer = new Collider2D[8];
        private ContactFilter2D _contactFilter;

        private PlayerStateSystem _stateSystem;
        private IInteractable _currentInteractable;
        private GameObject _currentTargetObject;
        private bool _promptVisible;
        private int _frameCounter;

        private void Awake()
        {
            _stateSystem = GetComponent<PlayerStateSystem>();

            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(_interactableLayerMask);
            _contactFilter.useTriggers = true;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InteractPressedEvent>(OnInteractPressed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractPressedEvent>(OnInteractPressed);
        }

        private void Update()
        {
            _frameCounter++;
            if (_frameCounter >= _detectionFrameInterval)
            {
                _frameCounter = 0;
                DetectNearestInteractable();
            }

            UpdatePromptPosition();
        }

        private void DetectNearestInteractable()
        {
            int hitCount = Physics2D.OverlapCircle(transform.position, _detectionRadius, _contactFilter, _detectionBuffer);

            IInteractable nearest = null;
            GameObject nearestObject = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = _detectionBuffer[i];

                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable == null) continue;

                float distance = Vector2.Distance(transform.position, hit.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearest = interactable;
                    nearestObject = hit.gameObject;
                }
            }

            if (nearest != _currentInteractable)
            {
                _currentInteractable = nearest;
                _currentTargetObject = nearestObject;
                UpdatePromptVisibility();
            }
        }

        private void UpdatePromptVisibility()
        {
            bool shouldShow = _currentInteractable != null;

            if (shouldShow && !_promptVisible)
            {
                EventBus.Raise(new InteractionPromptShownEvent());
                if (_promptRoot != null) _promptRoot.SetActive(true);
                _promptVisible = true;
            }
            else if (!shouldShow && _promptVisible)
            {
                EventBus.Raise(new InteractionPromptHiddenEvent());
                if (_promptRoot != null) _promptRoot.SetActive(false);
                _promptVisible = false;
            }
        }

        private void OnInteractPressed(InteractPressedEvent e)
        {
            if (_currentInteractable == null) return;
            if (!CanInteractInCurrentState()) return;

            EventBus.Raise(new InteractionTriggeredEvent(_currentTargetObject));
            _currentInteractable.OnInteract();
        }

        private bool CanInteractInCurrentState()
        {
            if (_stateSystem == null) return true;

            PlayerStateType current = _stateSystem.CurrentState;
            return current != PlayerStateType.Dashing &&
                   current != PlayerStateType.Attacking;
        }

        private void UpdatePromptPosition()
        {
            if (_promptRoot == null || !_promptRoot.activeSelf) return;
            _promptRoot.transform.position = transform.position + _promptWorldOffset;
        }
    }
}
