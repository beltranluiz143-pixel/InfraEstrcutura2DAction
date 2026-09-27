using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Infra2DAction.EditorTools
{
    public class SceneSetupWindow : EditorWindow
    {
        private string _sceneName = "";
        private string _scenePrefix = "Scene_";
        private string _chapter;
        private string _groundSortingLayer = "Ground";
        private string _decorationSortingLayer = "Decoration";
        private string _groundPhysicsLayer = "Ground";

        private bool _addToBuilds = true;
        private bool _createSceneRef = true;
        private bool _addCameraZone = true;
        private bool _addFollowTarget = true;
        private bool _addStandaloneCamera;
        private bool _addTilemap = true;
        private bool _addSpawn = true;
        private bool _showPaths;

        private Vector2 _scrollLog;
        private readonly List<string> _log = new List<string>();

        [MenuItem(EditorToolsUtility.MenuRoot + "Scene Setup", false, 121)]
        public static void OpenWindow()
        {
            SceneSetupWindow window = GetWindow<SceneSetupWindow>("Scene Setup");
            window.minSize = new Vector2(420, 640);
        }

        private void OnEnable()
        {
            _chapter = EditorToolsUtility.LastChapter;
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Scene Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            _scenePrefix = EditorGUILayout.TextField("Prefijo de escena:", _scenePrefix);
            EditorGUILayout.LabelField("Nombre de la escena (sin prefijo):");
            _sceneName = EditorGUILayout.TextField(_sceneName).Replace(" ", "_");

            EditorGUILayout.Space(5);

            _showPaths = EditorGUILayout.Foldout(_showPaths, "Rutas y capas", true);
            if (_showPaths)
            {
                EditorToolsUtility.DrawPathSettings();
                _groundSortingLayer = EditorGUILayout.TextField("Sorting layer suelo:", _groundSortingLayer);
                _decorationSortingLayer = EditorGUILayout.TextField("Sorting layer decoración:", _decorationSortingLayer);
                _groundPhysicsLayer = EditorGUILayout.TextField("Layer físico del suelo:", _groundPhysicsLayer);
            }

            string previousChapter = _chapter;
            _chapter = EditorToolsUtility.DrawChapterField(_chapter, EditorToolsUtility.ContentRoot);
            if (_chapter != previousChapter) EditorToolsUtility.LastChapter = _chapter;

            EditorGUILayout.Space(8);
            GUILayout.Label("Opciones:", EditorStyles.boldLabel);
            _addCameraZone = EditorGUILayout.Toggle("Añadir zona de cámara (límites)", _addCameraZone);
            _addFollowTarget = EditorGUILayout.Toggle("Añadir CameraFollowTarget", _addFollowTarget);
            _addStandaloneCamera = EditorGUILayout.Toggle("Añadir Main Camera (solo pruebas sin escena Core)", _addStandaloneCamera);
            _addTilemap = EditorGUILayout.Toggle("Añadir Tilemap Grid con layers", _addTilemap);
            _addSpawn = EditorGUILayout.Toggle("Añadir SpawnPoint por defecto", _addSpawn);
            _addToBuilds = EditorGUILayout.Toggle("Añadir al Build Settings", _addToBuilds);
            _createSceneRef = EditorGUILayout.Toggle("Crear SceneReference.asset", _createSceneRef);

            EditorGUILayout.Space(5);

            string fullName = GetFullName();
            string scenePath = GetScenePath(fullName);

            EditorGUILayout.LabelField("Nombre final:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  {fullName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  {scenePath}", EditorStyles.miniLabel);

            EditorGUILayout.Space(10);

            if (string.IsNullOrEmpty(_sceneName))
                EditorGUILayout.HelpBox("Escribe el nombre de la escena.", MessageType.Warning);
            else if (GUILayout.Button($"✅ Crear escena {fullName}", GUILayout.Height(35)))
                CreateScene(fullName, scenePath);

            if (_log.Count == 0) return;

            EditorGUILayout.Space(8);
            GUILayout.Label("Resultado:", EditorStyles.boldLabel);
            _scrollLog = EditorGUILayout.BeginScrollView(_scrollLog, GUILayout.Height(130));
            foreach (string entry in _log)
                EditorGUILayout.LabelField($"  {entry}");
            EditorGUILayout.EndScrollView();
        }

        private string GetFullName()
        {
            if (string.IsNullOrEmpty(_sceneName)) return $"{_scenePrefix}???";
            return $"{_scenePrefix}{EditorToolsUtility.NormalizeChapter(_chapter)}_{_sceneName}";
        }

        private string GetScenePath(string fullName)
            => $"{EditorToolsUtility.ScenesRoot}/{EditorToolsUtility.NormalizeChapter(_chapter)}/{fullName}.unity";

        private void CreateScene(string fullName, string scenePath)
        {
            _log.Clear();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
            {
                EditorUtility.DisplayDialog("Escena existente",
                    $"Ya existe una escena en {scenePath}.\nElige otro nombre o bórrala primero.", "OK");
                return;
            }

            string sceneFolder = $"{EditorToolsUtility.ScenesRoot}/{EditorToolsUtility.NormalizeChapter(_chapter)}";
            EditorToolsUtility.EnsureFolderPath(sceneFolder);

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            newScene.name = fullName;

            if (_addCameraZone || _addFollowTarget || _addStandaloneCamera) AddCameraRig(newScene);
            if (_addTilemap) AddTilemap(newScene);
            if (_addSpawn) AddSpawnPoint(newScene);

            EditorSceneManager.SaveScene(newScene, scenePath);
            _log.Add($"✅ Escena creada: {scenePath}");

            if (_addToBuilds)
            {
                AddToBuildSettings(scenePath);
                _log.Add("✅ Añadida al Build Settings");
            }

            if (_createSceneRef) CreateSceneReference(fullName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Escena creada",
                $"Escena {fullName} creada correctamente.\n\n" +
                "Recuerda:\n" +
                "→ Asignar el Transform del Player al CameraFollowTarget\n" +
                "→ Ajustar el polígono de la zona de cámara al tamaño de la sala\n" +
                "→ Asignar un CameraData a la zona (opcional)",
                "OK");
        }

        private void AddCameraRig(Scene scene)
        {
            GameObject root = new GameObject("CameraRig");
            SceneManager.MoveGameObjectToScene(root, scene);

            if (_addStandaloneCamera)
            {
                GameObject mainCam = new GameObject("Main Camera");
                mainCam.AddComponent<Camera>();
                mainCam.tag = "MainCamera";
                mainCam.transform.SetParent(root.transform);
                mainCam.transform.position = new Vector3(0f, 0f, -10f);
                _log.Add("✅ Main Camera añadida (desactívala si la escena se carga junto a la escena Core)");
            }

            if (_addFollowTarget)
            {
                GameObject target = new GameObject("CameraFollowTarget");
                target.transform.SetParent(root.transform);
                target.AddComponent<CameraFollowTarget>();
                _log.Add("✅ CameraFollowTarget añadido — asigna el Transform del Player");
            }

            if (_addCameraZone)
            {
                GameObject zone = new GameObject("CameraZone");
                zone.transform.SetParent(root.transform);

                PolygonCollider2D bounds = zone.AddComponent<PolygonCollider2D>();
                bounds.isTrigger = true;
                bounds.SetPath(0, new[]
                {
                    new Vector2(-20f, -11f), new Vector2(20f, -11f),
                    new Vector2(20f, 11f), new Vector2(-20f, 11f)
                });

                CameraTrigger trigger = zone.AddComponent<CameraTrigger>();
                SerializedObject serialized = new SerializedObject(trigger);
                SerializedProperty boundsProp = serialized.FindProperty("_boundsCollider");
                if (boundsProp != null) boundsProp.objectReferenceValue = bounds;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                _log.Add("✅ CameraZone con PolygonCollider2D (trigger) añadida");
            }
        }

        private void AddTilemap(Scene scene)
        {
            GameObject grid = new GameObject("Grid");
            grid.AddComponent<Grid>();
            SceneManager.MoveGameObjectToScene(grid, scene);

            GameObject groundTilemap = new GameObject("Tilemap_Ground");
            groundTilemap.transform.SetParent(grid.transform);
            groundTilemap.AddComponent<Tilemap>();

            TilemapRenderer groundRenderer = groundTilemap.AddComponent<TilemapRenderer>();
            ApplySortingLayer(groundRenderer, _groundSortingLayer, 0);

            TilemapCollider2D tilemapCollider = groundTilemap.AddComponent<TilemapCollider2D>();
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            Rigidbody2D body = groundTilemap.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            groundTilemap.AddComponent<CompositeCollider2D>();

            int groundLayer = LayerMask.NameToLayer(_groundPhysicsLayer);
            if (groundLayer >= 0)
                groundTilemap.layer = groundLayer;
            else
                _log.Add($"⚠️ Layer '{_groundPhysicsLayer}' no existe — créalo en Project Settings → Tags and Layers");

            GameObject decoTilemap = new GameObject("Tilemap_Decoration");
            decoTilemap.transform.SetParent(grid.transform);
            decoTilemap.AddComponent<Tilemap>();

            TilemapRenderer decoRenderer = decoTilemap.AddComponent<TilemapRenderer>();
            ApplySortingLayer(decoRenderer, _decorationSortingLayer, 1);

            _log.Add("✅ Grid con Tilemap_Ground y Tilemap_Decoration añadido");
        }

        private void ApplySortingLayer(Renderer renderer, string layerName, int order)
        {
            renderer.sortingOrder = order;

            foreach (SortingLayer layer in SortingLayer.layers)
            {
                if (layer.name != layerName) continue;

                renderer.sortingLayerName = layerName;
                return;
            }

            _log.Add($"⚠️ Sorting layer '{layerName}' no existe — créala en Project Settings → Tags and Layers");
        }

        private void AddSpawnPoint(Scene scene)
        {
            GameObject spawn = new GameObject("SpawnPoint_Default");
            spawn.transform.position = Vector3.zero;
            spawn.AddComponent<SpawnPoint>();
            SceneManager.MoveGameObjectToScene(spawn, scene);
            _log.Add("✅ SpawnPoint_Default añadido en (0,0,0)");
        }

        private void CreateSceneReference(string fullName)
        {
            string folder = $"{EditorToolsUtility.ContentRoot}/{EditorToolsUtility.NormalizeChapter(_chapter)}/SceneReferences";
            EditorToolsUtility.EnsureFolderPath(folder);

            string assetPath = $"{folder}/SceneReference_{fullName}.asset";

            if (AssetDatabase.LoadAssetAtPath<SceneReference>(assetPath) != null)
            {
                _log.Add("⚠️ SceneReference ya existe (omitido)");
                return;
            }

            SceneReference sceneRef = ScriptableObject.CreateInstance<SceneReference>();
            sceneRef.SceneName = fullName;
            sceneRef.DefaultSpawnPointID = "Default";
            AssetDatabase.CreateAsset(sceneRef, assetPath);

            _log.Add($"✅ SceneReference_{fullName}.asset creada");
        }

        private static void AddToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene existing in scenes)
                if (existing.path == scenePath) return;

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
