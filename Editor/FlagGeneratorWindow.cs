using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class FlagGeneratorWindow : EditorWindow
    {
        public enum EntityType
        {
            NPC, Boss, Quest, Item, Door, Ability,
            WorldEvent, Tutorial, Party, Story, Cutscene,
            Collectible, Relationship, Achievement, Shop,
            Dialogue, SceneTrigger, GameplaySystem, Enemy
        }

        private static readonly Dictionary<EntityType, string[]> Templates = new Dictionary<EntityType, string[]>
        {
            { EntityType.NPC, new[]
                {
                    "NPC_{0}_DISCOVERED", FlagConventions.NpcMet, "NPC_{0}_INTRO_DIALOGUE", "NPC_{0}_FIRST_INTERACTION",
                    "NPC_{0}_QUEST_AVAILABLE", "NPC_{0}_QUEST_COMPLETED", FlagConventions.NpcHelped,
                    "NPC_{0}_RECRUITABLE", "NPC_{0}_RECRUITED", FlagConventions.NpcTrustLevel1,
                    "NPC_{0}_TRUST_LEVEL_2", "NPC_{0}_TRUST_LEVEL_3"
                } },
            { EntityType.Boss, new[]
                {
                    FlagConventions.BossDiscovered, FlagConventions.BossStarted, "BOSS_{0}_PHASE2",
                    FlagConventions.BossDefeated, FlagConventions.BossRewardObtained, FlagConventions.BossWorldUpdated
                } },
            { EntityType.Quest, new[]
                {
                    "QUEST_{0}_AVAILABLE", FlagConventions.QuestStarted, "QUEST_{0}_PROGRESS",
                    FlagConventions.QuestCompleted, "QUEST_{0}_FAILED"
                } },
            { EntityType.Item, new[] { FlagConventions.ItemFound, "ITEM_{0}_USED" } },
            { EntityType.Door, new[] { "DOOR_{0}_OPEN", "ACCESS_{0}_UNLOCKED" } },
            { EntityType.Ability, new[] { "ABILITY_{0}_UNLOCKED", "ACCESS_{0}_UNLOCKED" } },
            { EntityType.WorldEvent, new[] { "WORLD_EVENT_{0}_TRIGGERED", "WORLD_EVENT_{0}_COMPLETED" } },
            { EntityType.Tutorial, new[] { "TUTORIAL_{0}_COMPLETED" } },
            { EntityType.Party, new[] { "PARTY_{0}_JOINED" } },
            { EntityType.Story, new[] { "STORY_{0}_STARTED", "STORY_{0}_COMPLETED" } },
            { EntityType.Cutscene, new[] { FlagConventions.CutscenePlayed } },
            { EntityType.Collectible, new[] { "COLLECTIBLE_{0}_FOUND" } },
            { EntityType.Relationship, new[]
                {
                    "REL_{0}_KNOWN", "REL_{0}_FRIENDLY", "REL_{0}_TRUST_LEVEL_1", "REL_{0}_TRUST_LEVEL_2",
                    "REL_{0}_TRUST_LEVEL_3", "REL_{0}_BEST_FRIEND", "REL_{0}_ENDING_REFERENCE"
                } },
            { EntityType.Achievement, new[] { "ACHIEVEMENT_{0}_UNLOCKED" } },
            { EntityType.Shop, new[] { "SHOP_{0}_UNLOCKED", "SHOP_{0}_EXPANDED" } },
            { EntityType.Dialogue, new[] { "DIAL_{0}_PLAYED", "DIAL_{0}_CHOICE_A", "DIAL_{0}_CHOICE_B" } },
            { EntityType.SceneTrigger, new[] { "TRIGGER_{0}_ACTIVATED", "TRIGGER_{0}_COMPLETED" } },
            { EntityType.GameplaySystem, new[] { "SYSTEM_{0}_UNLOCKED", "SYSTEM_{0}_TUTORIAL_DONE" } },
            { EntityType.Enemy, new[] { "ENEMY_{0}_ENCOUNTERED", "ENEMY_{0}_DEFEATED", "ENEMY_{0}_PACIFIED" } }
        };

        private EntityType _selectedType = EntityType.NPC;
        private string _entityName = "";
        private Vector2 _scrollPos;
        private List<string> _previewFlags = new List<string>();

        [MenuItem(EditorToolsUtility.MenuRoot + "Flag Generator", false, 100)]
        public static void OpenWindow()
        {
            FlagGeneratorWindow window = GetWindow<FlagGeneratorWindow>("Flag Generator");
            window.minSize = new Vector2(420, 520);
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Flag Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Tipo de entidad:");
            _selectedType = (EntityType)EditorGUILayout.EnumPopup(_selectedType);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Nombre (SCREAMING_SNAKE_CASE):");
            _entityName = FlagConventions.ToScreamingSnake(EditorGUILayout.TextField(_entityName));

            EditorGUILayout.Space(10);

            if (GUILayout.Button("🔍 Previsualizar flags"))
                _previewFlags = GenerateFlags(_selectedType, _entityName);

            if (_previewFlags.Count == 0) return;

            EditorGUILayout.Space(5);
            GUILayout.Label("Flags que se generarán:", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(220));
            foreach (string flag in _previewFlags)
                EditorGUILayout.SelectableLabel(flag, GUILayout.Height(18));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            if (string.IsNullOrEmpty(_entityName))
            {
                EditorGUILayout.HelpBox("Escribe un nombre antes de generar.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button($"✅ Generar {_previewFlags.Count} flags para {_selectedType}_{_entityName}", GUILayout.Height(32)))
                GenerateAndLog(_selectedType, _entityName);
        }

        private static List<string> GenerateFlags(EntityType type, string name)
        {
            List<string> flags = new List<string>();
            if (string.IsNullOrEmpty(name)) return flags;

            if (Templates.TryGetValue(type, out string[] templates))
                foreach (string template in templates)
                    flags.Add(FlagConventions.Format(template, name));

            return flags;
        }

        private static void GenerateAndLog(EntityType type, string name)
        {
            List<string> flags = GenerateFlags(type, name);

            Debug.Log($"[FlagGenerator] ===== Flags para {type}_{name} =====");
            foreach (string flag in flags)
                Debug.Log($"  → {flag}");
            Debug.Log($"[FlagGenerator] Añade estas {flags.Count} flags a la FlagDatabase que usa el GlobalVariablesSystem.");

            EditorUtility.DisplayDialog(
                "Flags generadas",
                $"Se han generado {flags.Count} flags para {type}_{name}.\n\n" +
                "Revisa la consola para ver la lista completa y añádelas a su FlagDatabase.",
                "OK");
        }
    }
}
