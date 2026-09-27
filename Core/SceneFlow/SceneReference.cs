using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "SceneRef_", menuName = "Infraestructura2DAction/Core/Scene Reference")]
    public class SceneReference : ScriptableObject
    {
        [SerializeField] public string SceneName = "";
        [SerializeField] public string DefaultSpawnPointID = "Default";
    }
}
