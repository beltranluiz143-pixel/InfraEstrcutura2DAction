using System;
using System.IO;
using UnityEngine;

namespace Infra2DAction
{
    public class SaveSystem : MonoBehaviour
    {
        public class SaveSlotSummary
        {
            public int SlotNumber;
            public bool IsEmpty = true;
            public string LastSaveTime = "";
            public float TotalPlaytimeSeconds;
        }

        private const int SLOT_COUNT = 3;
        private const string SAVE_FILE_PREFIX = "save_slot";

        private string _savePath;
        private int _activeSlot = 1;
        private SaveData _currentSave;
        private SettingsSystem _settingsSystem;

        public int ActiveSlot => _activeSlot;

        public void Initialize(SettingsSystem settingsSystem)
        {
            if (settingsSystem == null)
            {
                DebugSystem.LogError("SettingsSystem es null en SaveSystem.", "Save", "SaveSystem");
                return;
            }

            _settingsSystem = settingsSystem;
            _currentSave = new SaveData();

            SelectSlot(1);

            DebugSystem.Log($"SaveSystem initialized. Path: {_savePath}", "Save", "SaveSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SaveRequestedEvent>(OnSaveRequested);
            EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Subscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
            EventBus.Subscribe<CheckpointActivatedEvent>(OnCheckpointActivated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SaveRequestedEvent>(OnSaveRequested);
            EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
            EventBus.Unsubscribe<CheckpointActivatedEvent>(OnCheckpointActivated);
        }

        private void OnSceneTransitionStarted(SceneTransitionStartedEvent e)
        {
            CollectSaveData();
        }

        private void CollectSaveData()
        {
            if (_currentSave == null) return;
            EventBus.Raise(new CollectSaveDataEvent());
        }

        private void Update()
        {
            if (_currentSave != null)
                _currentSave.TotalPlaytimeSeconds += Time.unscaledDeltaTime;
        }

        private void OnSaveRequested(SaveRequestedEvent e)
        {
            SaveGame();
        }

        private void OnSceneLoaded(SceneLoadedEvent e)
        {
            _currentSave.Player.CurrentScene = e.SceneName;
            _currentSave.World.MarkSceneVisited(e.SceneName);
        }

        private void OnCheckpointActivated(CheckpointActivatedEvent e)
        {
            if (_currentSave == null) return;

            _currentSave.Player.LastCheckpointID = e.SpawnPointID;
            SaveGame();

            DebugSystem.Log($"Checkpoint activado: '{e.SpawnPointID}'. Partida guardada.", "Save", "SaveSystem");
        }

        private void SaveGame()
        {
            try
            {
                CollectSaveData();

                _currentSave.Settings = _settingsSystem.GetSaveData();
                _currentSave.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _currentSave.SaveVersion = 1;

                string json = JsonUtility.ToJson(_currentSave, prettyPrint: true);
                File.WriteAllText(_savePath, json);

                EventBus.Raise(new GameSavedEvent());
                DebugSystem.Log("Game saved successfully.", "Save", "SaveSystem");
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Save failed: {ex.Message}", "Save", "SaveSystem");
            }
        }

        private void TryLoadGame()
        {
            if (!File.Exists(_savePath))
            {
                DebugSystem.Log("No save file found. Starting fresh.", "Save", "SaveSystem");
                return;
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                {
                    DebugSystem.LogWarning("Save file corrupted. Starting fresh.", "Save", "SaveSystem");
                    return;
                }

                if (data.SaveVersion != 1)
                {
                    DebugSystem.LogWarning($"Save version mismatch: {data.SaveVersion}", "Save", "SaveSystem");
                }

                _currentSave = data;

                EventBus.Raise(new SaveLoadedEvent(_currentSave));

                DebugSystem.Log($"Game loaded. Last save: {_currentSave.LastSaveTime}", "Save", "SaveSystem");
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Load failed: {ex.Message}", "Save", "SaveSystem");
            }
        }

        public void UpdatePlayerData(PlayerSaveData playerData)
        {
            if (playerData == null) return;
            _currentSave.Player = playerData;
        }

        public void UpdateWorldData(WorldSaveData worldData)
        {
            if (worldData == null) return;
            _currentSave.World = worldData;
        }

        public SaveData GetCurrentSave() => _currentSave;
        public bool HasSaveFile() => File.Exists(_savePath);

        public void DeleteSave()
        {
            if (!File.Exists(_savePath)) return;
            File.Delete(_savePath);
            _currentSave = new SaveData();
            DebugSystem.Log("Save file deleted.", "Save", "SaveSystem");
        }

        private string GetSlotPath(int slotNumber)
            => Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_PREFIX}{slotNumber}.json");

        public void SelectSlot(int slotNumber)
        {
            slotNumber = Mathf.Clamp(slotNumber, 1, SLOT_COUNT);

            _activeSlot = slotNumber;
            _savePath = GetSlotPath(slotNumber);
            _currentSave = new SaveData();

            TryLoadGame();
        }

        public bool IsSlotEmpty(int slotNumber) => !File.Exists(GetSlotPath(slotNumber));

        public SaveSlotSummary GetSlotSummary(int slotNumber)
        {
            var summary = new SaveSlotSummary { SlotNumber = slotNumber };
            string path = GetSlotPath(slotNumber);

            if (!File.Exists(path)) return summary;

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data == null) return summary;

                summary.IsEmpty = false;
                summary.LastSaveTime = data.LastSaveTime;
                summary.TotalPlaytimeSeconds = data.TotalPlaytimeSeconds;
            }
            catch (Exception ex)
            {
                DebugSystem.LogWarning($"No se pudo leer resumen del slot {slotNumber}: {ex.Message}", "Save", "SaveSystem");
            }

            return summary;
        }

        public void DeleteSlot(int slotNumber)
        {
            string path = GetSlotPath(slotNumber);
            if (File.Exists(path)) File.Delete(path);

            if (slotNumber == _activeSlot)
                _currentSave = new SaveData();

            DebugSystem.Log($"Slot {slotNumber} eliminado.", "Save", "SaveSystem");
        }
    }
}
