namespace Infra2DAction
{
    public class QuestStartRequestEvent : BaseEvent
    {
        public QuestData Data { get; private set; }

        public QuestStartRequestEvent(QuestData data, string sourceID = "Unknown")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class QuestCompleteRequestEvent : BaseEvent
    {
        public QuestData Data { get; private set; }

        public QuestCompleteRequestEvent(QuestData data, string sourceID = "Unknown")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class QuestResetRequestEvent : BaseEvent
    {
        public QuestData Data { get; private set; }

        public QuestResetRequestEvent(QuestData data, string sourceID = "Unknown")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class QuestStartedEvent : BaseEvent
    {
        public string QuestID { get; private set; }
        public string DisplayText { get; private set; }

        public QuestStartedEvent(string questID, string displayText, string sourceID = "QuestSystem")
            : base(sourceID)
        {
            QuestID = questID;
            DisplayText = displayText;
        }
    }

    public class QuestObjectiveProgressEvent : BaseEvent
    {
        public string QuestID { get; private set; }
        public int ObjectiveIndex { get; private set; }
        public int CurrentAmount { get; private set; }
        public int RequiredAmount { get; private set; }

        public QuestObjectiveProgressEvent(string questID, int objectiveIndex, int current, int required,
                                           string sourceID = "QuestSystem")
            : base(sourceID)
        {
            QuestID = questID;
            ObjectiveIndex = objectiveIndex;
            CurrentAmount = current;
            RequiredAmount = required;
        }
    }

    public class QuestCompletedEvent : BaseEvent
    {
        public string QuestID { get; private set; }
        public string DisplayText { get; private set; }

        public QuestCompletedEvent(string questID, string displayText = "", string sourceID = "QuestSystem")
            : base(sourceID)
        {
            QuestID = questID;
            DisplayText = string.IsNullOrEmpty(displayText) ? questID : displayText;
        }
    }
}
