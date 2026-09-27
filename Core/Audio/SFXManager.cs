using UnityEngine;

namespace Infra2DAction
{
    public class SFXManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _sfxPoolAssetID = "AUDIO_SOURCE_SFX";

        private SoundDatabase _database;
        private PoolManager _poolManager;
        private float _sfxVolume = 1f;

        public void Initialize(SoundDatabase database, PoolManager poolManager)
        {
            _database = database;
            _poolManager = poolManager;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SFXPlayRequestEvent>(OnSFXPlayRequest);
            EventBus.Subscribe<AudioSettingsChangedEvent>(OnAudioSettingsChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SFXPlayRequestEvent>(OnSFXPlayRequest);
            EventBus.Unsubscribe<AudioSettingsChangedEvent>(OnAudioSettingsChanged);
        }

        private void OnSFXPlayRequest(SFXPlayRequestEvent e)
        {
            PlaySFX(e.SoundID, e.Position);
        }

        private void OnAudioSettingsChanged(AudioSettingsChangedEvent e)
        {
            _sfxVolume = e.SFXVolume;
        }

        public void PlaySFX(string soundID, Vector3 position)
        {
            SoundDatabase.SoundEntry entry = _database?.GetEntry(soundID);
            if (entry == null) return;

            if (_poolManager == null)
            {
                DebugSystem.LogError("PoolManager no inicializado.", "Audio", "SFXManager");
                return;
            }

            GameObject sfxObject = _poolManager.Spawn(_sfxPoolAssetID, position);
            if (sfxObject == null)
            {
                DebugSystem.LogWarning("No se pudo obtener objeto de audio del pool.", "Audio", "SFXManager");
                return;
            }

            AudioSource source = sfxObject.GetComponent<AudioSource>();
            if (source == null)
            {
                DebugSystem.LogError("Prefab de SFX sin AudioSource.", "Audio", "SFXManager");
                return;
            }

            source.clip = entry.Clip;
            source.volume = entry.DefaultVolume * _sfxVolume;
            source.Play();

            sfxObject.GetComponent<PooledObject>()?.AutoDespawn(entry.Clip.length);
        }
    }
}
