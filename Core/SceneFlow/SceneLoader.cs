using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infra2DAction
{
    public class SceneLoader
    {
        private readonly string _protectedSceneName;

        public SceneLoader(string protectedSceneName)
        {
            _protectedSceneName = protectedSceneName;
        }

        public IEnumerator LoadScene(string sceneName, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                DebugSystem.LogError("Nombre de escena vacio.", "SceneFlow", "SceneLoader");
                yield break;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (operation == null)
            {
                DebugSystem.LogError(
                    $"No se pudo cargar la escena '{sceneName}': ¿esta anadida en Build Settings? Revisa el nombre exacto.",
                    "SceneFlow", "SceneLoader");
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
                yield return null;

            operation.allowSceneActivation = true;

            yield return null;

            onComplete?.Invoke();
        }

        public IEnumerator UnloadScene(string sceneName, Action onComplete = null)
        {
            if (sceneName == _protectedSceneName)
            {
                DebugSystem.LogError($"Intento de descargar la escena persistente '{sceneName}' bloqueado.", "SceneFlow", "SceneLoader");
                yield break;
            }

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                DebugSystem.LogWarning($"Escena {sceneName} no estaba cargada.", "SceneFlow", "SceneLoader");
                yield break;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

            while (!operation.isDone)
                yield return null;

            onComplete?.Invoke();
        }
    }
}
