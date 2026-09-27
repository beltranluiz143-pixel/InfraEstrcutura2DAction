using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Infra2DAction
{
    public class MapPanel : MonoBehaviour
    {
        [Header("Sistema")]
        [SerializeField] private MapSystem _mapSystem;

        [Header("Layout")]
        [SerializeField] private RectTransform _mapContainer;
        [SerializeField] private GameObject _roomIconPrefab;
        [SerializeField] private GameObject _playerMarker;
        [SerializeField] private GameObject _pinIconPrefab;

        [Tooltip("Tamano en pixeles de cada celda de MapGridPosition, y del icono de cada sala.")]
        [SerializeField] private float _cellSize = 64f;

        [Header("Colocar pines")]
        [SerializeField] private MapPinIconDatabase _pinIconDatabase;
        [SerializeField] private GameObject _pinPalettePanel;
        [SerializeField] private Transform _pinPaletteButtonContainer;
        [SerializeField] private GameObject _pinPaletteButtonPrefab;
        [SerializeField] private Button _addPinButton;
        [SerializeField] private Button _cancelPinButton;

        private bool _pinPlacementMode;
        private string _pendingIconID;

        private readonly List<GameObject> _spawnedRoomIcons = new List<GameObject>();
        private readonly List<GameObject> _spawnedPinIcons = new List<GameObject>();

        private void OnEnable()
        {
            if (_addPinButton != null) _addPinButton.onClick.AddListener(OpenPinPalette);
            if (_cancelPinButton != null) _cancelPinButton.onClick.AddListener(CancelPinPlacement);
            RefreshMap();
        }

        private void OnDisable()
        {
            if (_addPinButton != null) _addPinButton.onClick.RemoveListener(OpenPinPalette);
            if (_cancelPinButton != null) _cancelPinButton.onClick.RemoveListener(CancelPinPlacement);
            CancelPinPlacement();
        }

        private void RefreshMap()
        {
            if (_mapSystem == null || _mapContainer == null || _roomIconPrefab == null) return;

            ClearSpawned(_spawnedRoomIcons);
            ClearSpawned(_spawnedPinIcons);

            RoomMapData currentRoom = _mapSystem.GetRoomData(_mapSystem.GetCurrentSceneName());

            foreach (RoomMapData room in _mapSystem.GetVisitedRooms())
            {
                GameObject iconObject = Instantiate(_roomIconPrefab, _mapContainer);
                RectTransform rect = iconObject.GetComponent<RectTransform>();
                Image image = iconObject.GetComponent<Image>();

                if (rect != null) rect.anchoredPosition = GetRoomAnchoredPosition(room);
                if (image != null) image.sprite = room.RoomSilhouette;

                if (room == currentRoom)
                {
                    Button roomButton = iconObject.GetComponent<Button>();
                    RectTransform capturedRect = rect;
                    if (roomButton != null) roomButton.onClick.AddListener(() => OnRoomIconClicked(capturedRect));
                }

                _spawnedRoomIcons.Add(iconObject);
            }

            PositionPlayerMarker(currentRoom);
            DrawPins();
        }

        private Vector2 GetRoomAnchoredPosition(RoomMapData room)
            => new Vector2(room.MapGridPosition.x, room.MapGridPosition.y) * _cellSize;

        private void PositionPlayerMarker(RoomMapData currentRoom)
        {
            if (_playerMarker == null) return;

            if (currentRoom == null)
            {
                _playerMarker.SetActive(false);
                return;
            }

            _playerMarker.SetActive(true);

            Vector2 worldPos = _mapSystem.GetPlayerWorldPosition();
            Vector2 normalized = currentRoom.WorldToNormalized(worldPos);
            Vector2 offset = (normalized - new Vector2(0.5f, 0.5f)) * _cellSize;

            RectTransform markerRect = _playerMarker.GetComponent<RectTransform>();
            if (markerRect != null)
                markerRect.anchoredPosition = GetRoomAnchoredPosition(currentRoom) + offset;
        }

        private void DrawPins()
        {
            if (_pinIconPrefab == null || _mapSystem == null) return;

            foreach (MapPinSaveData pin in _mapSystem.GetAllPins())
            {
                RoomMapData room = _mapSystem.GetRoomData(pin.SceneName);
                if (room == null) continue;

                GameObject pinObject = Instantiate(_pinIconPrefab, _mapContainer);
                RectTransform rect = pinObject.GetComponent<RectTransform>();
                Image image = pinObject.GetComponent<Image>();

                Vector2 normalized = room.WorldToNormalized(new Vector2(pin.WorldX, pin.WorldY));
                Vector2 offset = (normalized - new Vector2(0.5f, 0.5f)) * _cellSize;

                if (rect != null) rect.anchoredPosition = GetRoomAnchoredPosition(room) + offset;
                if (image != null && _pinIconDatabase != null) image.sprite = _pinIconDatabase.GetIcon(pin.IconID);

                MapPinSaveData capturedPin = pin;
                Button pinButton = pinObject.GetComponent<Button>();
                if (pinButton != null) pinButton.onClick.AddListener(() => OnPinClicked(capturedPin));

                _spawnedPinIcons.Add(pinObject);
            }
        }

        private void ClearSpawned(List<GameObject> list)
        {
            foreach (GameObject obj in list) Destroy(obj);
            list.Clear();
        }

        private void OpenPinPalette()
        {
            if (_pinPalettePanel == null || _pinIconDatabase == null ||
                _pinPaletteButtonContainer == null || _pinPaletteButtonPrefab == null) return;

            foreach (Transform child in _pinPaletteButtonContainer) Destroy(child.gameObject);

            foreach (MapPinIconDatabase.Entry entry in _pinIconDatabase.Icons)
            {
                GameObject buttonObject = Instantiate(_pinPaletteButtonPrefab, _pinPaletteButtonContainer);
                Image icon = buttonObject.GetComponentInChildren<Image>();
                if (icon != null) icon.sprite = entry.Icon;

                string capturedID = entry.IconID;
                Button button = buttonObject.GetComponent<Button>();
                if (button != null) button.onClick.AddListener(() => OnIconChosen(capturedID));
            }

            _pinPalettePanel.SetActive(true);
        }

        private void OnIconChosen(string iconID)
        {
            _pendingIconID = iconID;
            _pinPlacementMode = true;
            if (_pinPalettePanel != null) _pinPalettePanel.SetActive(false);
        }

        private void OnRoomIconClicked(RectTransform roomRect)
        {
            if (!_pinPlacementMode || roomRect == null || Mouse.current == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                roomRect, Mouse.current.position.ReadValue(), null, out Vector2 localPoint);

            Vector2 normalized = new Vector2(
                Mathf.Clamp01(localPoint.x / roomRect.rect.width + 0.5f),
                Mathf.Clamp01(localPoint.y / roomRect.rect.height + 0.5f));

            RoomMapData currentRoom = _mapSystem.GetRoomData(_mapSystem.GetCurrentSceneName());
            if (currentRoom == null) return;

            Vector2 worldPos = currentRoom.NormalizedToWorld(normalized);
            _mapSystem.AddPin(currentRoom.SceneName, worldPos, _pendingIconID, "");

            CancelPinPlacement();
            RefreshMap();
        }

        private void CancelPinPlacement()
        {
            _pinPlacementMode = false;
            _pendingIconID = null;
            if (_pinPalettePanel != null) _pinPalettePanel.SetActive(false);
        }

        private void OnPinClicked(MapPinSaveData pin)
        {
            if (_pinPlacementMode) return;

            _mapSystem.RemovePin(pin);
            RefreshMap();
        }
    }
}
