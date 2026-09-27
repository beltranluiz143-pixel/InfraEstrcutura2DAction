namespace Infra2DAction
{
    public class CutsceneStartRequestEvent : BaseEvent
    {
        public CutsceneData Data { get; private set; }

        public CutsceneStartRequestEvent(CutsceneData data, string sourceID = "Unknown")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class CutsceneStartedEvent : BaseEvent
    {
        public CutsceneStartedEvent(string sourceID = "CutsceneSystem") : base(sourceID) { }
    }

    public class CutsceneEndedEvent : BaseEvent
    {
        public CutsceneEndedEvent(string sourceID = "CutsceneSystem") : base(sourceID) { }
    }

    public class CutsceneCharacterRegisteredEvent : BaseEvent
    {
        public CutsceneCharacterReference Character { get; private set; }

        public CutsceneCharacterRegisteredEvent(CutsceneCharacterReference character, string sourceID = "CutsceneCharacterReference")
            : base(sourceID)
        {
            Character = character;
        }
    }

    public class CutsceneCharacterUnregisteredEvent : BaseEvent
    {
        public CutsceneCharacterReference Character { get; private set; }

        public CutsceneCharacterUnregisteredEvent(CutsceneCharacterReference character, string sourceID = "CutsceneCharacterReference")
            : base(sourceID)
        {
            Character = character;
        }
    }
}
