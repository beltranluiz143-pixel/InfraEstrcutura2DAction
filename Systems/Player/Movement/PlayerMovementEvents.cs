using UnityEngine;

namespace Infra2DAction
{
    public class PlayerMovedEvent : BaseEvent
    {
        public Vector2 Velocity { get; private set; }
        public bool IsRunning { get; private set; }

        public PlayerMovedEvent(Vector2 velocity, bool isRunning, string sourceID = "PlayerMovementSystem")
            : base(sourceID)
        {
            Velocity = velocity;
            IsRunning = isRunning;
        }
    }

    public class PlayerJumpedEvent : BaseEvent
    {
        public bool IsDoubleJump { get; private set; }

        public PlayerJumpedEvent(bool isDoubleJump, string sourceID = "PlayerMovementSystem")
            : base(sourceID)
        {
            IsDoubleJump = isDoubleJump;
        }
    }

    public class PlayerDashedEvent : BaseEvent
    {
        public Vector2 Direction { get; private set; }

        public PlayerDashedEvent(Vector2 direction, string sourceID = "PlayerMovementSystem")
            : base(sourceID)
        {
            Direction = direction;
        }
    }

    public class PlayerLandedEvent : BaseEvent
    {
        public float FallVelocity { get; private set; }

        public PlayerLandedEvent(float fallVelocity, string sourceID = "PlayerMovementSystem")
            : base(sourceID)
        {
            FallVelocity = fallVelocity;
        }
    }

    public class PlayerWallJumpEvent : BaseEvent
    {
        public PlayerWallJumpEvent(string sourceID = "PlayerMovementSystem") : base(sourceID) { }
    }

    public class PlayerFallingEvent : BaseEvent
    {
        public PlayerFallingEvent(string sourceID = "PlayerMovementSystem") : base(sourceID) { }
    }

    public class PlayerMovementLockedEvent : BaseEvent
    {
        public bool IsLocked { get; private set; }

        public PlayerMovementLockedEvent(bool isLocked, string sourceID = "PlayerMovementSystem")
            : base(sourceID)
        {
            IsLocked = isLocked;
        }
    }
}
