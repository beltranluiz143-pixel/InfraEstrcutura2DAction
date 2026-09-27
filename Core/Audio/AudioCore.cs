using UnityEngine;

namespace Infra2DAction
{
    public class AudioCore : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private SoundDatabase _soundDatabase;

        [Header("Subsystems")]
        [SerializeField] private MusicManager _musicManager;
        [SerializeField] private SFXManager _sfxManager;
        [SerializeField] private SnapshotController _snapshotController;

        public void Initialize(PoolManager poolManager)
        {
            if (_soundDatabase == null)
            {
                DebugSystem.LogError("SoundDatabase no asignada.", "Audio", "AudioCore");
                return;
            }

            _soundDatabase.Initialize();

            _musicManager?.Initialize(_soundDatabase);
            _sfxManager?.Initialize(_soundDatabase, poolManager);

            DebugSystem.Log("AudioCore initialized.", "Audio", "AudioCore");
        }
    }
}
