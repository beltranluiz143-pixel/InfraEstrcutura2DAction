using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [Serializable]
    public class RouteWeight
    {
        public string RouteID;
        public float Weight;
    }

    [CreateAssetMenu(fileName = "RouteAction_", menuName = "Infraestructura2DAction/Route/Route Action Data")]
    public class RouteActionData : ScriptableObject
    {
        [Header("Identification")]
        public string ActionID;

        [Header("Pesos por ruta")]
        public List<RouteWeight> Weights = new List<RouteWeight>();
    }
}
