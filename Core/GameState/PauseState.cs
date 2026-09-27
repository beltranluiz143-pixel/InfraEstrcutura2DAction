namespace Infra2DAction
{
    public class PauseState : StateBase
    {
        public PauseState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            UnityEngine.Time.timeScale = 0f;
            DebugSystem.Log("Entered PauseState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            UnityEngine.Time.timeScale = 1f;
            DebugSystem.Log("Exited PauseState.", "GameState", "GameStateSystem");
        }
    }
}
