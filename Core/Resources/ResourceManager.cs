using UnityEngine;

namespace Infra2DAction
{
    public class ResourceManager : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private AssetReferenceDatabase _database;

        public void Initialize()
        {
            if (_database == null)
            {
                DebugSystem.LogError("AssetReferenceDatabase no asignada.", "Resources", "ResourceManager");
                return;
            }

            _database.Initialize();
            DebugSystem.Log("ResourceManager initialized.", "Resources", "ResourceManager");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
        }

        public GameObject GetPrefab(string assetID)
        {
            if (_database == null)
            {
                DebugSystem.LogError("Database no inicializada.", "Resources", "ResourceManager");
                return null;
            }

            return _database.GetPrefab(assetID);
        }

        public AssetReferenceDatabase.AssetEntry GetEntry(string assetID)
        {
            return _database?.GetEntry(assetID);
        }

        public System.Collections.Generic.List<AssetReferenceDatabase.AssetEntry>
            GetPoolableEntries() => _database?.GetPoolableEntries();

        private void OnSceneLoaded(SceneLoadedEvent e)
        {
            DebugSystem.Log($"Scene loaded: {e.SceneName}. Assets ready.", "Resources", "ResourceManager");

            EventBus.Raise(new ResourceLoadedEvent(e.SceneName));
        }
    }
}
