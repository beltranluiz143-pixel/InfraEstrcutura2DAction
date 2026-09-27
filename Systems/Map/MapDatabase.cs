using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "MapDatabase", menuName = "Infraestructura2DAction/Content/Map Database")]
    public class MapDatabase : ScriptableObject
    {
        public List<RoomMapData> Rooms = new List<RoomMapData>();

        public RoomMapData GetRoom(string sceneName)
            => string.IsNullOrEmpty(sceneName) ? null : Rooms.Find(r => r != null && r.SceneName == sceneName);
    }
}
