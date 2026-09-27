namespace Infra2DAction
{
    public class CutsceneAnimationOverrideEvent : BaseEvent
    {
        public bool IsActive { get; private set; }

        public CutsceneAnimationOverrideEvent(bool isActive, string sourceID = "CutsceneSystem")
            : base(sourceID)
        {
            IsActive = isActive;
        }
    }

    public class CutsceneForceAnimationEvent : BaseEvent
    {
        public string CharacterID { get; private set; }
        public string AnimationTrigger { get; private set; }

        public CutsceneForceAnimationEvent(string characterID, string animationTrigger, string sourceID = "CutsceneSystem")
            : base(sourceID)
        {
            CharacterID = characterID;
            AnimationTrigger = animationTrigger;
        }
    }
}
