using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class NPCDialogueMapWindow : EditorWindow
    {
        private NPCData _selectedNPC;
        private Vector2 _scrollPos;

        [MenuItem(EditorToolsUtility.MenuRoot + "NPC Dialogue Map", false, 161)]
        public static void OpenWindow()
        {
            NPCDialogueMapWindow window = GetWindow<NPCDialogueMapWindow>("NPC Dialogue Map");
            window.minSize = new Vector2(480, 550);
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "NPC Dialogue Map", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            NPCData previous = _selectedNPC;
            _selectedNPC = (NPCData)EditorGUILayout.ObjectField("NPCData:", _selectedNPC, typeof(NPCData), false);

            if (_selectedNPC != previous) _scrollPos = Vector2.zero;

            if (_selectedNPC == null)
            {
                EditorGUILayout.HelpBox("Arrastra un NPCData para ver su árbol de diálogos.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);
            GUILayout.Label($"NPC ID: {_selectedNPC.NPCID}", EditorStyles.boldLabel);

            if (GUILayout.Button("Seleccionar en Project", GUILayout.Width(170)))
                Selection.activeObject = _selectedNPC;

            EditorGUILayout.Space(8);

            if (_selectedNPC.DialogueStates == null || _selectedNPC.DialogueStates.Count == 0)
            {
                EditorGUILayout.HelpBox("Este NPC no tiene estados de diálogo configurados.", MessageType.Warning);

                if (_selectedNPC.FallbackDialogue != null)
                {
                    GUILayout.Label("Diálogo Fallback:", EditorStyles.boldLabel);
                    DrawDialogueEntry(_selectedNPC.FallbackDialogue);
                }

                return;
            }

            List<NPCData.ConditionalDialogue> sorted = new List<NPCData.ConditionalDialogue>(_selectedNPC.DialogueStates);
            sorted.RemoveAll(state => state == null);
            sorted.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            GUILayout.Label($"Estados de diálogo ({sorted.Count} + fallback):", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < sorted.Count; i++)
                DrawConditionalDialogue(sorted[i], i + 1);

            if (_selectedNPC.FallbackDialogue != null)
            {
                EditorGUILayout.Space(5);
                GUILayout.Label("▼ FALLBACK (si ninguna condición se cumple)", EditorStyles.miniLabel);
                DrawDialogueEntry(_selectedNPC.FallbackDialogue);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawConditionalDialogue(NPCData.ConditionalDialogue state, int index)
        {
            Color background = state.Condition != null
                ? new Color(0.3f, 0.6f, 1f, 0.12f)
                : new Color(0.5f, 0.5f, 0.5f, 0.1f);

            Rect rect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(rect, background);

            string conditionLabel = state.Condition != null
                ? $"[{state.Condition.Type}] {state.Condition.Key}"
                : "Sin condición";

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"#{index} — Prioridad {state.Priority}", EditorStyles.boldLabel, GUILayout.Width(160));
            GUILayout.Label($"⚙ {conditionLabel}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            DrawDialogueEntry(state.Dialogue);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawDialogueEntry(DialogueData dialogue)
        {
            if (dialogue == null)
            {
                EditorGUILayout.HelpBox("⚠️ DialogueData no asignado.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.Label($"📄 {dialogue.DialogueID}", EditorStyles.miniLabel);

            int lineCount = dialogue.Lines != null ? dialogue.Lines.Count : 0;
            int choiceCount = 0;
            if (dialogue.Lines != null)
                foreach (DialogueData.DialogueLine line in dialogue.Lines)
                    if (line != null && line.Choices != null) choiceCount += line.Choices.Count;

            GUILayout.Label($"Líneas: {lineCount} | Elecciones: {choiceCount}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(130));
            if (GUILayout.Button("Seleccionar", GUILayout.Height(20)))
                Selection.activeObject = dialogue;
            if (GUILayout.Button("Vista previa", GUILayout.Height(20)))
                DialoguePreviewWindow.OpenWith(dialogue);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}
