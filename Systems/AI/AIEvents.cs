using UnityEngine;

namespace Infra2DAction
{
    public enum AIState { Idle, Patrol, Chase, Attack, Pacifiable, Dead }

    public class EnemyStateChangedEvent : BaseEvent
    {
        public AIState PreviousState { get; private set; }
        public AIState NewState { get; private set; }

        public EnemyStateChangedEvent(AIState previous, AIState newState, string sourceID = "AIController")
            : base(sourceID)
        {
            PreviousState = previous;
            NewState = newState;
        }
    }

    public class BossPhaseChangedEvent : BaseEvent
    {
        public string BossID { get; private set; }
        public int PhaseIndex { get; private set; }

        public BossPhaseChangedEvent(string bossID, int phaseIndex, string sourceID = "AIController")
            : base(sourceID)
        {
            BossID = bossID;
            PhaseIndex = phaseIndex;
        }
    }

    public class EnemyAttackEvent : BaseEvent
    {
        public int Damage { get; private set; }
        public Vector2 Origin { get; private set; }

        public EnemyAttackEvent(int damage, Vector2 origin, string sourceID = "AttackHandler")
            : base(sourceID)
        {
            Damage = damage;
            Origin = origin;
        }
    }
}
