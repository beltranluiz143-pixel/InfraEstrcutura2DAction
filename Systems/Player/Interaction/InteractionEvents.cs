using UnityEngine;

namespace Infra2DAction
{
    public class InteractionPromptShownEvent : BaseEvent
    {
        public InteractionPromptShownEvent(string sourceID = "InteractionSystem") : base(sourceID) { }
    }

    public class InteractionPromptHiddenEvent : BaseEvent
    {
        public InteractionPromptHiddenEvent(string sourceID = "InteractionSystem") : base(sourceID) { }
    }

    public class InteractionTriggeredEvent : BaseEvent
    {
        public GameObject Target { get; private set; }

        public InteractionTriggeredEvent(GameObject target, string sourceID = "InteractionSystem")
            : base(sourceID)
        {
            Target = target;
        }
    }
}
