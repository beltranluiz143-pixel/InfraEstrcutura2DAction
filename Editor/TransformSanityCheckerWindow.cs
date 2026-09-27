using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infra2DAction.EditorTools
{
    public class TransformSanityCheckerWindow : EditorWindow
    {
        private const float POSITION_ABSURD_THRESHOLD = 100000f;
        private const float SCALE_MIN = 0.01f;
        private const float SCALE_MAX = 50f;

        public enum ProblemType { AbsurdPosition, ZeroScale, SuspiciousScale }

        public class Problem
        {
            public Transform Target;
            public ProblemType Type;
            public string Path;
            public string Description;
        }

        private readonly List<Problem> _problems = new List<Problem>();
        private Vector2 _scrollPos;
        private bool _hasScanned;

        [MenuItem(EditorToolsUtility.MenuRoot + "Transform Sanity Checker", false, 180)]
        public static void OpenWindow()
        {
            TransformSanityCheckerWindow window = GetWindow<TransformSanityCheckerWindow>("Transform Sanity Checker");
            window.minSize = new Vector2(640, 420);
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Transform Sanity Checker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Escanea todas las escenas abiertas ahora mismo en el Editor buscando " +
                "posiciones absurdas (arrastre accidental) o escalas rotas (0, o fuera de " +
                "un rango razonable). Funciona también dentro de PrefabInstances.",
                MessageType.Info);

            EditorGUILayout.Space(6);

            if (GUILayout.Button("🔍 Escanear escenas abiertas", GUILayout.Height(30)))
                Scan();

            EditorGUILayout.Space(6);

            if (!_hasScanned)
            {
                EditorGUILayout.LabelField("Todavía no se ha escaneado.");
                return;
            }

            if (_problems.Count == 0)
            {
                EditorGUILayout.HelpBox("Ningún problema encontrado. Todo limpio.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"{_problems.Count} problema(s) encontrado(s):", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (Problem problem in _problems)
            {
                if (problem.Target == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(problem.Path, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(problem.Description);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Seleccionar", GUILayout.Width(100)))
                {
                    Selection.activeGameObject = problem.Target.gameObject;
                    EditorGUIUtility.PingObject(problem.Target.gameObject);
                    SceneView.FrameLastActiveSceneView();
                }

                if (problem.Type == ProblemType.AbsurdPosition)
                {
                    if (GUILayout.Button("Resetear posición a (0,0,0)", GUILayout.Width(220)))
                        ApplyFix(problem, () => problem.Target.localPosition = Vector3.zero);
                }
                else if (GUILayout.Button("Resetear escala a (1,1,1)", GUILayout.Width(220)))
                {
                    ApplyFix(problem, () => problem.Target.localScale = Vector3.one);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ApplyFix(Problem problem, System.Action fix)
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Confirmar corrección",
                $"Vas a modificar el Transform de '{problem.Target.name}' ({problem.Path}).\n\n¿Seguro?",
                "Sí, corregir", "Cancelar");

            if (!confirmed) return;

            Undo.RecordObject(problem.Target, "Transform Sanity Fix");
            fix();
            EditorUtility.SetDirty(problem.Target);
            EditorSceneManager.MarkSceneDirty(problem.Target.gameObject.scene);
            Scan();
        }

        private void Scan()
        {
            _problems.Clear();
            _hasScanned = true;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (GameObject root in scene.GetRootGameObjects())
                    ScanRecursive(root.transform, scene.name);
            }

            Debug.Log($"[TransformSanityChecker] Escaneo completo: {_problems.Count} problema(s) en " +
                      $"{SceneManager.sceneCount} escena(s) abierta(s).");
        }

        private void ScanRecursive(Transform target, string sceneName)
        {
            CheckTransform(target, sceneName);

            for (int i = 0; i < target.childCount; i++)
                ScanRecursive(target.GetChild(i), sceneName);
        }

        private void CheckTransform(Transform target, string sceneName)
        {
            string path = $"{sceneName} / {GetHierarchyPath(target)}";
            Vector3 position = target.localPosition;
            Vector3 scale = target.localScale;

            if (Mathf.Abs(position.x) > POSITION_ABSURD_THRESHOLD ||
                Mathf.Abs(position.y) > POSITION_ABSURD_THRESHOLD ||
                Mathf.Abs(position.z) > POSITION_ABSURD_THRESHOLD)
            {
                _problems.Add(new Problem
                {
                    Target = target,
                    Type = ProblemType.AbsurdPosition,
                    Path = path,
                    Description = $"Posición local absurda: {position}. Probablemente un arrastre " +
                                  "accidental en la Scene view, o una edición hecha en Play Mode."
                });
                return;
            }

            if (scale.x == 0f || scale.y == 0f || scale.z == 0f)
            {
                _problems.Add(new Problem
                {
                    Target = target,
                    Type = ProblemType.ZeroScale,
                    Path = path,
                    Description = $"Escala con un eje en 0: {scale}. El objeto es efectivamente invisible/sin colisión."
                });
            }
            else if (scale.x < SCALE_MIN || scale.x > SCALE_MAX ||
                     scale.y < SCALE_MIN || scale.y > SCALE_MAX ||
                     scale.z < SCALE_MIN || scale.z > SCALE_MAX)
            {
                _problems.Add(new Problem
                {
                    Target = target,
                    Type = ProblemType.SuspiciousScale,
                    Path = path,
                    Description = $"Escala fuera de rango razonable: {scale}. Revisa si es intencional " +
                                  "(un fondo enorme, por ejemplo) o un error de arrastre."
                });
            }
        }

        private static string GetHierarchyPath(Transform target)
        {
            string path = target.name;
            Transform current = target.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
