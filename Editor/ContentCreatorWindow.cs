using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class ContentCreatorWindow : EditorWindow
    {
        public enum ContentType { NPC, Boss, Quest, Enemy, Item, Cutscene, Minigame }

        private class PlannedAsset
        {
            public System.Type AssetType;
            public string Category;
            public string FileName;
            public System.Action<ScriptableObject> Initialize;
        }

        private ContentType _selectedType = ContentType.NPC;
        private string _entityName = "";
        private string _chapter;
        private bool _showFlags = true;
        private bool _showPaths;
        private Vector2 _scrollPos;
        private readonly List<string> _log = new List<string>();
        private int _createdCount;

        [MenuItem(EditorToolsUtility.MenuRoot + "Content Creator", false, 120)]
        public static void OpenWindow()
        {
            ContentCreatorWindow window = GetWindow<ContentCreatorWindow>("Content Creator");
            window.minSize = new Vector2(460, 580);
        }

        private void OnEnable()
        {
            _chapter = EditorToolsUtility.LastChapter;
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Content Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Tipo de contenido:");
            _selectedType = (ContentType)EditorGUILayout.EnumPopup(_selectedType);

            EditorGUILayout.Space(5);

            _showPaths = EditorGUILayout.Foldout(_showPaths, "Rutas", true);
            if (_showPaths) EditorToolsUtility.DrawPathSettings();

            string previousChapter = _chapter;
            _chapter = EditorToolsUtility.DrawChapterField(_chapter, EditorToolsUtility.ContentRoot);
            if (_chapter != previousChapter) EditorToolsUtility.LastChapter = _chapter;

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Nombre (SCREAMING_SNAKE_CASE):");
            _entityName = FlagConventions.ToScreamingSnake(EditorGUILayout.TextField(_entityName));

            EditorGUILayout.Space(5);
            _showFlags = EditorGUILayout.Toggle("Mostrar flags a añadir manualmente", _showFlags);

            List<PlannedAsset> plan = BuildPlan(_selectedType, _entityName);
            List<string> flags = GetFlags(_selectedType, _entityName);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Carpeta base:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  {GetChapterRoot()}", EditorStyles.miniLabel);

            EditorGUILayout.Space(8);
            GUILayout.Label("Se creará:", EditorStyles.boldLabel);
            foreach (PlannedAsset asset in plan)
                EditorGUILayout.LabelField($"  → {asset.Category}/{asset.FileName}.asset");

            if (_showFlags)
                foreach (string flag in flags)
                    EditorGUILayout.LabelField($"  → [Flag manual] {flag}");

            EditorGUILayout.Space(10);

            if (string.IsNullOrEmpty(_entityName))
            {
                EditorGUILayout.HelpBox("Escribe un nombre antes de continuar.", MessageType.Warning);
            }
            else if (GUILayout.Button(
                $"✅ Crear {_selectedType}_{_entityName} en {EditorToolsUtility.NormalizeChapter(_chapter)}",
                GUILayout.Height(35)))
            {
                CreateContent(plan, flags);
            }

            if (_log.Count == 0) return;

            EditorGUILayout.Space(10);
            GUILayout.Label("Resultado:", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
            foreach (string entry in _log)
                EditorGUILayout.LabelField($"  {entry}");
            EditorGUILayout.EndScrollView();
        }

        private string GetChapterRoot()
            => $"{EditorToolsUtility.ContentRoot}/{EditorToolsUtility.NormalizeChapter(_chapter)}";

        private static List<PlannedAsset> BuildPlan(ContentType type, string name)
        {
            List<PlannedAsset> plan = new List<PlannedAsset>();
            if (string.IsNullOrEmpty(name)) return plan;

            switch (type)
            {
                case ContentType.NPC:
                    string dialogueID = $"DIAL_{name}_INTRO";
                    plan.Add(Plan<NPCData>("NPCs", $"NPCData_{name}", so => ((NPCData)so).NPCID = name));
                    plan.Add(Plan<DialogueData>("Dialogues", $"DialogueData_{dialogueID}",
                        so => ((DialogueData)so).DialogueID = dialogueID));
                    break;

                case ContentType.Boss:
                    plan.Add(Plan<BossData>("Bosses", $"BossData_{name}", so => ((BossData)so).BossID = name));
                    break;

                case ContentType.Quest:
                    plan.Add(Plan<QuestData>("Quests", $"QuestData_{name}", so => ((QuestData)so).QuestID = name));
                    break;

                case ContentType.Enemy:
                    plan.Add(Plan<EnemyData>("Enemies", $"EnemyData_{name}", so => ((EnemyData)so).EnemyID = name));
                    break;

                case ContentType.Item:
                    plan.Add(Plan<ItemData>("Items", $"ItemData_{name}", so => ((ItemData)so).ItemID = name));
                    break;

                case ContentType.Cutscene:
                    plan.Add(Plan<CutsceneData>("Cutscenes", $"CutsceneData_{name}",
                        so => ((CutsceneData)so).CutsceneID = name));
                    break;

                case ContentType.Minigame:
                    plan.Add(Plan<MinigameData>("Minigames", $"MinigameData_{name}",
                        so => ((MinigameData)so).MinigameID = name));
                    break;
            }

            return plan;
        }

        private static PlannedAsset Plan<T>(string category, string fileName, System.Action<ScriptableObject> initialize)
            where T : ScriptableObject
        {
            return new PlannedAsset
            {
                AssetType = typeof(T),
                Category = category,
                FileName = fileName,
                Initialize = initialize
            };
        }

        private static List<string> GetFlags(ContentType type, string name)
        {
            List<string> flags = new List<string>();
            if (string.IsNullOrEmpty(name)) return flags;

            string[] templates = type switch
            {
                ContentType.NPC => new[]
                {
                    FlagConventions.NpcMet, FlagConventions.NpcHelped, FlagConventions.NpcTrustLevel1
                },
                ContentType.Boss => new[]
                {
                    FlagConventions.BossDiscovered, FlagConventions.BossStarted, FlagConventions.BossDefeated,
                    FlagConventions.BossRewardObtained, FlagConventions.BossWorldUpdated
                },
                ContentType.Quest => new[] { FlagConventions.QuestStarted, FlagConventions.QuestCompleted },
                ContentType.Item => new[] { FlagConventions.ItemFound },
                ContentType.Cutscene => new[] { FlagConventions.CutscenePlayed },
                _ => new string[0]
            };

            foreach (string template in templates)
                flags.Add(FlagConventions.Format(template, name));

            return flags;
        }

        private void CreateContent(List<PlannedAsset> plan, List<string> flags)
        {
            _log.Clear();
            _createdCount = 0;

            string chapterRoot = GetChapterRoot();

            foreach (PlannedAsset planned in plan)
                CreateAsset(planned, $"{chapterRoot}/{planned.Category}");

            if (_showFlags)
                foreach (string flag in flags)
                {
                    _log.Add($"📌 Añade a FlagDatabase: {flag}");
                    Debug.Log($"[ContentCreator] Añade esta flag a su FlagDatabase: {flag}");
                }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Contenido creado",
                $"Se han creado {_createdCount} archivos para {_selectedType}_{_entityName} en " +
                $"{EditorToolsUtility.NormalizeChapter(_chapter)}.\n\n" +
                "Recuerda añadir las flags indicadas (📌) a su FlagDatabase.",
                "OK");
        }

        private void CreateAsset(PlannedAsset planned, string folder)
        {
            if (!EditorToolsUtility.EnsureFolderPath(folder, _log))
            {
                _log.Add($"❌ Ruta no válida: {folder}");
                return;
            }

            string path = $"{folder}/{planned.FileName}.asset";

            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null)
            {
                _log.Add($"⚠️ Ya existe: {planned.FileName}.asset (omitido)");
                return;
            }

            ScriptableObject asset = ScriptableObject.CreateInstance(planned.AssetType);
            planned.Initialize(asset);
            AssetDatabase.CreateAsset(asset, path);

            _createdCount++;
            _log.Add($"✅ {planned.FileName}.asset");
        }
    }
}
