namespace Infra2DAction
{
    public abstract class StateBase
    {
        protected GameStateSystem Owner { get; private set; }

        public StateBase(GameStateSystem owner)
        {
            Owner = owner;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}
