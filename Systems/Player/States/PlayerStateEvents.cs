namespace Infra2DAction
{
    public enum PlayerStateType
    {
        Idle, Moving, Jumping, Falling, Dashing,
        WallSliding, Attacking, Stunned, Swimming, Dead
    }

    public class PlayerStateChangedEvent : BaseEvent
    {
        public PlayerStateType PreviousState { get; private set; }
        public PlayerStateType NewState { get; private set; }

        public PlayerStateChangedEvent(PlayerStateType previous, PlayerStateType newState, string sourceID = "PlayerStateSystem")
            : base(sourceID)
        {
            PreviousState = previous;
            NewState = newState;
        }
    }
}
