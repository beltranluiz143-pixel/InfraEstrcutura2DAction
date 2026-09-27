using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class BootstrapSystem : MonoBehaviour
    {
        [Header("Core (obligatorios)")]
        [SerializeField] private DebugSystem _debugSystem;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private GameStateSystem _gameStateSystem;
        [SerializeField] private SceneFlowSystem _sceneFlowSystem;
        [SerializeField] private SettingsSystem _settingsSystem;
        [SerializeField] private SaveSystem _saveSystem;
        [SerializeField] private ResourceManager _resourceManager;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private GlobalVariablesSystem _globalVariablesSystem;
        [SerializeField] private AudioCore _audioCore;
        [SerializeField] private CameraSystem _cameraSystem;

        [Header("UI")]
        [SerializeField] private UICore _uiCore;
        [SerializeField] private UIStateController _uiStateController;
        [SerializeField] private HUDSystem _hudSystem;
        [SerializeField] private SaveSlotPanel _saveSlotPanel;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private QuestLogPanel _questLogPanel;

        [Header("Sistemas de juego (todos opcionales: vacio = no se usa)")]
        [SerializeField] private DialogueSystem _dialogueSystem;
        [SerializeField] private RouteSystem _routeSystem;
        [SerializeField] private EventReactionSystem _eventReactionSystem;
        [SerializeField] private NPCSystem _npcSystem;
        [SerializeField] private QuestSystem _questSystem;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private CutsceneSystem _cutsceneSystem;
        [SerializeField] private MapSystem _mapSystem;

        [Header("Bases de datos de contenido")]
        [SerializeField] private ItemDatabase _itemDatabase;

        [Header("Opciones")]
        [Tooltip("0 = no modificar Application.targetFrameRate.")]
        [SerializeField] private int _targetFrameRate = 60;

        private readonly HashSet<PlayerSystem> _initializedPlayers = new HashSet<PlayerSystem>();
        private PlayerSystem _pendingPlayer;
        private bool _coreReady;

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
        }

        private void Start()
        {
            if (_targetFrameRate > 0)
                Application.targetFrameRate = _targetFrameRate;

            InitializeCore();
        }

        private void InitializeCore()
        {
            RunStep("Debug", InitializeDebug);
            RunStep("Input", () => { if (Require(_inputManager, "InputManager")) _inputManager.Initialize(); });
            RunStep("GameState", () => { if (Require(_gameStateSystem, "GameStateSystem")) _gameStateSystem.Initialize(); });
            RunStep("SceneFlow", () => { if (Require(_sceneFlowSystem, "SceneFlowSystem")) _sceneFlowSystem.Initialize(_saveSystem); });
            RunStep("Settings", () => { if (Require(_settingsSystem, "SettingsSystem")) _settingsSystem.Initialize(); });
            RunStep("Save", () => { if (Require(_saveSystem, "SaveSystem")) _saveSystem.Initialize(_settingsSystem); });
            RunStep("Resources", () => { if (Require(_resourceManager, "ResourceManager")) _resourceManager.Initialize(); });
            RunStep("Pool", () => { if (Require(_poolManager, "PoolManager")) _poolManager.Initialize(_resourceManager); });
            RunStep("GlobalVariables", () => { if (Require(_globalVariablesSystem, "GlobalVariablesSystem")) _globalVariablesSystem.Initialize(_saveSystem); });
            RunStep("Audio", () => { if (Require(_audioCore, "AudioCore")) _audioCore.Initialize(_poolManager); });
            RunStep("Camera", () => { if (Require(_cameraSystem, "CameraSystem")) _cameraSystem.Initialize(); });
            RunStep("UI", InitializeUI);

            RunStep("Dialogue", () => { if (_dialogueSystem != null) _dialogueSystem.Initialize(_globalVariablesSystem); });
            RunStep("Route", () => { if (_routeSystem != null) _routeSystem.Initialize(_globalVariablesSystem); });
            RunStep("EventReactions", () => { if (_eventReactionSystem != null) _eventReactionSystem.Initialize(_globalVariablesSystem); });
            RunStep("NPC", () => { if (_npcSystem != null) _npcSystem.Initialize(_globalVariablesSystem); });
            RunStep("Quest", () => { if (_questSystem != null) _questSystem.Initialize(_globalVariablesSystem, _saveSystem, _routeSystem); });
            RunStep("Shop", () => { if (_shopManager != null) _shopManager.Initialize(_routeSystem); });
            RunStep("Cutscene", InitializeCutscene);
            RunStep("Map", () => { if (_mapSystem != null) _mapSystem.Initialize(_saveSystem); });
            RunStep("MenuPanels", InitializeMenuPanels);

            AnnounceReady();
        }

        private void RunStep(string stepName, Action step)
        {
            try
            {
                step();
            }
            catch (Exception ex)
            {
                DebugSystem.LogError(
                    $"Paso de inicializacion '{stepName}' fallo y se salto: {ex.Message}\n{ex.StackTrace}",
                    "Bootstrap", "BootstrapSystem");
            }
        }

        private bool Require(UnityEngine.Object system, string systemName)
        {
            if (system != null) return true;

            DebugSystem.LogError($"{systemName} no asignado en BootstrapSystem.", "Bootstrap", "BootstrapSystem");
            return false;
        }

        private void InitializeDebug()
        {
            if (!Require(_debugSystem, "DebugSystem")) return;

            EventDebugger debugger = _debugSystem.GetEventDebugger();
            if (debugger != null) EventBus.RegisterDebugger(debugger);
        }

        private void InitializeUI()
        {
            if (Require(_uiCore, "UICore")) _uiCore.Initialize();
            if (Require(_uiStateController, "UIStateController")) _uiStateController.Initialize();
            if (Require(_hudSystem, "HUDSystem")) _hudSystem.Initialize();
        }

        private void InitializeCutscene()
        {
            if (_cutsceneSystem == null) return;

            TransitionController transitionController = _sceneFlowSystem != null
                ? _sceneFlowSystem.GetComponent<TransitionController>()
                : null;

            _cutsceneSystem.Initialize(transitionController);
        }

        private void InitializeMenuPanels()
        {
            if (_saveSlotPanel != null) _saveSlotPanel.Initialize(_saveSystem);
            if (_questLogPanel != null) _questLogPanel.Initialize(_questSystem);
        }

        private void AnnounceReady()
        {
            _coreReady = true;

            DebugSystem.Log("Core initialized. All systems ready.", "Bootstrap", "BootstrapSystem");
            EventBus.Raise(new CoreInitializedEvent());

            if (_pendingPlayer != null)
                QueuePlayerInitialization(_pendingPlayer);

            _pendingPlayer = null;
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent e)
        {
            PlayerSystem player = e.PlayerObject != null ? e.PlayerObject.GetComponent<PlayerSystem>() : null;
            if (player == null) return;

            if (!_coreReady)
            {
                _pendingPlayer = player;
                return;
            }

            QueuePlayerInitialization(player);
        }

        private void QueuePlayerInitialization(PlayerSystem player)
        {
            StartCoroutine(InitializePlayerNextFrame(player));
        }

        private IEnumerator InitializePlayerNextFrame(PlayerSystem player)
        {
            yield return null;

            if (player == null) yield break;

            _initializedPlayers.RemoveWhere(p => p == null);
            if (!_initializedPlayers.Add(player)) yield break;

            player.Initialize(_globalVariablesSystem, _saveSystem, _itemDatabase);

            if (_inventoryPanel != null)
                _inventoryPanel.Initialize(player.InventorySystem, _itemDatabase);
        }
    }
}
