using UnityEngine;

namespace Infra2DAction
{
    public class NPCSystem : MonoBehaviour
    {
        private GlobalVariablesSystem _globalVars;

        public void Initialize(GlobalVariablesSystem globalVars)
        {
            _globalVars = globalVars;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<NPCInteractionRequestEvent>(OnInteractionRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<NPCInteractionRequestEvent>(OnInteractionRequested);
        }

        private void OnInteractionRequested(NPCInteractionRequestEvent e)
        {
            NPCData npcData = e.Data;
            if (npcData == null) return;

            ApplyResourceEffect(npcData);

            DialogueData dialogue = ResolveDialogue(npcData);
            if (dialogue == null)
                DebugSystem.LogWarning($"Ningun dialogo resuelto para NPC: {npcData.NPCID}", "NPC", "NPCSystem");
            else
                EventBus.Raise(new DialogueStartRequestEvent(dialogue, "NPCSystem"));

            EventBus.Raise(new NpcTalkedEvent(npcData.NPCID));

            if (npcData.LinkedQuest != null)
                EventBus.Raise(new QuestStartRequestEvent(npcData.LinkedQuest, "NPCSystem"));
        }

        private void ApplyResourceEffect(NPCData npcData)
        {
            if (npcData.ResourceEffect > 0f)
                EventBus.Raise(new ResourceCollectRequestEvent(npcData.ResourceEffect, $"NPC_{npcData.NPCID}"));
            else if (npcData.ResourceEffect < 0f)
                EventBus.Raise(new ResourceFullDrainRequestEvent($"NPC_{npcData.NPCID}", "NPCSystem"));
        }

        public DialogueData ResolveDialogue(NPCData npcData)
        {
            if (npcData == null) return null;

            string firstMeetFlag = $"NPC_{npcData.NPCID}_MET";
            bool wasMetBefore = _globalVars != null && _globalVars.HasFlag(firstMeetFlag) && _globalVars.GetFlag(firstMeetFlag);

            if (!wasMetBefore)
            {
                _globalVars?.SetFlag(firstMeetFlag, true);
                EventBus.Raise(new NpcFirstMeetEvent(npcData.NPCID));
            }

            DialogueData resolved = EvaluateConditionalDialogues(npcData);
            return resolved != null ? resolved : npcData.FallbackDialogue;
        }

        private DialogueData EvaluateConditionalDialogues(NPCData npcData)
        {
            NPCData.ConditionalDialogue best = null;

            foreach (var entry in npcData.DialogueStates)
            {
                bool conditionMet = entry.Condition == null ||
                                    (_globalVars != null && _globalVars.EvaluateCondition(entry.Condition));

                if (!conditionMet) continue;

                if (best == null || entry.Priority > best.Priority)
                    best = entry;
            }

            return best != null ? best.Dialogue : null;
        }
    }
}
