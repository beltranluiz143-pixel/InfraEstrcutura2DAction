using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

namespace Infra2DAction
{
    public class CameraSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera _virtualCamera;
        [SerializeField] private CinemachineConfiner2D _confiner;

        [Header("Default")]
        [SerializeField] private CameraData _defaultCameraData;

        private CameraFollowTarget _followTarget;

        private class ActiveZone
        {
            public Collider2D Collider;
            public CameraData Data;
            public int Priority;
            public int RefCount;
        }

        private readonly List<ActiveZone> _activeZones = new List<ActiveZone>();

        private float _targetOrthographicSize;
        private float _orthoSizeVelocity;
        private const float ORTHO_SIZE_TRANSITION_TIME = 0.35f;

        private void OnEnable()
        {
            EventBus.Subscribe<CameraZoneEnteredEvent>(OnZoneEntered);
            EventBus.Subscribe<CameraZoneExitedEvent>(OnZoneExited);
            EventBus.Subscribe<CameraFollowTargetRegisteredEvent>(OnFollowTargetRegistered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CameraZoneEnteredEvent>(OnZoneEntered);
            EventBus.Unsubscribe<CameraZoneExitedEvent>(OnZoneExited);
            EventBus.Unsubscribe<CameraFollowTargetRegisteredEvent>(OnFollowTargetRegistered);
        }

        public void Initialize()
        {
            ApplyCameraData(_defaultCameraData);

            if (_virtualCamera != null && _defaultCameraData != null)
                _virtualCamera.Lens.OrthographicSize = _defaultCameraData.OrthographicSize;

            DebugSystem.Log("CameraSystem initialized.", "Camera", "CameraSystem");
        }

        private void Update()
        {
            if (_virtualCamera != null)
            {
                float current = _virtualCamera.Lens.OrthographicSize;
                _virtualCamera.Lens.OrthographicSize = Mathf.SmoothDamp(
                    current, _targetOrthographicSize, ref _orthoSizeVelocity, ORTHO_SIZE_TRANSITION_TIME);
            }
        }

        private void OnFollowTargetRegistered(CameraFollowTargetRegisteredEvent e)
        {
            _followTarget = e.Target;

            DebugSystem.Log($"CameraFollowTarget registrado: '{_followTarget.gameObject.name}' " +
                            $"en posicion {_followTarget.transform.position}.",
                            "Camera", "CameraSystem");

            if (_virtualCamera == null)
            {
                DebugSystem.LogError(
                    "_virtualCamera no esta asignado en CameraSystem - no se puede fijar el Follow.",
                    "Camera", "CameraSystem");
            }
            else
            {
                _virtualCamera.Follow = _followTarget.transform;

                Vector3 warpDelta = _followTarget.transform.position - _virtualCamera.transform.position;
                _virtualCamera.OnTargetObjectWarped(_followTarget.transform, warpDelta);
            }

            ApplyCameraData(_defaultCameraData);
        }

        private void OnZoneEntered(CameraZoneEnteredEvent e)
        {
            ActiveZone existing = _activeZones.Find(z => z.Collider == e.BoundsCollider);
            if (existing != null)
            {
                existing.RefCount++;
                return;
            }

            _activeZones.Add(new ActiveZone
            {
                Collider = e.BoundsCollider,
                Data = e.ZoneCameraData,
                Priority = e.Priority,
                RefCount = 1
            });

            EventBus.Raise(new CameraTransitionStartedEvent());
            ApplyHighestPriorityZone();
        }

        private void OnZoneExited(CameraZoneExitedEvent e)
        {
            ActiveZone existing = _activeZones.Find(z => z.Collider == e.BoundsCollider);
            if (existing == null) return;

            existing.RefCount--;
            if (existing.RefCount > 0) return;

            _activeZones.Remove(existing);

            EventBus.Raise(new CameraTransitionStartedEvent());
            ApplyHighestPriorityZone();
        }

        private void ApplyHighestPriorityZone()
        {
            if (_activeZones.Count == 0)
            {
                if (_confiner != null) _confiner.BoundingShape2D = null;
                ApplyCameraData(_defaultCameraData);
                return;
            }

            ActiveZone winner = _activeZones[0];
            foreach (ActiveZone zone in _activeZones)
            {
                if (zone.Priority > winner.Priority)
                    winner = zone;
            }

            if (_confiner != null && winner.Collider != null)
            {
                _confiner.BoundingShape2D = winner.Collider;
                _confiner.InvalidateBoundingShapeCache();
            }

            ApplyCameraData(winner.Data != null ? winner.Data : _defaultCameraData);
        }

        private void ApplyCameraData(CameraData data)
        {
            if (data == null) return;

            _targetOrthographicSize = data.OrthographicSize;

            _followTarget?.SetCameraData(data);
        }
    }
}
