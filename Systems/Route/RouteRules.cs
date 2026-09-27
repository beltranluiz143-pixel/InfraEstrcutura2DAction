using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "RouteRules", menuName = "Infraestructura2DAction/Route/Route Rules")]
    public class RouteRules : ScriptableObject
    {
        [Serializable]
        public class RouteRule
        {
            public string FromRoute;
            public List<string> AllowedTransitions = new List<string>();
        }

        [SerializeField] private List<RouteRule> _rules = new List<RouteRule>();

        public bool CanTransitionTo(string fromRoute, string toRoute)
        {
            if (fromRoute == toRoute) return true;

            foreach (RouteRule rule in _rules)
            {
                if (rule.FromRoute == fromRoute)
                    return rule.AllowedTransitions.Contains(toRoute);
            }

            return true;
        }
    }
}
