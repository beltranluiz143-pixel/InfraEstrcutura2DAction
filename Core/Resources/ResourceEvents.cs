using UnityEngine;

namespace Infra2DAction
{
    public class ResourceLoadedEvent : BaseEvent
    {
        public string AssetID { get; private set; }

        public ResourceLoadedEvent(string assetID, string sourceID = "ResourceManager")
            : base(sourceID)
        {
            AssetID = assetID;
        }
    }

    public class ObjectSpawnedEvent : BaseEvent
    {
        public string AssetID { get; private set; }
        public GameObject Instance { get; private set; }

        public ObjectSpawnedEvent(string assetID, GameObject instance, string sourceID = "PoolManager")
            : base(sourceID)
        {
            AssetID = assetID;
            Instance = instance;
        }
    }

    public class ObjectDespawnedEvent : BaseEvent
    {
        public string AssetID { get; private set; }

        public ObjectDespawnedEvent(string assetID, string sourceID = "PoolManager")
            : base(sourceID)
        {
            AssetID = assetID;
        }
    }

    public class PoolSpawnRequestEvent : BaseEvent
    {
        public string AssetID { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public GameObject Result { get; private set; }

        public PoolSpawnRequestEvent(string assetID, Vector3 position, Quaternion rotation = default, string sourceID = "Unknown")
            : base(sourceID)
        {
            AssetID = assetID;
            Position = position;
            Rotation = rotation;
        }

        public void SetResult(GameObject spawned)
        {
            Result = spawned;
        }
    }
}
