namespace Infra2DAction
{
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }
    }

    public class MovingState : PlayerStateBase
    {
        public MovingState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }
    }

    public class JumpingState : PlayerStateBase
    {
        public JumpingState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }
    }

    public class FallingState : PlayerStateBase
    {
        public FallingState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }
    }

    public class DashingState : PlayerStateBase
    {
        public DashingState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }

        public override bool CanBeInterrupted() => false;
    }

    public class WallSlidingState : PlayerStateBase
    {
        public WallSlidingState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }
    }

    public class AttackingState : PlayerStateBase
    {
        private bool _animationFinished;

        public AttackingState(PlayerStateSystem owner) : base(owner) { }

        public override void Enter() => _animationFinished = false;
        public override void Update() { }
        public override void Exit() { }

        public void MarkAnimationFinished() => _animationFinished = true;

        public override bool CanBeInterrupted() => _animationFinished;
    }

    public class StunnedState : PlayerStateBase
    {
        private bool _recoveryFinished;

        public StunnedState(PlayerStateSystem owner) : base(owner) { }

        public override void Enter() => _recoveryFinished = false;
        public override void Update() { }
        public override void Exit() { }

        public void MarkRecoveryFinished() => _recoveryFinished = true;

        public override bool CanBeInterrupted() => _recoveryFinished;
    }

    public class SwimmingState : PlayerStateBase
    {
        public SwimmingState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }
    }

    public class DeadState : PlayerStateBase
    {
        public DeadState(PlayerStateSystem owner) : base(owner) { }
        public override void Enter() { }
        public override void Update() { }
        public override void Exit() { }

        public override bool CanBeInterrupted() => false;
    }
}
