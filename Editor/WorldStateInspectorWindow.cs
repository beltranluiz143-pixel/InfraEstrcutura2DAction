using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class WorldStateInspectorWindow : EditorWindow
    {
        private Vector2 _scrollFlags;
        private Vector2 _scrollVars;
        private string _flagSearch = "";
        private string _varSearch = "";
        private bool _showFlags = true;
        private bool _showVars = true;
        private GlobalVariablesSystem _cachedSystem;

        [MenuItem(EditorToolsUtility.MenuRoot + "World State Inspector", false, 140)]
        public static void OpenWindow()
        {
            WorldStateInspectorWindow window = GetWindow<WorldStateInspectorWindow>("World State");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "World State Inspector", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Este panel funciona en Play Mode.", MessageType.Info);
                return;
            }

            if (_cachedSystem == null)
                _cachedSystem = FindFirstObjectByType<GlobalVariablesSystem>();

            if (_cachedSystem == null)
            {
                EditorGUILayout.HelpBox("GlobalVariablesSystem no encontrado en la escena.", MessageType.Warning);
                return;
            }

            DrawFlags();
            DrawVariables();
        }

        private void DrawFlags()
        {
            EditorGUILayout.Space(5);
            _showFlags = EditorGUILayout.Foldout(_showFlags, "🚩 FLAGS", true, EditorStyles.boldLabel);
            if (!_showFlags) return;

            Dictionary<string, bool> flags = _cachedSystem.GetAllFlagsForEditor();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Buscar:", GUILayout.Width(50));
            _flagSearch = EditorGUILayout.TextField(_flagSearch);
            EditorGUILayout.EndHorizontal();

            _scrollFlags = EditorGUILayout.BeginScrollView(_scrollFlags, GUILayout.Height(250));

            foreach (KeyValuePair<string, bool> flag in flags.OrderBy(f => f.Key))
            {
                if (!string.IsNullOrEmpty(_flagSearch) &&
                    !flag.Key.ToLower().Contains(_flagSearch.ToLower())) continue;

                EditorGUILayout.BeginHorizontal();

                Color previous = GUI.backgroundColor;
                GUI.backgroundColor = flag.Value ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
                bool newValue = EditorGUILayout.Toggle(flag.Value, GUILayout.Width(20));
                GUI.backgroundColor = previous;

                GUILayout.Label(flag.Key, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                if (newValue != flag.Value)
                    _cachedSystem.SetFlagFromEditor(flag.Key, newValue);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawVariables()
        {
            EditorGUILayout.Space(5);
            _showVars = EditorGUILayout.Foldout(_showVars, "📊 VARIABLES", true, EditorStyles.boldLabel);
            if (!_showVars) return;

            Dictionary<string, float> variables = _cachedSystem.GetAllVariablesForEditor();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Buscar:", GUILayout.Width(50));
            _varSearch = EditorGUILayout.TextField(_varSearch);
            EditorGUILayout.EndHorizontal();

            _scrollVars = EditorGUILayout.BeginScrollView(_scrollVars, GUILayout.Height(200));

            foreach (KeyValuePair<string, float> variable in variables.OrderBy(v => v.Key))
            {
                if (!string.IsNullOrEmpty(_varSearch) &&
                    !variable.Key.ToLower().Contains(_varSearch.ToLower())) continue;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(variable.Key, GUILayout.Width(280));
                float newValue = EditorGUILayout.FloatField(variable.Value, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                if (!Mathf.Approximately(newValue, variable.Value))
                    _cachedSystem.SetVariableFromEditor(variable.Key, newValue);
            }

            EditorGUILayout.EndScrollView();
        }

        private void Update()
        {
            if (Application.isPlaying) Repaint();
        }
    }
}
