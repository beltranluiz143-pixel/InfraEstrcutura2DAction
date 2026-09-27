namespace Infra2DAction
{
    public class CutsceneState : StateBase
    {
        public CutsceneState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            DebugSystem.Log("Entered CutsceneState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            DebugSystem.Log("Exited CutsceneState.", "GameState", "GameStateSystem");
        }
    }
}
