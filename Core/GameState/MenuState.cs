namespace Infra2DAction
{
    public class MenuState : StateBase
    {
        public MenuState(GameStateSystem owner) : base(owner) { }

        public override void Enter()
        {
            UnityEngine.Time.timeScale = 0f;
            DebugSystem.Log("Entered MenuState.", "GameState", "GameStateSystem");
        }

        public override void Update() { }

        public override void Exit()
        {
            UnityEngine.Time.timeScale = 1f;
            DebugSystem.Log("Exited MenuState.", "GameState", "GameStateSystem");
        }
    }
}
