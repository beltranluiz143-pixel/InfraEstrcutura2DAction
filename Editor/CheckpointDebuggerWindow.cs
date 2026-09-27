using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class CheckpointDebuggerWindow : EditorWindow
    {
        private const double REFRESH_INTERVAL_SECONDS = 3.0;

        private Vector2 _scrollPos;
        private string _searchFilter = "";
        private readonly List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        private double _lastRefreshTime;

        [MenuItem(EditorToolsUtility.MenuRoot + "Checkpoint Debugger", false, 142)]
        public static void OpenWindow()
        {
            CheckpointDebuggerWindow window = GetWindow<CheckpointDebuggerWindow>("Checkpoint Debugger");
            window.minSize = new Vector2(380, 450);
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Checkpoint Debugger", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Este panel funciona en Play Mode.\n" +
                    "Lista todos los componentes SpawnPoint de las escenas cargadas y mueve al PlayerSystem hasta ellos.",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Actualizar lista", GUILayout.Height(25)))
                RefreshSpawnPoints();
            GUILayout.Label($"{_spawnPoints.Count} puntos encontrados",
                            EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Buscar:", GUILayout.Width(50));
            _searchFilter = EditorGUILayout.TextField(_searchFilter);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (_spawnPoints.Count == 0)
            {
                EditorGUILayout.HelpBox("No se encontraron SpawnPoints en las escenas cargadas.", MessageType.Warning);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (SpawnPoint spawn in _spawnPoints)
            {
                if (spawn == null) continue;

                string label = $"{spawn.gameObject.scene.name} / {spawn.SpawnPointID}";
                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !label.ToLower().Contains(_searchFilter.ToLower())) continue;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(label, GUILayout.ExpandWidth(true));
                GUILayout.Label($"({spawn.transform.position.x:F1}, {spawn.transform.position.y:F1})",
                                EditorStyles.miniLabel, GUILayout.Width(100));

                if (GUILayout.Button("Ir aquí", GUILayout.Width(65)))
                    TeleportPlayerTo(spawn.transform);

                if (GUILayout.Button("📍", GUILayout.Width(30)))
                    Selection.activeGameObject = spawn.gameObject;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshSpawnPoints()
        {
            _spawnPoints.Clear();
            _spawnPoints.AddRange(FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None));

            _spawnPoints.Sort((a, b) =>
            {
                int bySceneName = string.Compare(a.gameObject.scene.name, b.gameObject.scene.name,
                                                 System.StringComparison.Ordinal);
                return bySceneName != 0
                    ? bySceneName
                    : string.Compare(a.SpawnPointID, b.SpawnPointID, System.StringComparison.Ordinal);
            });
        }

        private static void TeleportPlayerTo(Transform spawn)
        {
            PlayerSystem player = FindFirstObjectByType<PlayerSystem>();
            if (player == null)
            {
                Debug.LogWarning("[CheckpointDebugger] No se encontró ningún PlayerSystem en las escenas cargadas.");
                EditorUtility.DisplayDialog(
                    "Player no encontrado",
                    "No se encontró ningún PlayerSystem en las escenas cargadas.",
                    "OK");
                return;
            }

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            player.transform.position = spawn.position;
            Debug.Log($"[CheckpointDebugger] Player teletransportado a: {spawn.name} " +
                      $"({spawn.position.x:F1}, {spawn.position.y:F1})");
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastRefreshTime < REFRESH_INTERVAL_SECONDS) return;

            _lastRefreshTime = now;
            RefreshSpawnPoints();
            Repaint();
        }
    }
}
