using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class MapSystem : MonoBehaviour
    {
        [SerializeField] private MapDatabase _mapDatabase;

        private SaveSystem _saveSystem;
        private string _currentSceneName;
        private GameObject _playerObject;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            DebugSystem.Log("MapSystem initialized.", "Map", "MapSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Subscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Unsubscribe<PlayerRegisteredEvent>(OnPlayerRegistered);
        }

        private void OnSceneLoaded(SceneLoadedEvent e) => _currentSceneName = e.SceneName;

        private void OnPlayerRegistered(PlayerRegisteredEvent e) => _playerObject = e.PlayerObject;

        public string GetCurrentSceneName() => _currentSceneName;

        public RoomMapData GetRoomData(string sceneName) => _mapDatabase != null ? _mapDatabase.GetRoom(sceneName) : null;

        public List<RoomMapData> GetVisitedRooms()
        {
            var result = new List<RoomMapData>();
            WorldSaveData world = _saveSystem != null ? _saveSystem.GetCurrentSave()?.World : null;
            if (_mapDatabase == null || world == null) return result;

            foreach (RoomMapData room in _mapDatabase.Rooms)
                if (room != null && world.HasVisitedScene(room.SceneName))
                    result.Add(room);

            return result;
        }

        public Vector2 GetPlayerWorldPosition()
            => _playerObject != null ? (Vector2)_playerObject.transform.position : Vector2.zero;

        public IReadOnlyList<MapPinSaveData> GetAllPins()
        {
            WorldSaveData world = _saveSystem != null ? _saveSystem.GetCurrentSave()?.World : null;
            return world != null ? (IReadOnlyList<MapPinSaveData>)world.Pins : new List<MapPinSaveData>();
        }

        public void AddPin(string sceneName, Vector2 worldPosition, string iconID, string label)
        {
            WorldSaveData world = _saveSystem != null ? _saveSystem.GetCurrentSave()?.World : null;
            if (world == null || string.IsNullOrEmpty(sceneName)) return;

            world.Pins.Add(new MapPinSaveData
            {
                SceneName = sceneName,
                WorldX = worldPosition.x,
                WorldY = worldPosition.y,
                IconID = iconID,
                Label = label ?? ""
            });

            EventBus.Raise(new SaveRequestedEvent());
            EventBus.Raise(new MapPinsChangedEvent());

            DebugSystem.Log($"Pin anadido en '{sceneName}' ({iconID}).", "Map", "MapSystem");
        }

        public void RemovePin(MapPinSaveData pin)
        {
            WorldSaveData world = _saveSystem != null ? _saveSystem.GetCurrentSave()?.World : null;
            if (world == null || pin == null) return;

            world.Pins.Remove(pin);

            EventBus.Raise(new SaveRequestedEvent());
            EventBus.Raise(new MapPinsChangedEvent());
        }
    }
}
