using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class RouteSystem : MonoBehaviour
    {
        private const string SCORE_KEY_PREFIX = "ROUTE_SCORE_";
        private const string DOMINANT_KEY_PREFIX = "ROUTE_DOMINANT_";

        [Header("Configuration")]
        [SerializeField] private List<RouteData> _routes = new List<RouteData>();
        [SerializeField] private RouteRules _routeRules;

        private readonly List<string> _routeIDs = new List<string>();
        private readonly Dictionary<string, RouteData> _definitions = new Dictionary<string, RouteData>();
        private readonly Dictionary<string, float> _scores = new Dictionary<string, float>();

        private string _dominantRouteID = "";
        private GlobalVariablesSystem _globalVars;

        public void Initialize(GlobalVariablesSystem globalVars)
        {
            _globalVars = globalVars;

            _routeIDs.Clear();
            _definitions.Clear();
            _scores.Clear();

            foreach (RouteData route in _routes)
                AddDefinition(route);

            _dominantRouteID = FindHighestRoute();

            RestoreFromGlobalVars();
            SyncScoresToGlobalVars();
            SyncDominantFlagsToGlobalVars();

            DebugSystem.Log($"RouteSystem initialized. Rutas: {_routeIDs.Count} | Dominante: '{_dominantRouteID}'",
                            "Route", "RouteSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<RouteActionTriggeredEvent>(OnRouteAction);
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RouteActionTriggeredEvent>(OnRouteAction);
            EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
        }

        public void RegisterRoute(RouteData route)
        {
            if (!AddDefinition(route)) return;

            SyncScoresToGlobalVars();
            SyncDominantFlagsToGlobalVars();
            RecalculateDominantRoute();
        }

        public void AddScore(string routeID, float amount)
        {
            if (!ApplyScore(routeID, amount)) return;

            SyncScoresToGlobalVars();
            RecalculateDominantRoute();
        }

        public string GetDominantRoute() => _dominantRouteID;

        public bool HasRoute(string routeID) => _definitions.ContainsKey(routeID);

        public float GetRouteScore(string routeID)
            => _scores.TryGetValue(routeID, out float score) ? score : 0f;

        public Dictionary<string, float> GetAllRouteScores() => new Dictionary<string, float>(_scores);

        private bool AddDefinition(RouteData route)
        {
            if (route == null || string.IsNullOrEmpty(route.RouteID))
            {
                DebugSystem.LogWarning("RouteData nula o sin RouteID ignorada.", "Route", "RouteSystem");
                return false;
            }

            if (_definitions.ContainsKey(route.RouteID))
            {
                DebugSystem.LogWarning($"RouteID duplicado ignorado: {route.RouteID}", "Route", "RouteSystem");
                return false;
            }

            _routeIDs.Add(route.RouteID);
            _definitions[route.RouteID] = route;
            _scores[route.RouteID] = route.InitialScore;
            return true;
        }

        private bool ApplyScore(string routeID, float amount)
        {
            if (!_scores.ContainsKey(routeID))
            {
                DebugSystem.LogWarning($"Ruta no registrada: {routeID}", "Route", "RouteSystem");
                return false;
            }

            _scores[routeID] += amount;
            return true;
        }

        private void OnRouteAction(RouteActionTriggeredEvent e)
        {
            if (e.ActionData == null) return;

            bool changed = false;
            foreach (RouteWeight weight in e.ActionData.Weights)
                changed |= ApplyScore(weight.RouteID, weight.Weight);

            if (!changed) return;

            SyncScoresToGlobalVars();
            RecalculateDominantRoute();
        }

        private string FindHighestRoute()
        {
            string best = "";
            float bestScore = float.MinValue;

            foreach (string id in _routeIDs)
            {
                if (_scores[id] > bestScore)
                {
                    bestScore = _scores[id];
                    best = id;
                }
            }

            return best;
        }

        private void RecalculateDominantRoute()
        {
            if (_routeIDs.Count == 0) return;

            float max = float.MinValue;
            foreach (string id in _routeIDs)
                if (_scores[id] > max) max = _scores[id];

            string candidate = null;
            int leaders = 0;
            foreach (string id in _routeIDs)
            {
                if (Mathf.Approximately(_scores[id], max))
                {
                    leaders++;
                    candidate = id;
                }
            }

            if (leaders != 1 || candidate == _dominantRouteID) return;

            if (_routeRules != null && !_routeRules.CanTransitionTo(_dominantRouteID, candidate)) return;

            string previous = _dominantRouteID;
            _dominantRouteID = candidate;

            if (_globalVars != null)
            {
                if (!string.IsNullOrEmpty(previous))
                    _globalVars.SetFlag(DOMINANT_KEY_PREFIX + previous, false);
                _globalVars.SetFlag(DOMINANT_KEY_PREFIX + candidate, true);
            }

            EventBus.Raise(new RouteChangedEvent(previous, candidate));
        }

        private void SyncScoresToGlobalVars()
        {
            if (_globalVars == null) return;

            foreach (string id in _routeIDs)
                _globalVars.SetVariable(SCORE_KEY_PREFIX + id, _scores[id]);
        }

        private void SyncDominantFlagsToGlobalVars()
        {
            if (_globalVars == null) return;

            foreach (string id in _routeIDs)
                _globalVars.SetFlag(DOMINANT_KEY_PREFIX + id, id == _dominantRouteID);
        }

        private void RestoreFromGlobalVars()
        {
            if (_globalVars == null) return;

            foreach (string id in _routeIDs)
            {
                string scoreKey = SCORE_KEY_PREFIX + id;
                if (_globalVars.HasVariable(scoreKey))
                    _scores[id] = _globalVars.GetVariable(scoreKey);
            }

            foreach (string id in _routeIDs)
            {
                string dominantKey = DOMINANT_KEY_PREFIX + id;
                if (_globalVars.HasFlag(dominantKey) && _globalVars.GetFlag(dominantKey))
                {
                    _dominantRouteID = id;
                    return;
                }
            }

            _dominantRouteID = FindHighestRoute();
        }

        private void OnSaveLoaded(SaveLoadedEvent e)
        {
            if (_routeIDs.Count == 0 || e.Data?.World == null) return;

            WorldSaveData world = e.Data.World;

            foreach (string id in _routeIDs)
                _scores[id] = world.GetVariable(SCORE_KEY_PREFIX + id, _definitions[id].InitialScore);

            string dominant = null;
            foreach (string id in _routeIDs)
            {
                if (world.GetFlag(DOMINANT_KEY_PREFIX + id))
                {
                    dominant = id;
                    break;
                }
            }

            _dominantRouteID = dominant ?? FindHighestRoute();

            SyncScoresToGlobalVars();
            SyncDominantFlagsToGlobalVars();
        }
    }
}
