using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Infra2DAction.EditorTools
{
    public class AudioPreviewWindow : EditorWindow
    {
        private AudioClip _selectedClip;
        private bool _isLooping;
        private bool _isPlaying;

        private System.Type _audioUtilType;
        private MethodInfo _playMethod;
        private MethodInfo _stopMethod;
        private MethodInfo _positionMethod;
        private MethodInfo _isPlayingMethod;
        private double _playStartTime;

        [MenuItem(EditorToolsUtility.MenuRoot + "Audio Preview", false, 162)]
        public static void OpenWindow()
        {
            AudioPreviewWindow window = GetWindow<AudioPreviewWindow>("Audio Preview");
            window.minSize = new Vector2(360, 300);
        }

        private void OnEnable() => CacheAudioUtilMethods();
        private void OnDisable() => StopPreview();

        private void CacheAudioUtilMethods()
        {
            _audioUtilType = System.Type.GetType("UnityEditor.AudioUtil, UnityEditor");
            if (_audioUtilType == null)
            {
                Debug.LogWarning("[AudioPreview] UnityEditor.AudioUtil no encontrado. " +
                                 "La previsualización de audio no estará disponible.");
                return;
            }

            _playMethod = _audioUtilType.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);

            _stopMethod = _audioUtilType.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public);
            _positionMethod = _audioUtilType.GetMethod("GetPreviewClipPosition", BindingFlags.Static | BindingFlags.Public);
            _isPlayingMethod = _audioUtilType.GetMethod("IsPreviewClipPlaying", BindingFlags.Static | BindingFlags.Public);
        }

        private void OnGUI()
        {
            GUILayout.Label(EditorToolsUtility.WindowTitlePrefix + "Audio Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            if (_audioUtilType == null || _playMethod == null)
            {
                EditorGUILayout.HelpBox(
                    "La API interna de previsualización de audio no está disponible en esta versión de Unity.",
                    MessageType.Error);
                return;
            }

            AudioClip previous = _selectedClip;
            _selectedClip = (AudioClip)EditorGUILayout.ObjectField("AudioClip:", _selectedClip, typeof(AudioClip), false);

            if (_selectedClip != previous && _isPlaying)
                StopPreview();

            if (_selectedClip == null)
            {
                EditorGUILayout.HelpBox("Arrastra un AudioClip para previsualizarlo.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);

            GUILayout.Label($"Nombre: {_selectedClip.name}", EditorStyles.miniLabel);
            GUILayout.Label($"Duración: {_selectedClip.length:F2}s | " +
                            $"Frecuencia: {_selectedClip.frequency}Hz | " +
                            $"Canales: {_selectedClip.channels}", EditorStyles.miniLabel);

            EditorGUILayout.Space(8);

            _isLooping = EditorGUILayout.Toggle("Loop", _isLooping);

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            if (!_isPlaying)
            {
                if (GUILayout.Button("▶ Play", GUILayout.Height(30)))
                    PlayPreview();
            }
            else if (GUILayout.Button("⏹ Stop", GUILayout.Height(30)))
            {
                StopPreview();
            }

            if (GUILayout.Button("🔄 Reiniciar", GUILayout.Height(30), GUILayout.Width(90)))
            {
                StopPreview();
                PlayPreview();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            if (_selectedClip.length <= 0f) return;

            float position = GetPlaybackPosition();
            float progress = Mathf.Clamp01(position / _selectedClip.length);

            Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(12));
            EditorGUI.DrawRect(progressRect, new Color(0.15f, 0.15f, 0.15f));
            EditorGUI.DrawRect(
                new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height),
                new Color(0.3f, 0.7f, 1f));

            GUILayout.Label($"{position:F1}s / {_selectedClip.length:F1}s", EditorStyles.centeredGreyMiniLabel);
        }

        private void PlayPreview()
        {
            if (_selectedClip == null || _playMethod == null) return;

            _playMethod.Invoke(null, new object[] { _selectedClip, 0, _isLooping });
            _isPlaying = true;
            _playStartTime = EditorApplication.timeSinceStartup;
        }

        private void StopPreview()
        {
            if (_stopMethod != null) _stopMethod.Invoke(null, null);
            _isPlaying = false;
        }

        private float GetPlaybackPosition()
        {
            if (!_isPlaying || _positionMethod == null) return 0f;

            object position = _positionMethod.Invoke(null, null);
            return position is float value ? value : 0f;
        }

        private void Update()
        {
            if (!_isPlaying) return;

            if (HasPlaybackFinished())
            {
                StopPreview();
                Repaint();
                return;
            }

            Repaint();
        }

        private bool HasPlaybackFinished()
        {
            if (_isLooping || _selectedClip == null) return false;
            if (EditorApplication.timeSinceStartup - _playStartTime < 0.25) return false;

            if (_isPlayingMethod != null && _isPlayingMethod.Invoke(null, null) is bool stillPlaying)
                return !stillPlaying;

            return GetPlaybackPosition() >= _selectedClip.length;
        }
    }
}
