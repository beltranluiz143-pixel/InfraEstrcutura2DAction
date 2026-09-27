using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class QuestDebuggerWindow : EditorWindow
    {
        private Vector2 _scrollQuests;
        private Vector2 _scrollAll;
        private string _searchFilter = "";
        private bool _showAllQuests;
        private QuestSystem _cachedSystem;
        private List<QuestData> _allQuests = new List<QuestData>();

        [MenuItem(EditorToolsUtility.MenuRoot + "Quest Debugger", false, 141)]
        public static void OpenWindow()
        {
            QuestDebuggerWindow window = GetWindow<QuestDebuggerWindow>("Quest Debugger");
            window.minSize = new Vector2(500, 600);
        }

        private void OnEnable() => _allQuests = EditorToolsUtility.LoadAll<QuestData>();

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Quest Debugger", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Este panel funciona en Play Mode.", MessageType.Info);
                DrawAllQuestsReadOnly();
                return;
            }

            if (_cachedSystem == null)
                _cachedSystem = FindFirstObjectByType<QuestSystem>();

            if (_cachedSystem == null)
            {
                EditorGUILayout.HelpBox("QuestSystem no encontrado en la escena.", MessageType.Warning);
                return;
            }

            DrawRuntimeQuests();
        }

        private void DrawRuntimeQuests()
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Buscar:", GUILayout.Width(90));
            _searchFilter = EditorGUILayout.TextField(_searchFilter);
            EditorGUILayout.EndHorizontal();

            _showAllQuests = EditorGUILayout.Toggle("Mostrar todas las quests", _showAllQuests);
            EditorGUILayout.Space(5);

            _scrollQuests = EditorGUILayout.BeginScrollView(_scrollQuests);

            foreach (QuestData quest in _allQuests.OrderBy(q => q.QuestID))
            {
                if (string.IsNullOrEmpty(quest.QuestID)) continue;

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !quest.QuestID.ToLower().Contains(_searchFilter.ToLower())) continue;

                bool completed = _cachedSystem.IsQuestCompleted(quest.QuestID);
                bool active = _cachedSystem.IsQuestActive(quest.QuestID);

                if (!_showAllQuests && !active && !completed) continue;

                Color background = completed ? new Color(0.4f, 1f, 0.4f, 0.15f) :
                                   active ? new Color(1f, 1f, 0.4f, 0.15f) :
                                            new Color(0.5f, 0.5f, 0.5f, 0.1f);

                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.DrawRect(rect, background);

                EditorGUILayout.BeginHorizontal();
                string status = completed ? "✅" : active ? "🔄" : "⭕";
                GUILayout.Label($"{status} {quest.QuestID}", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

                if (!active && !completed && GUILayout.Button("Activar", GUILayout.Width(65)))
                    EventBus.Raise(new QuestStartRequestEvent(quest, "QuestDebugger"));

                if (!completed && GUILayout.Button("Completar", GUILayout.Width(75)))
                    EventBus.Raise(new QuestCompleteRequestEvent(quest, "QuestDebugger"));

                if ((active || completed) && GUILayout.Button("Reset", GUILayout.Width(55)))
                    EventBus.Raise(new QuestResetRequestEvent(quest, "QuestDebugger"));

                EditorGUILayout.EndHorizontal();

                if (active && quest.Objectives != null)
                    foreach (QuestData.QuestObjective objective in quest.Objectives)
                        EditorGUILayout.LabelField(
                            $"   → [{objective.Type}] {objective.TargetID} (x{objective.RequiredAmount})",
                            EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAllQuestsReadOnly()
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("Quests en el proyecto:", EditorStyles.miniLabel);
            _scrollAll = EditorGUILayout.BeginScrollView(_scrollAll, GUILayout.Height(300));
            foreach (QuestData quest in _allQuests.OrderBy(q => q.QuestID))
                EditorGUILayout.LabelField($"  • {quest.QuestID}");
            EditorGUILayout.EndScrollView();
        }

        private void Update()
        {
            if (Application.isPlaying) Repaint();
        }
    }
}
