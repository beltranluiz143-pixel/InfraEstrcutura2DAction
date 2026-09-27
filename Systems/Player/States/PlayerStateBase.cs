namespace Infra2DAction
{
    public abstract class PlayerStateBase
    {
        protected PlayerStateSystem Owner { get; private set; }

        public PlayerStateBase(PlayerStateSystem owner)
        {
            Owner = owner;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();

        public virtual bool CanBeInterrupted() => true;
    }
}
