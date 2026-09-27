namespace Infra2DAction
{
    public class DreamState : StateBase
    {
        public DreamState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            DebugSystem.Log("Entered DreamState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            DebugSystem.Log("Exited DreamState.", "GameState", "GameStateSystem");
        }
    }
}
