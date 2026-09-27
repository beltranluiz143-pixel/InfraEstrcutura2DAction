using UnityEngine;

namespace Infra2DAction
{
    public class CameraZoneEnteredEvent : BaseEvent
    {
        public Collider2D BoundsCollider { get; private set; }
        public CameraData ZoneCameraData { get; private set; }
        public int Priority { get; private set; }

        public CameraZoneEnteredEvent(Collider2D bounds, CameraData cameraData, int priority, string sourceID = "CameraTrigger")
            : base(sourceID)
        {
            BoundsCollider = bounds;
            ZoneCameraData = cameraData;
            Priority = priority;
        }
    }

    public class CameraZoneExitedEvent : BaseEvent
    {
        public Collider2D BoundsCollider { get; private set; }

        public CameraZoneExitedEvent(Collider2D bounds, string sourceID = "CameraTrigger")
            : base(sourceID)
        {
            BoundsCollider = bounds;
        }
    }

    public class CameraTransitionStartedEvent : BaseEvent
    {
        public CameraTransitionStartedEvent(string sourceID = "CameraSystem") : base(sourceID) { }
    }

    public class CameraFollowTargetRegisteredEvent : BaseEvent
    {
        public CameraFollowTarget Target { get; private set; }

        public CameraFollowTargetRegisteredEvent(CameraFollowTarget target, string sourceID = "CameraFollowTarget")
            : base(sourceID)
        {
            Target = target;
        }
    }
}
