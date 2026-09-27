using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public enum ReactionTrigger
    {
        EnemyKilled,
        EnemyPacified,
        BossDefeated,
        SceneLoaded,
        NpcHelped
    }

    public enum ReactionEffect
    {
        IncrementVariable,
        SetVariable,
        SetFlag,
        TriggerRouteAction
    }

    [Serializable]
    public class EventReaction
    {
        [Header("Cuando")]
        public ReactionTrigger Trigger;
        [Tooltip("Vacio = cualquier ID. Si no, solo reacciona cuando el ID del evento coincide.")]
        public string TriggerID;

        [Header("Entonces")]
        public ReactionEffect Effect;
        [Tooltip("Clave de flag o variable. {ID} se sustituye por el ID del evento.")]
        public string Key;
        public float Amount = 1f;
        public bool FlagValue = true;
        public RouteActionData RouteAction;
    }

    [CreateAssetMenu(fileName = "EventReactionRules", menuName = "Infraestructura2DAction/Reactions/Event Reaction Rules")]
    public class EventReactionRules : ScriptableObject
    {
        public List<EventReaction> Reactions = new List<EventReaction>();
    }
}
