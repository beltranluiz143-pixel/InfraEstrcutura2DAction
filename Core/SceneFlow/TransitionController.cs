using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Infra2DAction
{
    public class TransitionController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _fadeDuration = 0.5f;

        [Header("References")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private Image _fadeImage;

        private void Awake()
        {
            if (_fadeCanvasGroup == null || _fadeImage == null)
            {
                DebugSystem.LogError("FadeCanvasGroup o FadeImage no asignados.", "SceneFlow", "TransitionController");
                return;
            }

            _fadeImage.color = Color.black;
            _fadeCanvasGroup.alpha = 1f;
        }

        public IEnumerator FadeOut(Color color, Action onComplete = null)
        {
            if (_fadeImage != null) _fadeImage.color = color;

            yield return Fade(0f, 1f);
            onComplete?.Invoke();
        }

        public IEnumerator FadeIn(Action onComplete = null)
        {
            yield return Fade(1f, 0f);
            onComplete?.Invoke();
        }

        private IEnumerator Fade(float from, float to)
        {
            if (_fadeCanvasGroup == null) yield break;

            float elapsed = 0f;
            _fadeCanvasGroup.alpha = from;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                yield return null;
            }

            _fadeCanvasGroup.alpha = to;
        }
    }
}
