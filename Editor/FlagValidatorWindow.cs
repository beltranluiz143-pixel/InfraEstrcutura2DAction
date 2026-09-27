using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class FlagValidatorWindow : EditorWindow
    {
        public enum ErrorType { FlagNotRegistered, PossibleTypo, MissingFlagForID }

        public class ValidationError
        {
            public ErrorType Type;
            public string Message;
            public UnityEngine.Object TargetAsset;
        }

        private readonly List<ValidationError> _errors = new List<ValidationError>();
        private readonly List<string> _allFlags = new List<string>();
        private readonly HashSet<string> _flagSet = new HashSet<string>();
        private readonly HashSet<string> _variableSet = new HashSet<string>();

        private Vector2 _scrollPos;
        private bool _hasValidated;
        private string _searchFilter = "";
        private bool _showRegistered = true;
        private bool _showTypos = true;
        private bool _showMissing = true;

        [MenuItem(EditorToolsUtility.MenuRoot + "Flag Validator", false, 101)]
        public static void OpenWindow()
        {
            FlagValidatorWindow window = GetWindow<FlagValidatorWindow>("Flag Validator");
            window.minSize = new Vector2(620, 550);
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Flag Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Validate All", GUILayout.Height(30)))
                RunValidation();
            if (_hasValidated && GUILayout.Button("Clear", GUILayout.Height(30), GUILayout.Width(70)))
            {
                _errors.Clear();
                _hasValidated = false;
            }
            EditorGUILayout.EndHorizontal();

            if (!_hasValidated) return;

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Buscar:", GUILayout.Width(50));
            _searchFilter = EditorGUILayout.TextField(_searchFilter);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _showRegistered = EditorGUILayout.ToggleLeft("🔴 Sin registrar", _showRegistered, GUILayout.Width(130));
            _showTypos = EditorGUILayout.ToggleLeft("🟠 Typos", _showTypos, GUILayout.Width(90));
            _showMissing = EditorGUILayout.ToggleLeft("🔵 IDs sin flag", _showMissing, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            if (_errors.Count == 0)
            {
                EditorGUILayout.HelpBox("✅ Sin errores. El sistema de flags está consistente.", MessageType.Info);
                return;
            }

            GUILayout.Label($"Total: {_errors.Count} problemas | Flags registradas: {_allFlags.Count}", EditorStyles.miniLabel);
            EditorGUILayout.Space(3);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (ValidationError error in _errors)
            {
                if (!IsVisible(error)) continue;

                Rect rect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(rect, GetBackground(error.Type));
                GUILayout.Label($"{GetPrefix(error.Type)} {error.Message}", GUILayout.ExpandWidth(true));
                if (error.TargetAsset != null && GUILayout.Button("Ir", GUILayout.Width(35)))
                    Selection.activeObject = error.TargetAsset;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private bool IsVisible(ValidationError error)
        {
            bool typeVisible = error.Type switch
            {
                ErrorType.FlagNotRegistered => _showRegistered,
                ErrorType.PossibleTypo => _showTypos,
                ErrorType.MissingFlagForID => _showMissing,
                _ => true
            };

            if (!typeVisible) return false;

            return string.IsNullOrEmpty(_searchFilter) ||
                   error.Message.ToLower().Contains(_searchFilter.ToLower());
        }

        private static Color GetBackground(ErrorType type) => type switch
        {
            ErrorType.FlagNotRegistered => new Color(1f, 0.3f, 0.3f, 0.15f),
            ErrorType.PossibleTypo => new Color(1f, 0.6f, 0.2f, 0.15f),
            ErrorType.MissingFlagForID => new Color(0.3f, 0.6f, 1f, 0.15f),
            _ => Color.clear
        };

        private static string GetPrefix(ErrorType type) => type switch
        {
            ErrorType.FlagNotRegistered => "🔴",
            ErrorType.PossibleTypo => "🟠",
            ErrorType.MissingFlagForID => "🔵",
            _ => "⚪"
        };

        private void RunValidation()
        {
            _errors.Clear();
            _allFlags.Clear();
            _flagSet.Clear();
            _variableSet.Clear();

            EditorUtility.DisplayProgressBar("Flag Validator", "Recopilando flags y variables...", 0f);
            CollectAllRegisteredFlags();
            CollectAllRegisteredVariables();

            EditorUtility.DisplayProgressBar("Flag Validator", "Validando QuestData...", 0.2f);
            ValidateQuestData();

            EditorUtility.DisplayProgressBar("Flag Validator", "Validando NPCData...", 0.4f);
            ValidateNPCData();

            EditorUtility.DisplayProgressBar("Flag Validator", "Validando BossData...", 0.6f);
            ValidateBossData();

            EditorUtility.DisplayProgressBar("Flag Validator", "Validando DialogueData...", 0.75f);
            ValidateDialogueData();

            EditorUtility.DisplayProgressBar("Flag Validator", "Validando EnemyData...", 0.85f);
            ValidateEnemyData();

            EditorUtility.DisplayProgressBar("Flag Validator", "Detectando typos...", 0.95f);
            DetectPossibleTypos();

            EditorUtility.ClearProgressBar();
            _hasValidated = true;

            Debug.Log($"[FlagValidator] Completado. {_errors.Count} problemas. {_allFlags.Count} flags escaneadas.");
        }

        private void CollectAllRegisteredFlags()
        {
            foreach (FlagDatabase db in EditorToolsUtility.LoadAll<FlagDatabase>())
            {
                foreach (var (key, _) in db.GetAllFlags())
                {
                    if (_flagSet.Add(key))
                    {
                        _allFlags.Add(key);
                        continue;
                    }

                    _errors.Add(new ValidationError
                    {
                        Type = ErrorType.PossibleTypo,
                        Message = $"Flag duplicada entre FlagDatabases: {key}",
                        TargetAsset = db
                    });
                }
            }
        }

        private void CollectAllRegisteredVariables()
        {
            foreach (VariableDatabase db in EditorToolsUtility.LoadAll<VariableDatabase>())
                foreach (var (key, _) in db.GetAllVariables())
                    _variableSet.Add(key);
        }

        private void ValidateQuestData()
        {
            foreach (QuestData quest in EditorToolsUtility.LoadAll<QuestData>())
            {
                if (string.IsNullOrEmpty(quest.QuestID)) continue;

                CheckExpectedFlag(FlagConventions.Format(FlagConventions.QuestCompleted, quest.QuestID),
                                  quest, $"QuestData '{quest.QuestID}'");
                CheckCondition(quest.StartCondition, quest, $"Quest '{quest.QuestID}' (condición de inicio)");
            }
        }

        private void ValidateNPCData()
        {
            foreach (NPCData npc in EditorToolsUtility.LoadAll<NPCData>())
            {
                if (string.IsNullOrEmpty(npc.NPCID)) continue;

                CheckExpectedFlag(FlagConventions.Format(FlagConventions.NpcMet, npc.NPCID),
                                  npc, $"NPCData '{npc.NPCID}'");

                if (npc.DialogueStates == null) continue;

                foreach (NPCData.ConditionalDialogue state in npc.DialogueStates)
                    if (state != null)
                        CheckCondition(state.Condition, npc, $"NPC '{npc.NPCID}' (estado de diálogo)");
            }
        }

        private void ValidateBossData()
        {
            foreach (BossData boss in EditorToolsUtility.LoadAll<BossData>())
            {
                if (string.IsNullOrEmpty(boss.BossID)) continue;

                CheckExpectedFlag(FlagConventions.Format(FlagConventions.BossDefeated, boss.BossID),
                                  boss, $"BossData '{boss.BossID}'");
            }
        }

        private void ValidateDialogueData()
        {
            foreach (DialogueData dialogue in EditorToolsUtility.LoadAll<DialogueData>())
            {
                if (dialogue.Lines == null) continue;

                foreach (DialogueData.DialogueLine line in dialogue.Lines)
                    if (line != null)
                        CheckCondition(line.Condition, dialogue, $"Diálogo '{dialogue.DialogueID}'");
            }
        }

        private void ValidateEnemyData()
        {
            foreach (EnemyData enemy in EditorToolsUtility.LoadAll<EnemyData>())
            {
                if (!FlagConventions.IsScreamingSnake(enemy.EnemyID))
                    _errors.Add(new ValidationError
                    {
                        Type = ErrorType.PossibleTypo,
                        Message = $"EnemyData '{enemy.name}' tiene un ID que no es SCREAMING_SNAKE_CASE: '{enemy.EnemyID}'",
                        TargetAsset = enemy
                    });
            }
        }

        private void CheckCondition(FlagCondition condition, UnityEngine.Object asset, string label)
        {
            if (condition == null || string.IsNullOrEmpty(condition.Key)) return;

            bool isFlagCondition = condition.Type == FlagCondition.ConditionType.FlagIsTrue ||
                                   condition.Type == FlagCondition.ConditionType.FlagIsFalse;

            if (isFlagCondition && !_flagSet.Contains(condition.Key))
                _errors.Add(new ValidationError
                {
                    Type = ErrorType.FlagNotRegistered,
                    Message = $"{label} usa flag no registrada: '{condition.Key}'",
                    TargetAsset = asset
                });
            else if (!isFlagCondition && !_variableSet.Contains(condition.Key))
                _errors.Add(new ValidationError
                {
                    Type = ErrorType.FlagNotRegistered,
                    Message = $"{label} usa variable no registrada: '{condition.Key}'",
                    TargetAsset = asset
                });
        }

        private void CheckExpectedFlag(string expectedFlag, UnityEngine.Object asset, string label)
        {
            if (!_flagSet.Contains(expectedFlag))
                _errors.Add(new ValidationError
                {
                    Type = ErrorType.MissingFlagForID,
                    Message = $"{label} no tiene '{expectedFlag}' en ninguna FlagDatabase",
                    TargetAsset = asset
                });
        }

        private void DetectPossibleTypos()
        {
            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();

            foreach (string flag in _allFlags)
            {
                string prefix = GetFlagPrefix(flag);
                if (!groups.ContainsKey(prefix))
                    groups[prefix] = new List<string>();
                groups[prefix].Add(flag);
            }

            foreach (List<string> group in groups.Values)
            {
                if (group.Count < 2 || group.Count > 100) continue;

                for (int i = 0; i < group.Count; i++)
                    for (int j = i + 1; j < group.Count; j++)
                        if (AreSimilar(group[i], group[j]))
                            _errors.Add(new ValidationError
                            {
                                Type = ErrorType.PossibleTypo,
                                Message = $"Flags muy similares (¿typo?): '{group[i]}' vs '{group[j]}'"
                            });
            }
        }

        private static string GetFlagPrefix(string flag)
        {
            int idx = flag.IndexOf('_');
            return idx < 0 ? flag : flag.Substring(0, idx);
        }

        private static bool AreSimilar(string a, string b)
        {
            if (a == b) return false;
            if (Mathf.Abs(a.Length - b.Length) > 2) return false;

            int differences = 0;
            int minLen = Mathf.Min(a.Length, b.Length);
            for (int i = 0; i < minLen; i++)
                if (a[i] != b[i]) differences++;

            differences += Mathf.Abs(a.Length - b.Length);
            return differences == 1;
        }
    }
}
