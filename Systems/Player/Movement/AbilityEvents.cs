namespace Infra2DAction
{
    public class AbilityUnlockRequestEvent : BaseEvent
    {
        public string AbilityFlagKey { get; private set; }

        public AbilityUnlockRequestEvent(string abilityFlagKey, string sourceID = "Unknown")
            : base(sourceID)
        {
            AbilityFlagKey = abilityFlagKey;
        }
    }
}
