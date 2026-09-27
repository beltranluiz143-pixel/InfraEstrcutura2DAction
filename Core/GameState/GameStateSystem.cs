using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class GameStateSystem : MonoBehaviour
    {
        private StateBase _currentState;
        private GameStateType _currentStateType;
        private GameStateType _stateBeforeDialogue = GameStateType.Gameplay;

        private Dictionary<GameStateType, StateBase> _states;

        public void Initialize()
        {
            _states = new Dictionary<GameStateType, StateBase>
            {
                { GameStateType.Gameplay,  new GameplayState(this)  },
                { GameStateType.Pause,     new PauseState(this)     },
                { GameStateType.Dialogue,  new DialogueState(this)  },
                { GameStateType.Cutscene,  new CutsceneState(this)  },
                { GameStateType.Death,     new DeathState(this)     },
                { GameStateType.Dream,     new DreamState(this)     },
                { GameStateType.Minigame,  new MinigameState(this)  },
                { GameStateType.MainMenu,  new MainMenuState(this)  },
                { GameStateType.Menu,      new MenuState(this)      },
            };

            TransitionTo(GameStateType.Gameplay);

            DebugSystem.Log("GameStateSystem initialized.", "GameState", "GameStateSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PausePressedEvent>(OnPausePressed);
            EventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Subscribe<GameStateChangeRequestEvent>(OnStateChangeRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PausePressedEvent>(OnPausePressed);
            EventBus.Unsubscribe<PlayerDeathEvent>(OnPlayerDeath);
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Unsubscribe<GameStateChangeRequestEvent>(OnStateChangeRequested);
        }

        private void OnStateChangeRequested(GameStateChangeRequestEvent e)
        {
            TransitionTo(e.NewState);
        }

        private void Update()
        {
            _currentState?.Update();
        }

        public void TransitionTo(GameStateType newStateType, bool force = false)
        {
            if (_currentState != null && _currentStateType == newStateType) return;

            if (!force && _currentStateType == GameStateType.Dialogue &&
               newStateType != GameStateType.Gameplay)
            {
                DebugSystem.LogWarning($"Transicion a {newStateType} bloqueada: dialogo activo no permite interrupciones.",
                                       "GameState", "GameStateSystem");
                return;
            }

            if (!_states.ContainsKey(newStateType))
            {
                DebugSystem.LogError($"Estado {newStateType} no existe.", "GameState", "GameStateSystem");
                return;
            }

            GameStateType previousType = _currentStateType;

            _currentState?.Exit();
            _currentStateType = newStateType;
            _currentState = _states[newStateType];
            _currentState.Enter();

            EventBus.Raise(new GameStateChangedEvent(previousType, newStateType));
        }

        public GameStateType GetCurrentState() => _currentStateType;

        private void OnPausePressed(PausePressedEvent e)
        {
            if (_currentStateType == GameStateType.Gameplay ||
                _currentStateType == GameStateType.Dream)
            {
                TransitionTo(GameStateType.Pause);
            }
            else if (_currentStateType == GameStateType.Pause)
            {
                TransitionTo(GameStateType.Gameplay);
            }
        }

        private void OnPlayerDeath(PlayerDeathEvent e)
        {
            TransitionTo(GameStateType.Death);
        }

        private void OnDialogueStarted(DialogueStartedEvent e)
        {
            if (_currentStateType != GameStateType.Dialogue)
                _stateBeforeDialogue = _currentStateType;

            TransitionTo(GameStateType.Dialogue);
        }

        private void OnDialogueEnded(DialogueEndedEvent e)
        {
            GameStateType target = _stateBeforeDialogue == GameStateType.Dialogue
                ? GameStateType.Gameplay
                : _stateBeforeDialogue;

            _stateBeforeDialogue = GameStateType.Gameplay;
            TransitionTo(target, true);
        }

        private void OnSceneLoaded(SceneLoadedEvent e)
        {
            if (_currentStateType == GameStateType.Death)
                TransitionTo(GameStateType.Gameplay);
        }
    }
}
