namespace Infra2DAction
{
    public class GameplayState : StateBase
    {
        public GameplayState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            DebugSystem.Log("Entered GameplayState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            DebugSystem.Log("Exited GameplayState.", "GameState", "GameStateSystem");
        }
    }
}
