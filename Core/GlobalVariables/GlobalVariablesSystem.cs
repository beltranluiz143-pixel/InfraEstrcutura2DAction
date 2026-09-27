using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [System.Serializable]
    public class ChapterFlagGroup
    {
        [Header("Nombre del grupo (solo informativo)")]
        public string GroupName = "Chapter1";

        [Header("Flags por categoria")]
        public List<FlagDatabase> Story = new List<FlagDatabase>();
        public List<FlagDatabase> Cutscene = new List<FlagDatabase>();
        public List<FlagDatabase> Boss = new List<FlagDatabase>();
        public List<FlagDatabase> Quest = new List<FlagDatabase>();
        public List<FlagDatabase> NPC = new List<FlagDatabase>();
        public List<FlagDatabase> Relationship = new List<FlagDatabase>();
        public List<FlagDatabase> World = new List<FlagDatabase>();
        public List<FlagDatabase> Puzzle = new List<FlagDatabase>();
        public List<FlagDatabase> Collectible = new List<FlagDatabase>();
        public List<FlagDatabase> Ability = new List<FlagDatabase>();
        public List<FlagDatabase> Persistent = new List<FlagDatabase>();

        public IEnumerable<FlagDatabase> GetAll()
        {
            foreach (var db in Story) if (db != null) yield return db;
            foreach (var db in Cutscene) if (db != null) yield return db;
            foreach (var db in Boss) if (db != null) yield return db;
            foreach (var db in Quest) if (db != null) yield return db;
            foreach (var db in NPC) if (db != null) yield return db;
            foreach (var db in Relationship) if (db != null) yield return db;
            foreach (var db in World) if (db != null) yield return db;
            foreach (var db in Puzzle) if (db != null) yield return db;
            foreach (var db in Collectible) if (db != null) yield return db;
            foreach (var db in Ability) if (db != null) yield return db;
            foreach (var db in Persistent) if (db != null) yield return db;
        }
    }

    public class GlobalVariablesSystem : MonoBehaviour
    {
        [Header("Flags Globales (permanentes, todos los capitulos)")]
        [SerializeField] private List<FlagDatabase> _globalFlagDatabases = new List<FlagDatabase>();

        [Header("Flags por capitulo y categoria")]
        [SerializeField] private List<ChapterFlagGroup> _chapterGroups = new List<ChapterFlagGroup>();

        [Header("Variables Numericas")]
        [SerializeField] private List<VariableDatabase> _variableDatabases = new List<VariableDatabase>();

        private Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        private Dictionary<string, float> _variables = new Dictionary<string, float>();
        private SaveSystem _saveSystem;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            InitializeFlags();
            InitializeVariables();
            DebugSystem.Log($"GlobalVariablesSystem initialized. " +
                            $"Flags: {_flags.Count} | Variables: {_variables.Count}",
                            "GlobalVars", "GlobalVariablesSystem");
        }

        private void InitializeFlags()
        {
            foreach (FlagDatabase db in _globalFlagDatabases)
            {
                if (db == null) continue;
                db.Initialize();
                foreach (var (key, defaultValue) in db.GetAllFlags())
                    if (!_flags.ContainsKey(key))
                        _flags[key] = defaultValue;
            }

            foreach (ChapterFlagGroup group in _chapterGroups)
            {
                if (group == null) continue;

                foreach (FlagDatabase db in group.GetAll())
                {
                    db.Initialize();
                    foreach (var (key, defaultValue) in db.GetAllFlags())
                        if (!_flags.ContainsKey(key))
                            _flags[key] = defaultValue;
                }
            }
        }

        private void InitializeVariables()
        {
            foreach (VariableDatabase database in _variableDatabases)
            {
                if (database == null) continue;
                foreach (var (key, defaultValue) in database.GetAllVariables())
                    if (!_variables.ContainsKey(key))
                        _variables[key] = defaultValue;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Subscribe<FlagQueryEvent>(OnFlagQuery);
            EventBus.Subscribe<FlagSetRequestEvent>(OnFlagSetRequest);
            EventBus.Subscribe<VariableQueryEvent>(OnVariableQuery);
            EventBus.Subscribe<ConditionQueryEvent>(OnConditionQuery);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
            EventBus.Unsubscribe<FlagQueryEvent>(OnFlagQuery);
            EventBus.Unsubscribe<FlagSetRequestEvent>(OnFlagSetRequest);
            EventBus.Unsubscribe<VariableQueryEvent>(OnVariableQuery);
            EventBus.Unsubscribe<ConditionQueryEvent>(OnConditionQuery);
        }

        private void OnFlagQuery(FlagQueryEvent e) => e.SetResult(HasFlag(e.Key) && GetFlag(e.Key));
        private void OnFlagSetRequest(FlagSetRequestEvent e) => SetFlag(e.Key, e.Value);
        private void OnVariableQuery(VariableQueryEvent e) => e.SetResult(GetVariable(e.Key));
        private void OnConditionQuery(ConditionQueryEvent e) => e.SetResult(EvaluateCondition(e.Condition));

        public bool HasFlag(string key) => _flags.ContainsKey(key);

        public bool HasVariable(string key) => _variables.ContainsKey(key);

        public bool GetFlag(string key, bool defaultValue = false)
        {
            if (_flags.TryGetValue(key, out bool value)) return value;
            DebugSystem.LogWarning($"Flag no registrada consultada: {key}", "GlobalVars", "GlobalVariablesSystem");
            return defaultValue;
        }

        public void SetFlag(string key, bool value)
        {
            _flags[key] = value;
            EventBus.Raise(new GlobalFlagChangedEvent(key, value));
            DebugSystem.Log($"Flag: {key} = {value}", "GlobalVars", "GlobalVariablesSystem");
        }

        public float GetVariable(string key, float defaultValue = 0f)
        {
            if (_variables.TryGetValue(key, out float value)) return value;
            DebugSystem.LogWarning($"Variable no registrada consultada: {key}", "GlobalVars", "GlobalVariablesSystem");
            return defaultValue;
        }

        public void SetVariable(string key, float value)
        {
            _variables[key] = value;
            EventBus.Raise(new GlobalVariableChangedEvent(key, value));
        }

        public void IncrementVariable(string key, float amount = 1f)
        {
            if (!_variables.ContainsKey(key)) _variables[key] = 0f;
            _variables[key] += amount;
            EventBus.Raise(new GlobalVariableChangedEvent(key, _variables[key]));
        }

        public bool EvaluateCondition(FlagCondition condition)
        {
            if (condition == null) return true;
            return condition.Type switch
            {
                FlagCondition.ConditionType.FlagIsTrue => GetFlag(condition.Key),
                FlagCondition.ConditionType.FlagIsFalse => !GetFlag(condition.Key),
                FlagCondition.ConditionType.VariableGreaterThan => GetVariable(condition.Key) > condition.CompareValue,
                FlagCondition.ConditionType.VariableLessThan => GetVariable(condition.Key) < condition.CompareValue,
                FlagCondition.ConditionType.VariableEquals => Mathf.Approximately(GetVariable(condition.Key), condition.CompareValue),
                _ => false
            };
        }

        public bool EvaluateConditions(List<FlagCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;
            foreach (FlagCondition c in conditions)
                if (!EvaluateCondition(c)) return false;
            return true;
        }

        private void OnCollectSaveData(CollectSaveDataEvent e)
        {
            _saveSystem?.UpdateWorldData(BuildWorldSaveData());
            DebugSystem.Log("WorldData updated for save.", "GlobalVars", "GlobalVariablesSystem");
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            if (e.Data?.World == null)
            {
                DebugSystem.LogWarning("SaveLoadedEvent sin WorldData.", "GlobalVars", "GlobalVariablesSystem");
                return;
            }
            RestoreFromWorldSaveData(e.Data.World);
            DebugSystem.Log("WorldData restored from save.", "GlobalVars", "GlobalVariablesSystem");
        }

        private WorldSaveData BuildWorldSaveData()
        {
            WorldSaveData existing = _saveSystem != null ? _saveSystem.GetCurrentSave()?.World : null;
            WorldSaveData data = existing ?? new WorldSaveData();

            data.GlobalFlags.Clear();
            data.GameVariables.Clear();

            foreach (var kvp in _flags) data.SetFlag(kvp.Key, kvp.Value);
            foreach (var kvp in _variables) data.SetVariable(kvp.Key, kvp.Value);

            return data;
        }

        private void RestoreFromWorldSaveData(WorldSaveData data)
        {
            foreach (var entry in data.GlobalFlags)
            {
                _flags[entry.Key] = entry.Value;
                EventBus.Raise(new GlobalFlagChangedEvent(entry.Key, entry.Value));
            }
            foreach (var entry in data.GameVariables)
            {
                _variables[entry.Key] = entry.Value;
                EventBus.Raise(new GlobalVariableChangedEvent(entry.Key, entry.Value));
            }
        }

        public void PrintStatus()
        {
            DebugSystem.Log("===== FLAGS =====", "GlobalVars", "GlobalVariablesSystem");
            foreach (var kvp in _flags)
                DebugSystem.Log($"  {kvp.Key}: {kvp.Value}", "GlobalVars", "GlobalVariablesSystem");
            DebugSystem.Log("===== VARIABLES =====", "GlobalVars", "GlobalVariablesSystem");
            foreach (var kvp in _variables)
                DebugSystem.Log($"  {kvp.Key}: {kvp.Value}", "GlobalVars", "GlobalVariablesSystem");
        }

        public void ResetFlagGroup(string groupName)
        {
            ChapterFlagGroup group = _chapterGroups.Find(g => g.GroupName == groupName);
            if (group == null)
            {
                DebugSystem.LogWarning($"No hay ChapterFlagGroup llamado '{groupName}'.", "GlobalVars", "GlobalVariablesSystem");
                return;
            }

            void ResetList(List<FlagDatabase> list)
            {
                foreach (FlagDatabase db in list)
                {
                    if (db == null) continue;
                    foreach (var (key, defaultValue) in db.GetAllFlags())
                    {
                        _flags[key] = defaultValue;
                        EventBus.Raise(new GlobalFlagChangedEvent(key, defaultValue));
                    }
                }
            }

            ResetList(group.Story);
            ResetList(group.Cutscene);
            ResetList(group.Boss);
            ResetList(group.Quest);
            ResetList(group.NPC);
            ResetList(group.Relationship);
            ResetList(group.World);
            ResetList(group.Puzzle);
            ResetList(group.Collectible);
            ResetList(group.Ability);

            DebugSystem.Log($"Flags del grupo '{groupName}' reseteadas.", "GlobalVars", "GlobalVariablesSystem");
        }

#if UNITY_EDITOR
        public Dictionary<string, bool> GetAllFlagsForEditor()
            => new Dictionary<string, bool>(_flags);

        public Dictionary<string, float> GetAllVariablesForEditor()
            => new Dictionary<string, float>(_variables);

        public void SetFlagFromEditor(string key, bool value)
        {
            SetFlag(key, value);
            DebugSystem.Log($"[EDITOR] Flag forzada: {key} = {value}", "Editor", "WorldStateInspector");
        }

        public void SetVariableFromEditor(string key, float value)
        {
            SetVariable(key, value);
            DebugSystem.Log($"[EDITOR] Variable forzada: {key} = {value}", "Editor", "WorldStateInspector");
        }
#endif
    }
}
