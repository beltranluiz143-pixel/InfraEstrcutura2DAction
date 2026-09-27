using System;
using System.Collections.Generic;

namespace Infra2DAction
{
    [Serializable]
    public class QuestSaveData
    {
        [Serializable]
        public class ActiveQuestProgress
        {
            public string QuestID;
            public List<int> ObjectiveProgress = new List<int>();
        }

        public List<ActiveQuestProgress> ActiveQuests = new List<ActiveQuestProgress>();
        public List<string> CompletedQuestIDs = new List<string>();
    }
}
