using UnityEngine;

namespace Infra2DAction
{
    public class EventReactionSystem : MonoBehaviour
    {
        private const string ID_PLACEHOLDER = "{ID}";

        [Header("Configuration")]
        [SerializeField] private EventReactionRules _rules;

        private GlobalVariablesSystem _globalVars;

        public void Initialize(GlobalVariablesSystem globalVars)
        {
            _globalVars = globalVars;
            DebugSystem.Log("EventReactionSystem initialized.", "Reactions", "EventReactionSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<EnemyPacifiedEvent>(OnEnemyPacified);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Subscribe<NpcHelpedEvent>(OnNpcHelped);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<EnemyPacifiedEvent>(OnEnemyPacified);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Unsubscribe<NpcHelpedEvent>(OnNpcHelped);
        }

        private void OnEnemyKilled(EnemyKilledEvent e) => React(ReactionTrigger.EnemyKilled, e.EnemyID);
        private void OnEnemyPacified(EnemyPacifiedEvent e) => React(ReactionTrigger.EnemyPacified, e.EnemyID);
        private void OnBossDefeated(BossDefeatedEvent e) => React(ReactionTrigger.BossDefeated, e.BossID);
        private void OnSceneLoaded(SceneLoadedEvent e) => React(ReactionTrigger.SceneLoaded, e.SceneName);
        private void OnNpcHelped(NpcHelpedEvent e) => React(ReactionTrigger.NpcHelped, e.NpcID);

        private void React(ReactionTrigger trigger, string id)
        {
            if (_rules == null || _globalVars == null) return;

            foreach (EventReaction reaction in _rules.Reactions)
            {
                if (reaction.Trigger != trigger) continue;
                if (!string.IsNullOrEmpty(reaction.TriggerID) && reaction.TriggerID != id) continue;

                Apply(reaction, id);
            }
        }

        private void Apply(EventReaction reaction, string id)
        {
            string key = string.IsNullOrEmpty(reaction.Key) ? "" : reaction.Key.Replace(ID_PLACEHOLDER, id);

            switch (reaction.Effect)
            {
                case ReactionEffect.IncrementVariable:
                    if (!string.IsNullOrEmpty(key)) _globalVars.IncrementVariable(key, reaction.Amount);
                    break;

                case ReactionEffect.SetVariable:
                    if (!string.IsNullOrEmpty(key)) _globalVars.SetVariable(key, reaction.Amount);
                    break;

                case ReactionEffect.SetFlag:
                    if (!string.IsNullOrEmpty(key)) _globalVars.SetFlag(key, reaction.FlagValue);
                    break;

                case ReactionEffect.TriggerRouteAction:
                    if (reaction.RouteAction != null)
                        EventBus.Raise(new RouteActionTriggeredEvent(reaction.RouteAction, "EventReactionSystem"));
                    break;
            }
        }
    }
}
