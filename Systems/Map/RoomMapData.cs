using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "RoomMap_", menuName = "Infraestructura2DAction/Content/Room Map Data")]
    public class RoomMapData : ScriptableObject
    {
        [Header("Identificacion")]
        [Tooltip("Debe coincidir EXACTO con el nombre de la escena de nivel (SceneLoadedEvent.SceneName).")]
        public string SceneName;

        [Header("Posicion en el mapa general")]
        [Tooltip("Coordenada de celda dentro de la cuadricula del mapa completo. MapPanel la multiplica por su Cell Size.")]
        public Vector2Int MapGridPosition;

        [Header("Visual")]
        public Sprite RoomSilhouette;

        [Header("Limites en coordenadas de mundo de la escena")]
        public Vector2 WorldBoundsMin;
        public Vector2 WorldBoundsMax;

        public Vector2 WorldToNormalized(Vector2 worldPosition)
        {
            float sizeX = WorldBoundsMax.x - WorldBoundsMin.x;
            float sizeY = WorldBoundsMax.y - WorldBoundsMin.y;

            float nx = sizeX > 0f ? (worldPosition.x - WorldBoundsMin.x) / sizeX : 0.5f;
            float ny = sizeY > 0f ? (worldPosition.y - WorldBoundsMin.y) / sizeY : 0.5f;

            return new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny));
        }

        public Vector2 NormalizedToWorld(Vector2 normalized)
        {
            float x = Mathf.Lerp(WorldBoundsMin.x, WorldBoundsMax.x, normalized.x);
            float y = Mathf.Lerp(WorldBoundsMin.y, WorldBoundsMax.y, normalized.y);
            return new Vector2(x, y);
        }
    }
}
