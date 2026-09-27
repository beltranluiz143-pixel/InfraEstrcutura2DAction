using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "RouteData_", menuName = "Infraestructura2DAction/Route/Route Data")]
    public class RouteData : ScriptableObject
    {
        [Header("Identification")]
        public string RouteID;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Score")]
        public float InitialScore = 0f;
    }
}
