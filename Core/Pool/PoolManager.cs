using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class PoolManager : MonoBehaviour
    {
        private ResourceManager _resourceManager;

        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private HashSet<GameObject> _activeObjects = new HashSet<GameObject>();

        private Transform _poolContainer;

        public void Initialize(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;

            _poolContainer = new GameObject("_POOL_CONTAINER").transform;
            _poolContainer.SetParent(transform);

            WarmUpPools(resourceManager);

            DebugSystem.Log("PoolManager initialized.", "Pool", "PoolManager");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Subscribe<PoolSpawnRequestEvent>(OnSpawnRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
            EventBus.Unsubscribe<PoolSpawnRequestEvent>(OnSpawnRequested);
        }

        private void OnSpawnRequested(PoolSpawnRequestEvent e)
        {
            e.SetResult(Spawn(e.AssetID, e.Position, e.Rotation));
        }

        private void WarmUpPools(ResourceManager resourceManager)
        {
            var poolableEntries = resourceManager.GetPoolableEntries();
            if (poolableEntries == null) return;

            foreach (var entry in poolableEntries)
                WarmUpPool(entry.ID, entry.Prefab, entry.PoolSize);
        }

        private void WarmUpPool(string assetID, GameObject prefab, int size)
        {
            if (!_pools.ContainsKey(assetID))
                _pools[assetID] = new Queue<GameObject>();

            for (int i = 0; i < size; i++)
                _pools[assetID].Enqueue(CreatePooledObject(assetID, prefab));

            DebugSystem.Log($"Pool warmed: {assetID} x{size}", "Pool", "PoolManager");
        }

        private GameObject CreatePooledObject(string assetID, GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, _poolContainer);
            obj.SetActive(false);

            PooledObject pooled = obj.GetComponent<PooledObject>();
            if (pooled == null) pooled = obj.AddComponent<PooledObject>();
            pooled.AssetID = assetID;
            pooled.Owner = this;

            return obj;
        }

        public GameObject Spawn(string assetID, Vector3 position, Quaternion rotation = default)
        {
            if (!_pools.ContainsKey(assetID) || _pools[assetID].Count == 0)
            {
                GameObject prefab = _resourceManager?.GetPrefab(assetID);

                if (prefab == null)
                {
                    DebugSystem.LogError($"No se puede spawnear: {assetID}", "Pool", "PoolManager");
                    return null;
                }

                if (!_pools.ContainsKey(assetID))
                    _pools[assetID] = new Queue<GameObject>();

                _pools[assetID].Enqueue(CreatePooledObject(assetID, prefab));
            }

            GameObject obj = _pools[assetID].Dequeue();
            obj.transform.SetParent(null);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);

            _activeObjects.Add(obj);

            obj.GetComponent<PooledObject>()?.OnSpawn();

            EventBus.Raise(new ObjectSpawnedEvent(assetID, obj));
            return obj;
        }

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            PooledObject pooled = obj.GetComponent<PooledObject>();

            if (pooled == null)
            {
                DebugSystem.LogWarning("Objeto sin PooledObject enviado al Despawn.", "Pool", "PoolManager");
                Destroy(obj);
                return;
            }

            pooled.OnDespawn();

            string assetID = pooled.AssetID;

            _activeObjects.Remove(obj);

            obj.SetActive(false);
            obj.transform.SetParent(_poolContainer);

            if (!_pools.ContainsKey(assetID))
                _pools[assetID] = new Queue<GameObject>();

            _pools[assetID].Enqueue(obj);

            EventBus.Raise(new ObjectDespawnedEvent(assetID));
        }

        private void OnSceneLoaded(SceneLoadedEvent e)
        {
            DespawnAllActive();
            DebugSystem.Log("Pool cleaned for new scene.", "Pool", "PoolManager");
        }

        private void DespawnAllActive()
        {
            var snapshot = new List<GameObject>(_activeObjects);

            foreach (GameObject obj in snapshot)
            {
                if (obj != null && obj.activeSelf)
                    Despawn(obj);
            }
        }

        public int GetAvailableCount(string assetID)
            => _pools.ContainsKey(assetID) ? _pools[assetID].Count : 0;

        public int GetActiveCount() => _activeObjects.Count;

        public void PrintPoolStatus()
        {
            DebugSystem.Log("===== POOL STATUS =====", "Pool", "PoolManager");
            foreach (var kvp in _pools)
                DebugSystem.Log($"{kvp.Key}: {kvp.Value.Count} available", "Pool", "PoolManager");
            DebugSystem.Log($"Active objects: {_activeObjects.Count}", "Pool", "PoolManager");
        }
    }
}
