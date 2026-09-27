using UnityEngine;

namespace Infra2DAction
{
    public class PlayerLockCoordinator : MonoBehaviour
    {
        private int _lockCount;

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerMovementLockedEvent>(OnLockRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerMovementLockedEvent>(OnLockRequested);
        }

        private void OnLockRequested(PlayerMovementLockedEvent e)
        {
            if (e.IsLocked) _lockCount++;
            else _lockCount = Mathf.Max(0, _lockCount - 1);
        }

        public bool IsMovementLocked() => _lockCount > 0;
    }
}
