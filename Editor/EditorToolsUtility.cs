using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    internal static class FlagConventions
    {
        public const string NpcMet = "NPC_{0}_MET";
        public const string NpcHelped = "NPC_{0}_HELPED";
        public const string NpcTrustLevel1 = "NPC_{0}_TRUST_LEVEL_1";
        public const string BossDiscovered = "BOSS_{0}_DISCOVERED";
        public const string BossStarted = "BOSS_{0}_STARTED";
        public const string BossDefeated = "BOSS_{0}_DEFEATED";
        public const string BossRewardObtained = "BOSS_{0}_REWARD_OBTAINED";
        public const string BossWorldUpdated = "BOSS_{0}_WORLD_UPDATED";
        public const string QuestStarted = "QUEST_{0}_STARTED";
        public const string QuestCompleted = "QUEST_{0}_COMPLETED";
        public const string ItemFound = "ITEM_{0}_FOUND";
        public const string CutscenePlayed = "CUTSCENE_{0}_PLAYED";

        public static string Format(string template, string id) => string.Format(template, id);

        public static string ToScreamingSnake(string value)
            => (value ?? "").Trim().ToUpperInvariant().Replace(' ', '_');

        public static bool IsScreamingSnake(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            foreach (char c in value)
            {
                bool valid = (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
                if (!valid) return false;
            }

            return true;
        }
    }

    internal static class EditorToolsUtility
    {
        public const string MenuRoot = "Infraestructura2DAction/";
        public const string WindowTitlePrefix = "Infraestructura2DAction — ";

        private const string DefaultContentRoot = "Assets/Content";
        private const string DefaultScenesRoot = "Assets/Scenes";
        private const string DefaultChapter = "Chapter_01";
        private const string SharedChapter = "Shared";

        public static string ContentRoot
        {
            get => GetPref("ContentRoot", DefaultContentRoot);
            set => SetPref("ContentRoot", value);
        }

        public static string ScenesRoot
        {
            get => GetPref("ScenesRoot", DefaultScenesRoot);
            set => SetPref("ScenesRoot", value);
        }

        public static string LastChapter
        {
            get => GetPref("LastChapter", DefaultChapter);
            set => SetPref("LastChapter", value);
        }

        public static string NormalizeChapter(string chapter)
        {
            string trimmed = (chapter ?? "").Trim().Replace(' ', '_');
            return string.IsNullOrEmpty(trimmed) ? SharedChapter : trimmed;
        }

        public static string[] GetExistingChapters(string root)
        {
            if (!AssetDatabase.IsValidFolder(root)) return new string[0];

            string[] subFolders = AssetDatabase.GetSubFolders(root);
            string[] names = new string[subFolders.Length];

            for (int i = 0; i < subFolders.Length; i++)
                names[i] = System.IO.Path.GetFileName(subFolders[i]);

            return names;
        }

        public static string DrawChapterField(string chapter, string root)
        {
            chapter = EditorGUILayout.TextField("Chapter:", chapter);

            string[] existing = GetExistingChapters(root);
            if (existing.Length > 0)
            {
                int picked = EditorGUILayout.Popup("Chapters existentes:", -1, existing);
                if (picked >= 0) chapter = existing[picked];
            }

            return chapter;
        }

        public static void DrawPathSettings()
        {
            ContentRoot = EditorGUILayout.TextField("Carpeta de contenido:", ContentRoot);
            ScenesRoot = EditorGUILayout.TextField("Carpeta de escenas:", ScenesRoot);
        }

        public static bool EnsureFolderPath(string fullPath, List<string> log = null)
        {
            if (string.IsNullOrEmpty(fullPath) || !fullPath.StartsWith("Assets")) return false;
            if (AssetDatabase.IsValidFolder(fullPath)) return true;

            string[] parts = fullPath.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                    if (log != null) log.Add($"📁 Carpeta creada: {next}");
                }

                current = next;
            }

            return true;
        }

        public static List<T> LoadAll<T>() where T : UnityEngine.Object
        {
            List<T> result = new List<T>();

            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) result.Add(asset);
            }

            return result;
        }

        private static string GetPref(string key, string fallback)
            => EditorPrefs.GetString(PrefKey(key), fallback);

        private static void SetPref(string key, string value)
            => EditorPrefs.SetString(PrefKey(key), value);

        private static string PrefKey(string key)
            => $"Infra2DAction.{PlayerSettings.productGUID}.{key}";
    }
}
