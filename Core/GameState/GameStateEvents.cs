namespace Infra2DAction
{
    public enum GameStateType
    {
        Gameplay,
        Pause,
        Dialogue,
        Cutscene,
        Death,
        Dream,
        Minigame,
        MainMenu,
        Menu,
    }

    public class GameStateChangeRequestEvent : BaseEvent
    {
        public GameStateType NewState { get; private set; }

        public GameStateChangeRequestEvent(GameStateType newState, string sourceID = "Unknown")
            : base(sourceID)
        {
            NewState = newState;
        }
    }

    public class GameStateChangedEvent : BaseEvent
    {
        public GameStateType PreviousState { get; private set; }
        public GameStateType NewState { get; private set; }

        public GameStateChangedEvent(GameStateType previous, GameStateType newState, string sourceID = "GameStateSystem")
            : base(sourceID)
        {
            PreviousState = previous;
            NewState = newState;
        }
    }
}
