using UnityEngine;

namespace Infra2DAction
{
    public class SurfacePhysicsSystem : MonoBehaviour
    {
        [Header("Default Surface")]
        [SerializeField] private SurfaceData _groundSurface;

        private SurfaceData _currentSurface;
        private PlayerStateSystem _stateSystem;

        public SurfaceData CurrentSurface => _currentSurface;

        private void Awake()
        {
            _stateSystem = GetComponent<PlayerStateSystem>();
            _currentSurface = _groundSurface;
        }

        public void EnterSurface(SurfaceData surface)
        {
            if (surface == null || surface == _currentSurface) return;

            string previousID = _currentSurface != null ? _currentSurface.SurfaceID : "None";
            _currentSurface = surface;

            EventBus.Raise(new SurfaceChangedEvent(previousID, surface.SurfaceID));

            if (surface.Behavior == SurfaceData.SurfaceBehavior.Water)
            {
                EventBus.Raise(new PlayerEnteredWaterEvent());
                _stateSystem?.TransitionTo(PlayerStateType.Swimming);
            }

            DebugSystem.Log($"Surface entered: {surface.SurfaceID}", "Player", "SurfacePhysicsSystem");
        }

        public void ExitSurface(SurfaceData surface)
        {
            if (surface != _currentSurface) return;

            if (surface.Behavior == SurfaceData.SurfaceBehavior.Water)
            {
                EventBus.Raise(new PlayerExitedWaterEvent());

                if (_stateSystem != null && _stateSystem.CurrentState == PlayerStateType.Swimming)
                    _stateSystem.TransitionTo(PlayerStateType.Falling);
            }

            string previousID = _currentSurface.SurfaceID;
            _currentSurface = _groundSurface;

            EventBus.Raise(new SurfaceChangedEvent(previousID, _groundSurface != null ? _groundSurface.SurfaceID : "None"));
        }

        public float GetFrictionMultiplier() => _currentSurface != null ? _currentSurface.FrictionMultiplier : 1f;
        public float GetGravityMultiplier() => _currentSurface != null ? _currentSurface.GravityMultiplier : 1f;
        public float GetMaxSpeedMultiplier() => _currentSurface != null ? _currentSurface.MaxSpeedMultiplier : 1f;

        public Vector2 GetDirectionalForce()
        {
            if (_currentSurface != null && _currentSurface.Behavior == SurfaceData.SurfaceBehavior.Directional)
                return _currentSurface.DirectionalForce;
            return Vector2.zero;
        }

        public bool IsInWater() => _currentSurface != null && _currentSurface.Behavior == SurfaceData.SurfaceBehavior.Water;
    }
}
