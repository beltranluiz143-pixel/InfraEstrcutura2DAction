using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class PlayerStateSystem : MonoBehaviour
    {
        private PlayerStateType _currentStateType;
        private PlayerStateBase _currentState;
        private Dictionary<PlayerStateType, PlayerStateBase> _states;

        public PlayerStateType CurrentState => _currentStateType;

        private void Awake()
        {
            _states = new Dictionary<PlayerStateType, PlayerStateBase>
            {
                { PlayerStateType.Idle,        new IdleState(this) },
                { PlayerStateType.Moving,      new MovingState(this) },
                { PlayerStateType.Jumping,     new JumpingState(this) },
                { PlayerStateType.Falling,     new FallingState(this) },
                { PlayerStateType.Dashing,     new DashingState(this) },
                { PlayerStateType.WallSliding, new WallSlidingState(this) },
                { PlayerStateType.Attacking,   new AttackingState(this) },
                { PlayerStateType.Stunned,     new StunnedState(this) },
                { PlayerStateType.Swimming,    new SwimmingState(this) },
                { PlayerStateType.Dead,        new DeadState(this) }
            };

            _currentStateType = PlayerStateType.Idle;
            _currentState = _states[PlayerStateType.Idle];
        }

        private void Update()
        {
            _currentState?.Update();
        }

        public void TransitionTo(PlayerStateType newStateType, bool force = false)
        {
            if (_currentStateType == newStateType) return;

            if (!force && _currentState != null && !_currentState.CanBeInterrupted())
            {
                DebugSystem.Log($"Transition blocked: {_currentStateType} cannot be interrupted yet.",
                                "Player", "PlayerStateSystem");
                return;
            }

            PlayerStateType previous = _currentStateType;

            _currentState?.Exit();
            _currentStateType = newStateType;
            _currentState = _states[newStateType];
            _currentState.Enter();

            EventBus.Raise(new PlayerStateChangedEvent(previous, newStateType));
        }

        public bool CanAct() => _currentState?.CanBeInterrupted() ?? true;
    }
}
