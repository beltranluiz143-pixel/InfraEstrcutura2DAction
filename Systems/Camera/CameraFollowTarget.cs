using UnityEngine;

namespace Infra2DAction
{
    public class CameraFollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform _player;

        private CameraData _activeData;
        private Vector3 _currentVelocitySmoothing;
        private float _currentLookAheadX;

        public void SetCameraData(CameraData data) => _activeData = data;

        private void OnEnable()
        {
            EventBus.Subscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Raise(new CameraFollowTargetRegisteredEvent(this));
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CoreInitializedEvent>(OnCoreInitialized);
        }

        private void OnCoreInitialized(CoreInitializedEvent e)
        {
            EventBus.Raise(new CameraFollowTargetRegisteredEvent(this));
        }

        private void LateUpdate()
        {
            if (_player == null || _activeData == null) return;

            Vector3 targetPosition = _player.position;

            Rigidbody2D playerRb = _player.GetComponent<Rigidbody2D>();
            float desiredLookAheadX = 0f;

            if (playerRb != null && playerRb.linearVelocity.sqrMagnitude > 0.1f)
                desiredLookAheadX = Mathf.Sign(playerRb.linearVelocity.x) * _activeData.LookAheadDistance;

            _currentLookAheadX = Mathf.Lerp(_currentLookAheadX, desiredLookAheadX, _activeData.LookAheadSmoothing);
            targetPosition += new Vector3(_currentLookAheadX, 0f, 0f);

            float dampTime = _activeData.AllowAbruptMovement
                ? _activeData.AbruptDamping
                : Mathf.Max(_activeData.HorizontalDamping, _activeData.VerticalDamping);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
                                                     ref _currentVelocitySmoothing, dampTime);
        }
    }
}
