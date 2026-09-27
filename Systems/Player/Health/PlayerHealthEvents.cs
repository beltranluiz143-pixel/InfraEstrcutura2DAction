namespace Infra2DAction
{
    public class PlayerDamagedEvent : BaseEvent
    {
        public int CurrentHP { get; private set; }
        public int Damage { get; private set; }

        public PlayerDamagedEvent(int currentHP, int damage, string sourceID = "PlayerHealthSystem")
            : base(sourceID)
        {
            CurrentHP = currentHP;
            Damage = damage;
        }
    }

    public class PlayerHealedEvent : BaseEvent
    {
        public int CurrentHP { get; private set; }

        public PlayerHealedEvent(int currentHP, string sourceID = "PlayerHealthSystem")
            : base(sourceID)
        {
            CurrentHP = currentHP;
        }
    }

    public class PlayerDeathEvent : BaseEvent
    {
        public PlayerDeathEvent(string sourceID = "PlayerHealthSystem") : base(sourceID) { }
    }

    public class PlayerCriticalHealthEvent : BaseEvent
    {
        public bool IsCritical { get; private set; }

        public PlayerCriticalHealthEvent(bool isCritical, string sourceID = "PlayerHealthSystem")
            : base(sourceID)
        {
            IsCritical = isCritical;
        }
    }

    public class PlayerInvulnerabilityStartedEvent : BaseEvent
    {
        public PlayerInvulnerabilityStartedEvent(string sourceID = "PlayerHealthSystem") : base(sourceID) { }
    }

    public class PlayerHealthChangedEvent : BaseEvent
    {
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }

        public PlayerHealthChangedEvent(int currentHP, int maxHP, string sourceID = "PlayerHealthSystem")
            : base(sourceID)
        {
            CurrentHP = currentHP;
            MaxHP = maxHP;
        }
    }
}
