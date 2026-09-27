namespace Infra2DAction
{
    public class MainMenuState : StateBase
    {
        public MainMenuState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            DebugSystem.Log("Entered MainMenuState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            DebugSystem.Log("Exited MainMenuState.", "GameState", "GameStateSystem");
        }
    }
}
