using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infra2DAction
{
    public class InputManager : MonoBehaviour
    {
        private const string GAMEPLAY_MAP = "Gameplay";
        private const string UI_MAP = "UI";
        private const string GLOBAL_MAP = "Global";

        [Header("Input Actions Asset")]
        [SerializeField] private InputActionAsset _inputActions;

        private InputActionMap _gameplayMap;
        private InputActionMap _uiMap;
        private InputActionMap _globalMap;

        private InputAction _move;
        private InputAction _jump;
        private InputAction _dash;
        private InputAction _crouchSlide;
        private InputAction _lookUp;
        private InputAction _lookDown;
        private InputAction _attack;
        private InputAction _shield;
        private InputAction _heal;
        private InputAction _pacify;
        private InputAction _interact;
        private InputAction _dialogueAdvance;
        private InputAction _uiCancel;
        private InputAction _pause;
        private InputAction _openInventory;
        private InputAction _openMap;

        private bool _gameplayInputEnabled = true;
        public bool IsGameplayInputEnabled => _gameplayInputEnabled;

        public void Initialize()
        {
            if (_inputActions == null)
            {
                DebugSystem.LogError("InputActions asset no asignado.", "Input", "InputManager");
                return;
            }

            _gameplayMap = _inputActions.FindActionMap(GAMEPLAY_MAP);
            _uiMap = _inputActions.FindActionMap(UI_MAP);
            _globalMap = _inputActions.FindActionMap(GLOBAL_MAP);

            if (_gameplayMap == null || _uiMap == null || _globalMap == null)
            {
                DebugSystem.LogError($"Faltan Action Maps en el asset. Se esperan: '{GAMEPLAY_MAP}', '{UI_MAP}', '{GLOBAL_MAP}'.",
                                     "Input", "InputManager");
                return;
            }

            CacheActions();
            BindActions();

            _gameplayMap.Enable();
            _globalMap.Enable();
            _uiMap.Disable();

            DebugSystem.Log("InputManager ready.", "Input", "InputManager");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            UnbindActions();
        }

        private InputAction Find(InputActionMap map, string actionName)
        {
            InputAction action = map.FindAction(actionName);
            if (action == null)
                DebugSystem.LogError($"Accion de input no encontrada en el asset: '{actionName}' (mapa '{map.name}').",
                                     "Input", "InputManager");
            return action;
        }

        private void CacheActions()
        {
            _move = Find(_gameplayMap, "Move");
            _jump = Find(_gameplayMap, "Jump");
            _dash = Find(_gameplayMap, "Dash");
            _crouchSlide = Find(_gameplayMap, "CrouchSlide");
            _lookUp = Find(_gameplayMap, "LookUp");
            _lookDown = Find(_gameplayMap, "LookDown");
            _attack = Find(_gameplayMap, "Attack");
            _shield = Find(_gameplayMap, "Shield");
            _heal = Find(_gameplayMap, "Heal");
            _pacify = Find(_gameplayMap, "Pacify");
            _interact = Find(_gameplayMap, "Interact");

            _dialogueAdvance = Find(_uiMap, "DialogueAdvance");
            _uiCancel = Find(_uiMap, "Cancel");

            _pause = Find(_globalMap, "Pause");
            _openInventory = Find(_globalMap, "OpenInventory");
            _openMap = Find(_globalMap, "OpenMap");
        }

        private void Bind(InputAction action, Action<InputAction.CallbackContext> performed,
                          Action<InputAction.CallbackContext> canceled = null)
        {
            if (action == null) return;
            action.performed += performed;
            if (canceled != null) action.canceled += canceled;
        }

        private void Unbind(InputAction action, Action<InputAction.CallbackContext> performed,
                            Action<InputAction.CallbackContext> canceled = null)
        {
            if (action == null) return;
            action.performed -= performed;
            if (canceled != null) action.canceled -= canceled;
        }

        private void BindActions()
        {
            Bind(_move, OnMove, OnMoveCanceled);
            Bind(_jump, OnJump, OnJumpCanceled);
            Bind(_dash, OnDash);
            Bind(_crouchSlide, OnCrouchSlide, OnCrouchSlideCanceled);
            Bind(_lookUp, OnLookUp, OnLookCanceled);
            Bind(_lookDown, OnLookDown, OnLookCanceled);
            Bind(_attack, OnAttack);
            Bind(_shield, OnShield, OnShieldCanceled);
            Bind(_heal, OnHeal);
            Bind(_pacify, OnPacify);
            Bind(_interact, OnInteract);
            Bind(_dialogueAdvance, OnDialogueAdvance);
            Bind(_uiCancel, OnUICancel);
            Bind(_pause, OnPause);
            Bind(_openInventory, OnOpenInventory);
            Bind(_openMap, OnOpenMap);
        }

        private void UnbindActions()
        {
            Unbind(_move, OnMove, OnMoveCanceled);
            Unbind(_jump, OnJump, OnJumpCanceled);
            Unbind(_dash, OnDash);
            Unbind(_crouchSlide, OnCrouchSlide, OnCrouchSlideCanceled);
            Unbind(_lookUp, OnLookUp, OnLookCanceled);
            Unbind(_lookDown, OnLookDown, OnLookCanceled);
            Unbind(_attack, OnAttack);
            Unbind(_shield, OnShield, OnShieldCanceled);
            Unbind(_heal, OnHeal);
            Unbind(_pacify, OnPacify);
            Unbind(_interact, OnInteract);
            Unbind(_dialogueAdvance, OnDialogueAdvance);
            Unbind(_uiCancel, OnUICancel);
            Unbind(_pause, OnPause);
            Unbind(_openInventory, OnOpenInventory);
            Unbind(_openMap, OnOpenMap);
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (_gameplayMap == null || _uiMap == null) return;

            switch (e.NewState)
            {
                case GameStateType.Gameplay:
                case GameStateType.Dream:
                    _gameplayMap.Enable();
                    _uiMap.Disable();
                    _gameplayInputEnabled = true;
                    break;

                case GameStateType.Pause:
                case GameStateType.Dialogue:
                case GameStateType.Minigame:
                case GameStateType.MainMenu:
                case GameStateType.Menu:
                    _gameplayMap.Disable();
                    _uiMap.Enable();
                    _gameplayInputEnabled = false;
                    break;

                case GameStateType.Cutscene:
                case GameStateType.Death:
                    _gameplayMap.Disable();
                    _uiMap.Disable();
                    _gameplayInputEnabled = false;
                    break;
            }
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new MoveInputEvent(ctx.ReadValue<Vector2>()));
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new MoveInputEvent(Vector2.zero));
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new JumpPressedEvent());
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new JumpReleasedEvent());
        }

        private void OnDash(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new DashPressedEvent());
        }

        private void OnCrouchSlide(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new CrouchSlideEvent(true));
        }

        private void OnCrouchSlideCanceled(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new CrouchSlideEvent(false));
        }

        private void OnLookUp(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new LookInputEvent(LookDirection.Up));
        }

        private void OnLookDown(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new LookInputEvent(LookDirection.Down));
        }

        private void OnLookCanceled(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new LookInputEvent(LookDirection.None));
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            Vector2 moveDir = _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;

            AttackDirection dir = AttackDirection.Normal;
            if (moveDir.y > 0.5f) dir = AttackDirection.Up;
            else if (moveDir.y < -0.5f) dir = AttackDirection.Down;

            EventBus.Raise(new AttackPressedEvent(dir, false));
        }

        private void OnShield(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new ShieldPressedEvent(true));
        }

        private void OnShieldCanceled(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new ShieldPressedEvent(false));
        }

        private void OnHeal(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new HealPressedEvent());
        }

        private void OnPacify(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new PacifyPressedEvent());
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new InteractPressedEvent());
        }

        private void OnDialogueAdvance(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new DialogueAdvanceEvent());
        }

        private void OnUICancel(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new UIClosePanelRequestEvent("InputManager"));
        }

        private void OnPause(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new PausePressedEvent());
        }

        private void OnOpenInventory(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new OpenInventoryEvent());
        }

        private void OnOpenMap(InputAction.CallbackContext ctx)
        {
            EventBus.Raise(new OpenMapEvent());
        }
    }
}
