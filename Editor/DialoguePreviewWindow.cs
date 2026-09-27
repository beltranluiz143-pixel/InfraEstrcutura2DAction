using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class DialoguePreviewWindow : EditorWindow
    {
        private DialogueData _selectedDialogue;
        private int _currentLineIndex;
        private Vector2 _scrollChoices;
        private GUIStyle _textStyle;

        [MenuItem(EditorToolsUtility.MenuRoot + "Dialogue Preview", false, 160)]
        public static void OpenWindow()
        {
            OpenWith(null);
        }

        public static void OpenWith(DialogueData dialogue)
        {
            DialoguePreviewWindow window = GetWindow<DialoguePreviewWindow>("Dialogue Preview");
            window.minSize = new Vector2(450, 500);

            if (dialogue == null) return;

            window._selectedDialogue = dialogue;
            window._currentLineIndex = 0;
            window.Repaint();
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Dialogue Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DialogueData previous = _selectedDialogue;
            _selectedDialogue = (DialogueData)EditorGUILayout.ObjectField(
                "DialogueData:", _selectedDialogue, typeof(DialogueData), false);

            if (_selectedDialogue != previous) _currentLineIndex = 0;

            if (_selectedDialogue == null)
            {
                EditorGUILayout.HelpBox("Arrastra un DialogueData para previsualizarlo.", MessageType.Info);
                return;
            }

            if (_selectedDialogue.Lines == null || _selectedDialogue.Lines.Count == 0)
            {
                EditorGUILayout.HelpBox("Este DialogueData no tiene líneas.", MessageType.Warning);
                return;
            }

            _currentLineIndex = Mathf.Clamp(_currentLineIndex, 0, _selectedDialogue.Lines.Count - 1);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Anterior", GUILayout.Height(25)))
                _currentLineIndex = Mathf.Max(0, _currentLineIndex - 1);

            GUILayout.Label($"Línea {_currentLineIndex + 1} / {_selectedDialogue.Lines.Count}",
                            EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Siguiente ▶", GUILayout.Height(25)))
                _currentLineIndex = Mathf.Min(_selectedDialogue.Lines.Count - 1, _currentLineIndex + 1);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("🔄 Reiniciar desde el principio"))
                _currentLineIndex = 0;

            EditorGUILayout.Space(10);
            DrawDialoguePanel(_selectedDialogue.Lines[_currentLineIndex]);
        }

        private void DrawDialoguePanel(DialogueData.DialogueLine line)
        {
            if (line == null)
            {
                EditorGUILayout.HelpBox("Esta línea está vacía.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (line.Portrait != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(line.Portrait);
                if (preview != null) GUILayout.Label(preview, GUILayout.Width(60), GUILayout.Height(60));
            }
            else
            {
                GUILayout.Box("Sin retrato", GUILayout.Width(60), GUILayout.Height(60));
            }

            EditorGUILayout.BeginVertical();
            GUILayout.Label($"Personaje: {line.CharacterID}", EditorStyles.boldLabel);
            if (line.Condition != null)
                GUILayout.Label($"⚠ Condición: {line.Condition.Key} ({line.Condition.Type})", EditorStyles.miniLabel);
            if (line.AlternativeLine != null)
                GUILayout.Label("↪ Tiene línea alternativa", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { fontSize = 12 };
                _textStyle.normal.textColor = Color.white;
            }

            Rect textRect = EditorGUILayout.GetControlRect(GUILayout.Height(80));
            EditorGUI.DrawRect(textRect, new Color(0.1f, 0.1f, 0.1f, 0.8f));
            GUI.Label(new RectOffset(8, 8, 8, 8).Remove(textRect), line.Text ?? "", _textStyle);

            EditorGUILayout.Space(5);

            if (line.Choices == null || line.Choices.Count == 0) return;

            GUILayout.Label("Elecciones:", EditorStyles.miniLabel);
            _scrollChoices = EditorGUILayout.BeginScrollView(_scrollChoices, GUILayout.Height(80));
            foreach (DialogueData.DialogueChoice choice in line.Choices)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"▶ {choice.Text}", GUILayout.ExpandWidth(true));
                GUILayout.Label($"[{choice.Decision}]", EditorStyles.miniLabel, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
