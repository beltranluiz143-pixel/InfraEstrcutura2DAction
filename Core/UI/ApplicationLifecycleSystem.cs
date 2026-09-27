using UnityEngine;

namespace Infra2DAction
{
    public class ApplicationLifecycleSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<QuitGameRequestEvent>(OnQuitRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<QuitGameRequestEvent>(OnQuitRequested);
        }

        private void OnQuitRequested(QuitGameRequestEvent e)
        {
            DebugSystem.Log("Quit game requested.", "Application", "ApplicationLifecycleSystem");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
