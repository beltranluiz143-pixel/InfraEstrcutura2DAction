using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infra2DAction
{
    [RequireComponent(typeof(TransitionController))]
    public class SceneFlowSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SceneReference _initialScene;
        [Tooltip("Activo: si el slot activo ya tiene partida en curso, al arrancar se reanuda ahi en vez de cargar la escena inicial.")]
        [SerializeField] private bool _resumeSavedGameOnBoot = true;

        private SceneLoader _sceneLoader;
        private TransitionController _transitionController;
        private SaveSystem _saveSystem;
        private string _currentSceneName;
        private bool _isTransitioning;

        private readonly List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        private GameObject _playerObject;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            _sceneLoader = new SceneLoader(gameObject.scene.name);
            _transitionController = GetComponent<TransitionController>();

            if (_transitionController == null)
                DebugSystem.LogError("TransitionController no encontrado.", "SceneFlow", "SceneFlowSystem");

            DebugSystem.Log("SceneFlowSystem initialized.", "SceneFlow", "SceneFlowSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Subscribe<SceneTransitionRequestEvent>(OnTransitionRequested);
            EventBus.Subscribe<StartGameRequestEvent>(OnStartGameRequested);
            EventBus.Subscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
            EventBus.Subscribe<SpawnPointRegisteredEvent>(OnSpawnPointRegistered);
            EventBus.Subscribe<SpawnPointUnregisteredEvent>(OnSpawnPointUnregistered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CoreInitializedEvent>(OnCoreInitialized);
            EventBus.Unsubscribe<SceneTransitionRequestEvent>(OnTransitionRequested);
            EventBus.Unsubscribe<StartGameRequestEvent>(OnStartGameRequested);
            EventBus.Unsubscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
            EventBus.Unsubscribe<SpawnPointRegisteredEvent>(OnSpawnPointRegistered);
            EventBus.Unsubscribe<SpawnPointUnregisteredEvent>(OnSpawnPointUnregistered);
        }

        private void OnPlayerRegistered(PlayerRegisteredEvent e)
        {
            _playerObject = e.PlayerObject;
        }

        private void OnSpawnPointRegistered(SpawnPointRegisteredEvent e)
        {
            if (!_spawnPoints.Contains(e.SpawnPoint))
                _spawnPoints.Add(e.SpawnPoint);
        }

        private void OnSpawnPointUnregistered(SpawnPointUnregisteredEvent e)
        {
            _spawnPoints.Remove(e.SpawnPoint);
        }

        private void OnCoreInitialized(CoreInitializedEvent e)
        {
            if (_resumeSavedGameOnBoot)
                StartOrResume(_initialScene);
            else
                GoToScene(_initialScene);
        }

        private void OnStartGameRequested(StartGameRequestEvent e)
        {
            if (_isTransitioning) return;

            StartOrResume(e.NewGameScene);
        }

        private void StartOrResume(SceneReference newGameScene)
        {
            SaveData currentSave = _saveSystem != null ? _saveSystem.GetCurrentSave() : null;
            if (currentSave != null && !string.IsNullOrEmpty(currentSave.Player.CurrentScene))
            {
                string savedSpawnID = string.IsNullOrEmpty(currentSave.Player.LastCheckpointID)
                                      ? "Default" : currentSave.Player.LastCheckpointID;

                DebugSystem.Log($"Reanudando partida guardada en '{currentSave.Player.CurrentScene}' " +
                                $"(checkpoint '{savedSpawnID}').", "SceneFlow", "SceneFlowSystem");

                StartCoroutine(PerformTransitionByName(currentSave.Player.CurrentScene, savedSpawnID, Color.black));
                return;
            }

            GoToScene(newGameScene);
        }

        private void GoToScene(SceneReference scene)
        {
            if (scene == null)
            {
                DebugSystem.LogError("Escena inicial no asignada.", "SceneFlow", "SceneFlowSystem");
                return;
            }

            StartCoroutine(PerformTransition(scene, scene.DefaultSpawnPointID, Color.black));
        }

        private void OnTransitionRequested(SceneTransitionRequestEvent e)
        {
            if (_isTransitioning)
            {
                DebugSystem.LogWarning("Transicion en curso, peticion ignorada.", "SceneFlow", "SceneFlowSystem");
                return;
            }

            StartCoroutine(PerformTransition(e.TargetScene, e.SpawnPointID, e.FadeColor));
        }

        private IEnumerator PerformTransition(SceneReference targetScene, string spawnPointID, Color fadeColor)
        {
            if (targetScene == null)
            {
                DebugSystem.LogError("SceneReference nula pasada a PerformTransition.", "SceneFlow", "SceneFlowSystem");
                yield break;
            }

            yield return StartCoroutine(TransitionCore(targetScene.SceneName, spawnPointID, fadeColor));
        }

        private IEnumerator PerformTransitionByName(string targetSceneName, string spawnPointID, Color fadeColor)
        {
            yield return StartCoroutine(TransitionCore(targetSceneName, spawnPointID, fadeColor));
        }

        private IEnumerator TransitionCore(string targetSceneName, string spawnPointID, Color fadeColor)
        {
            _isTransitioning = true;

            EventBus.Raise(new SceneTransitionStartedEvent());

            yield return StartCoroutine(_transitionController.FadeOut(fadeColor));

            if (!string.IsNullOrEmpty(_currentSceneName))
                yield return StartCoroutine(_sceneLoader.UnloadScene(_currentSceneName));

            yield return StartCoroutine(_sceneLoader.LoadScene(targetSceneName));

            _currentSceneName = targetSceneName;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_currentSceneName));

            RepositionPlayerAtSpawnPoint(spawnPointID);

            EventBus.Raise(new SceneLoadedEvent(_currentSceneName));

            yield return StartCoroutine(_transitionController.FadeIn());

            _isTransitioning = false;

            DebugSystem.Log($"Transition complete: {_currentSceneName} (fade color used)", "SceneFlow", "SceneFlowSystem");
        }

        private void RepositionPlayerAtSpawnPoint(string spawnPointID)
        {
            _spawnPoints.RemoveAll(s => s == null);

            if (_spawnPoints.Count == 0)
            {
                DebugSystem.LogWarning($"No hay ningun SpawnPoint en '{_currentSceneName}'. " +
                                       "El jugador aparece donde ya estuviera colocado en la escena.",
                                       "SceneFlow", "SceneFlowSystem");
                return;
            }

            SpawnPoint target = _spawnPoints.Find(s => s.SpawnPointID == spawnPointID);
            if (target == null)
            {
                DebugSystem.LogWarning($"Ningun SpawnPoint con ID '{spawnPointID}' en '{_currentSceneName}'. " +
                                       $"Usando '{_spawnPoints[0].SpawnPointID}' como alternativa.",
                                       "SceneFlow", "SceneFlowSystem");
                target = _spawnPoints[0];
            }

            if (_playerObject == null)
            {
                DebugSystem.LogWarning("No hay ningun Player registrado para reposicionarlo en el SpawnPoint.",
                                       "SceneFlow", "SceneFlowSystem");
                return;
            }

            Rigidbody2D rb = _playerObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = target.transform.position;
            else _playerObject.transform.position = target.transform.position;

            DebugSystem.Log($"Player reposicionado en SpawnPoint '{target.SpawnPointID}' ({target.transform.position}).",
                            "SceneFlow", "SceneFlowSystem");
        }
    }
}
