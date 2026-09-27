using System.Collections;
using UnityEngine;

namespace Infra2DAction
{
    public class MusicManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioSource _musicSource;

        [Header("Configuration")]
        [SerializeField] private float _fadeDuration = 1.5f;

        private SoundDatabase _database;
        private float _musicVolume = 0.8f;
        private string _currentMusicID;
        private Coroutine _fadeCoroutine;

        public void Initialize(SoundDatabase database)
        {
            _database = database;

            if (_musicSource == null)
                DebugSystem.LogError("AudioSource no asignado en MusicManager.", "Audio", "MusicManager");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AudioSettingsChangedEvent>(OnAudioSettingsChanged);
            EventBus.Subscribe<MusicPlayRequestEvent>(OnMusicPlayRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AudioSettingsChangedEvent>(OnAudioSettingsChanged);
            EventBus.Unsubscribe<MusicPlayRequestEvent>(OnMusicPlayRequested);
        }

        private void OnAudioSettingsChanged(AudioSettingsChangedEvent e)
        {
            _musicVolume = e.MusicVolume;
            if (_musicSource != null && _musicSource.isPlaying)
                _musicSource.volume = _musicVolume;
        }

        private void OnMusicPlayRequested(MusicPlayRequestEvent e)
        {
            PlayMusic(e.MusicID);
        }

        public void PlayMusic(string musicID)
        {
            if (_currentMusicID == musicID) return;

            SoundDatabase.SoundEntry entry = _database?.GetEntry(musicID);
            if (entry == null) return;

            _currentMusicID = musicID;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeToNewTrack(entry.Clip));

            EventBus.Raise(new MusicChangedEvent(musicID));
        }

        private IEnumerator FadeToNewTrack(AudioClip newClip)
        {
            if (_musicSource == null)
            {
                DebugSystem.LogError("AudioSource no asignado en MusicManager.", "Audio", "MusicManager");
                yield break;
            }

            float elapsed = 0f;
            float startVolume = _musicSource.volume;

            while (elapsed < _fadeDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (_fadeDuration / 2f));
                yield return null;
            }

            _musicSource.clip = newClip;
            _musicSource.Play();

            elapsed = 0f;
            while (elapsed < _fadeDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(0f, _musicVolume, elapsed / (_fadeDuration / 2f));
                yield return null;
            }

            _musicSource.volume = _musicVolume;
        }

        public void Stop() => _musicSource?.Stop();
    }
}
