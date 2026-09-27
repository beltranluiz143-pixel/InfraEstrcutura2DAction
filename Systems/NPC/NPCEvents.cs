namespace Infra2DAction
{
    public class NPCInteractionRequestEvent : BaseEvent
    {
        public NPCData Data { get; private set; }

        public NPCInteractionRequestEvent(NPCData data, string sourceID = "NPCInteractable")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class NpcFirstMeetEvent : BaseEvent
    {
        public string NPCID { get; private set; }

        public NpcFirstMeetEvent(string npcID, string sourceID = "NPCSystem")
            : base(sourceID)
        {
            NPCID = npcID;
        }
    }

    public class NpcTalkedEvent : BaseEvent
    {
        public string NPCID { get; private set; }

        public NpcTalkedEvent(string npcID, string sourceID = "NPCSystem")
            : base(sourceID)
        {
            NPCID = npcID;
        }
    }

    public class NpcHelpedEvent : BaseEvent
    {
        public string NpcID { get; private set; }

        public NpcHelpedEvent(string npcID, string sourceID = "NPCSystem")
            : base(sourceID)
        {
            NpcID = npcID;
        }
    }
}
