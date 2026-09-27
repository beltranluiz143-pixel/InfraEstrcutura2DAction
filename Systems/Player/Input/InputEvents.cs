using UnityEngine;

namespace Infra2DAction
{
    public enum AttackDirection { Normal, Up, Down, Air }

    public enum LookDirection { None, Up, Down }

    public class MoveInputEvent : BaseEvent
    {
        public Vector2 Direction { get; private set; }

        public MoveInputEvent(Vector2 direction, string sourceID = "InputManager")
            : base(sourceID)
        {
            Direction = direction;
        }
    }

    public class JumpPressedEvent : BaseEvent
    {
        public JumpPressedEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class JumpReleasedEvent : BaseEvent
    {
        public JumpReleasedEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class DashPressedEvent : BaseEvent
    {
        public DashPressedEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class CrouchSlideEvent : BaseEvent
    {
        public bool IsActive { get; private set; }

        public CrouchSlideEvent(bool isActive, string sourceID = "InputManager")
            : base(sourceID)
        {
            IsActive = isActive;
        }
    }

    public class LookInputEvent : BaseEvent
    {
        public LookDirection Direction { get; private set; }

        public LookInputEvent(LookDirection direction, string sourceID = "InputManager")
            : base(sourceID)
        {
            Direction = direction;
        }
    }

    public class AttackPressedEvent : BaseEvent
    {
        public AttackDirection Direction { get; private set; }
        public bool IsAirborne { get; private set; }

        public AttackPressedEvent(AttackDirection direction, bool isAirborne, string sourceID = "InputManager")
            : base(sourceID)
        {
            Direction = direction;
            IsAirborne = isAirborne;
        }
    }

    public class ShieldPressedEvent : BaseEvent
    {
        public bool IsActive { get; private set; }

        public ShieldPressedEvent(bool isActive, string sourceID = "InputManager")
            : base(sourceID)
        {
            IsActive = isActive;
        }
    }

    public class HealPressedEvent : BaseEvent
    {
        public HealPressedEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class PacifyPressedEvent : BaseEvent
    {
        public PacifyPressedEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class InteractPressedEvent : BaseEvent
    {
        public InteractPressedEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class PausePressedEvent : BaseEvent
    {
        public PausePressedEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class DialogueAdvanceEvent : BaseEvent
    {
        public DialogueAdvanceEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class OpenInventoryEvent : BaseEvent
    {
        public OpenInventoryEvent(string sourceID = "InputManager") : base(sourceID) { }
    }

    public class OpenMapEvent : BaseEvent
    {
        public OpenMapEvent(string sourceID = "InputManager") : base(sourceID) { }
    }
}
