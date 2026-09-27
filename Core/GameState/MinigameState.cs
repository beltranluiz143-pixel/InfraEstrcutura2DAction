namespace Infra2DAction
{
    public class MinigameState : StateBase
    {
        public MinigameState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            EventBus.Raise(new PlayerMovementLockedEvent(true));
            DebugSystem.Log("Entered MinigameState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            EventBus.Raise(new PlayerMovementLockedEvent(false));
            DebugSystem.Log("Exited MinigameState.", "GameState", "GameStateSystem");
        }
    }
}
