namespace Infra2DAction
{
    public class DeathState : StateBase
    {
        public DeathState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            DebugSystem.Log("Entered DeathState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            DebugSystem.Log("Exited DeathState.", "GameState", "GameStateSystem");
        }
    }
}
