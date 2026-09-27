using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "QuestData_", menuName = "Infraestructura2DAction/Content/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public enum ObjectiveType { KillEnemy, CollectItem, TalkToNPC, ReachFlag }

        [Serializable]
        public class QuestObjective
        {
            public ObjectiveType Type;
            public string TargetID;
            public int RequiredAmount = 1;
        }

        [Header("Identification")]
        public string QuestID;
        [TextArea] public string DisplayText;

        [Header("Requirements (opcional)")]
        public string RequiredCompletedQuestID;
        [Tooltip("Si se especifica, la quest solo esta disponible cuando esta es la ruta dominante.")]
        public string RequiredRoute;
        [Tooltip("Condicion de flags/variables que debe cumplirse para poder iniciar la quest.")]
        public FlagCondition StartCondition;

        [Header("Objectives")]
        public List<QuestObjective> Objectives = new List<QuestObjective>();

        [Header("Reward (opcional)")]
        public string RewardItemID;
        public int RewardCurrency;
    }
}
