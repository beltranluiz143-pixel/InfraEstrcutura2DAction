using UnityEngine;

namespace Infra2DAction
{
    public enum AttackVariant { Normal, Up, Down, Air }

    public class PlayerAttackEvent : BaseEvent
    {
        public AttackVariant Variant { get; private set; }
        public int Damage { get; private set; }

        public PlayerAttackEvent(AttackVariant variant, int damage, string sourceID = "PlayerCombatSystem")
            : base(sourceID)
        {
            Variant = variant;
            Damage = damage;
        }
    }

    public class EnemyHitEvent : BaseEvent
    {
        public GameObject Target { get; private set; }
        public int Damage { get; private set; }
        public Vector2 KnockbackDirection { get; private set; }

        public EnemyHitEvent(GameObject target, int damage, Vector2 knockbackDirection, string sourceID = "PlayerCombatSystem")
            : base(sourceID)
        {
            Target = target;
            Damage = damage;
            KnockbackDirection = knockbackDirection;
        }
    }

    public class AttackFinishedEvent : BaseEvent
    {
        public AttackFinishedEvent(string sourceID = "PlayerCombatSystem") : base(sourceID) { }
    }

    public class EnemyWeakenedEvent : BaseEvent
    {
        public string EnemyID { get; private set; }

        public EnemyWeakenedEvent(string enemyID, string sourceID = "AIController")
            : base(sourceID)
        {
            EnemyID = enemyID;
        }
    }

    public class EnemyPacifiedEvent : BaseEvent
    {
        public string EnemyID { get; private set; }

        public EnemyPacifiedEvent(string enemyID, string sourceID = "PlayerCombatSystem")
            : base(sourceID)
        {
            EnemyID = enemyID;
        }
    }

    public class EnemyKilledEvent : BaseEvent
    {
        public string EnemyID { get; private set; }

        public EnemyKilledEvent(string enemyID, string sourceID = "EnemySystem")
            : base(sourceID)
        {
            EnemyID = enemyID;
        }
    }

    public class BossDefeatedEvent : BaseEvent
    {
        public string BossID { get; private set; }

        public BossDefeatedEvent(string bossID, string sourceID = "EnemySystem")
            : base(sourceID)
        {
            BossID = bossID;
        }
    }

    public class ShieldActivatedEvent : BaseEvent
    {
        public ShieldActivatedEvent(string sourceID = "ShieldSystem") : base(sourceID) { }
    }

    public class ShieldDeactivatedEvent : BaseEvent
    {
        public ShieldDeactivatedEvent(string sourceID = "ShieldSystem") : base(sourceID) { }
    }

    public class ShieldBrokenEvent : BaseEvent
    {
        public ShieldBrokenEvent(string sourceID = "ShieldSystem") : base(sourceID) { }
    }
}
