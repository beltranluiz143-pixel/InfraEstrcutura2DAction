using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class QuestSystem : MonoBehaviour
    {
        public class QuestLogEntry
        {
            public QuestData Data;
            public bool Completed;
        }

        [Header("Database")]
        [SerializeField] private QuestDatabase _questDatabase;

        private readonly Dictionary<string, List<int>> _questProgress = new Dictionary<string, List<int>>();
        private readonly Dictionary<string, QuestData> _activeQuestData = new Dictionary<string, QuestData>();
        private readonly HashSet<string> _completedQuestIDs = new HashSet<string>();

        private GlobalVariablesSystem _globalVars;
        private SaveSystem _saveSystem;
        private RouteSystem _routeSystem;

        public void Initialize(GlobalVariablesSystem globalVars, SaveSystem saveSystem, RouteSystem routeSystem)
        {
            _globalVars = globalVars;
            _saveSystem = saveSystem;
            _routeSystem = routeSystem;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<QuestStartRequestEvent>(OnQuestStartRequested);
            EventBus.Subscribe<QuestCompleteRequestEvent>(OnQuestCompleteRequested);
            EventBus.Subscribe<QuestResetRequestEvent>(OnQuestResetRequested);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            EventBus.Subscribe<NpcTalkedEvent>(OnNpcTalked);
            EventBus.Subscribe<GlobalFlagChangedEvent>(OnFlagChanged);
            EventBus.Subscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<QuestStartRequestEvent>(OnQuestStartRequested);
            EventBus.Unsubscribe<QuestCompleteRequestEvent>(OnQuestCompleteRequested);
            EventBus.Unsubscribe<QuestResetRequestEvent>(OnQuestResetRequested);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
            EventBus.Unsubscribe<NpcTalkedEvent>(OnNpcTalked);
            EventBus.Unsubscribe<GlobalFlagChangedEvent>(OnFlagChanged);
            EventBus.Unsubscribe<CollectSaveDataEvent>(OnCollectSaveData);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        private void OnQuestStartRequested(QuestStartRequestEvent e)
        {
            if (e.Data == null) return;

            string questID = e.Data.QuestID;
            if (_activeQuestData.ContainsKey(questID) || _completedQuestIDs.Contains(questID)) return;

            if (!string.IsNullOrEmpty(e.Data.RequiredRoute) && _routeSystem != null &&
                _routeSystem.GetDominantRoute() != e.Data.RequiredRoute)
            {
                DebugSystem.LogWarning($"Quest {questID} no disponible: ruta incorrecta.", "Quest", "QuestSystem");
                return;
            }

            if (e.Data.StartCondition != null &&
                (_globalVars == null || !_globalVars.EvaluateCondition(e.Data.StartCondition)))
            {
                DebugSystem.LogWarning($"Quest {questID} no disponible: condicion de inicio no cumplida.", "Quest", "QuestSystem");
                return;
            }

            if (!string.IsNullOrEmpty(e.Data.RequiredCompletedQuestID) &&
                !_completedQuestIDs.Contains(e.Data.RequiredCompletedQuestID))
            {
                DebugSystem.LogWarning($"Quest {questID} bloqueada por dependencia.", "Quest", "QuestSystem");
                return;
            }

            ActivateQuest(e.Data, null);

            _globalVars?.SetFlag($"QUEST_{questID}_STARTED", true);

            EventBus.Raise(new QuestStartedEvent(questID, e.Data.DisplayText));
        }

        private void OnQuestCompleteRequested(QuestCompleteRequestEvent e)
        {
            if (e.Data == null || _completedQuestIDs.Contains(e.Data.QuestID)) return;

            string questID = e.Data.QuestID;

            if (!_activeQuestData.ContainsKey(questID))
            {
                ActivateQuest(e.Data, null);

                if (_globalVars != null) _globalVars.SetFlag($"QUEST_{questID}_STARTED", true);

                EventBus.Raise(new QuestStartedEvent(questID, e.Data.DisplayText));
            }

            CompleteQuest(questID, e.Data);
        }

        private void OnQuestResetRequested(QuestResetRequestEvent e)
        {
            if (e.Data == null) return;

            string questID = e.Data.QuestID;

            _activeQuestData.Remove(questID);
            _questProgress.Remove(questID);
            _completedQuestIDs.Remove(questID);

            if (_globalVars != null)
            {
                _globalVars.SetFlag($"QUEST_{questID}_STARTED", false);
                _globalVars.SetFlag($"QUEST_{questID}_COMPLETED", false);
            }

            DebugSystem.Log($"Quest {questID} reseteada.", "Quest", "QuestSystem");
        }

        private void ActivateQuest(QuestData quest, List<int> savedProgress)
        {
            _activeQuestData[quest.QuestID] = quest;

            List<int> progress = new List<int>();
            for (int i = 0; i < quest.Objectives.Count; i++)
                progress.Add(savedProgress != null && i < savedProgress.Count ? savedProgress[i] : 0);

            _questProgress[quest.QuestID] = progress;
        }

        private void OnEnemyKilled(EnemyKilledEvent e)
            => CheckObjectiveProgress(QuestData.ObjectiveType.KillEnemy, e.EnemyID);

        private void OnItemCollected(ItemCollectedEvent e)
            => CheckObjectiveProgress(QuestData.ObjectiveType.CollectItem, e.ItemID);

        private void OnNpcTalked(NpcTalkedEvent e)
            => CheckObjectiveProgress(QuestData.ObjectiveType.TalkToNPC, e.NPCID);

        private void OnFlagChanged(GlobalFlagChangedEvent e)
        {
            if (!e.Value) return;
            CheckObjectiveProgress(QuestData.ObjectiveType.ReachFlag, e.FlagKey);
        }

        private void CheckObjectiveProgress(QuestData.ObjectiveType type, string targetID)
        {
            var questIDs = new List<string>(_activeQuestData.Keys);

            foreach (string questID in questIDs)
            {
                QuestData quest = _activeQuestData[questID];
                List<int> progress = _questProgress[questID];

                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    QuestData.QuestObjective objective = quest.Objectives[i];

                    if (objective.Type != type || objective.TargetID != targetID) continue;
                    if (progress[i] >= objective.RequiredAmount) continue;

                    progress[i]++;

                    EventBus.Raise(new QuestObjectiveProgressEvent(questID, i, progress[i], objective.RequiredAmount));
                }

                if (AreAllObjectivesComplete(quest, progress))
                    CompleteQuest(questID, quest);
            }
        }

        private bool AreAllObjectivesComplete(QuestData quest, List<int> progress)
        {
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                if (progress[i] < quest.Objectives[i].RequiredAmount)
                    return false;
            }
            return true;
        }

        private void CompleteQuest(string questID, QuestData quest)
        {
            _activeQuestData.Remove(questID);
            _questProgress.Remove(questID);
            _completedQuestIDs.Add(questID);

            _globalVars?.SetFlag($"QUEST_{questID}_COMPLETED", true);

            if (!string.IsNullOrEmpty(quest.RewardItemID))
                EventBus.Raise(new ItemCollectedEvent(quest.RewardItemID, "QuestSystem"));

            if (quest.RewardCurrency > 0)
                EventBus.Raise(new CurrencyAddRequestEvent(quest.RewardCurrency, "QuestSystem"));

            EventBus.Raise(new QuestCompletedEvent(questID, quest.DisplayText));
        }

        public bool IsQuestActive(string questID) => _activeQuestData.ContainsKey(questID);
        public bool IsQuestCompleted(string questID) => _completedQuestIDs.Contains(questID);

        public List<QuestLogEntry> GetQuestLog()
        {
            List<QuestLogEntry> log = new List<QuestLogEntry>();

            foreach (QuestData quest in _activeQuestData.Values)
                log.Add(new QuestLogEntry { Data = quest, Completed = false });

            foreach (string questID in _completedQuestIDs)
            {
                QuestData quest = _questDatabase != null ? _questDatabase.GetQuest(questID) : null;
                if (quest != null) log.Add(new QuestLogEntry { Data = quest, Completed = true });
            }

            return log;
        }

        private void OnCollectSaveData(CollectSaveDataEvent e)
        {
            if (_saveSystem == null) return;

            QuestSaveData saveData = new QuestSaveData();

            foreach (var kvp in _questProgress)
            {
                saveData.ActiveQuests.Add(new QuestSaveData.ActiveQuestProgress
                {
                    QuestID = kvp.Key,
                    ObjectiveProgress = new List<int>(kvp.Value)
                });
            }

            saveData.CompletedQuestIDs = new List<string>(_completedQuestIDs);

            _saveSystem.GetCurrentSave().Quests = saveData;
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            if (e.Data?.Quests == null) return;

            _completedQuestIDs.Clear();
            _activeQuestData.Clear();
            _questProgress.Clear();

            foreach (string id in e.Data.Quests.CompletedQuestIDs)
                _completedQuestIDs.Add(id);

            foreach (QuestSaveData.ActiveQuestProgress saved in e.Data.Quests.ActiveQuests)
            {
                QuestData quest = _questDatabase != null ? _questDatabase.GetQuest(saved.QuestID) : null;
                if (quest == null)
                {
                    DebugSystem.LogWarning($"Quest activa guardada no encontrada en la QuestDatabase: {saved.QuestID}",
                                           "Quest", "QuestSystem");
                    continue;
                }

                ActivateQuest(quest, saved.ObjectiveProgress);
            }

            DebugSystem.Log($"Quest progress loaded: {_completedQuestIDs.Count} completadas, {_activeQuestData.Count} activas.",
                            "Quest", "QuestSystem");
        }
    }
}
