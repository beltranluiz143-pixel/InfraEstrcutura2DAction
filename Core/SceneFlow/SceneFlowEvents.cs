using UnityEngine;

namespace Infra2DAction
{
    public class SceneTransitionRequestEvent : BaseEvent
    {
        public SceneReference TargetScene { get; private set; }
        public string SpawnPointID { get; private set; }
        public Color FadeColor { get; private set; }

        public SceneTransitionRequestEvent(SceneReference targetScene,
                                           string spawnPointID = "Default",
                                           Color fadeColor = default,
                                           string sourceID = "Unknown")
            : base(sourceID)
        {
            TargetScene = targetScene;
            SpawnPointID = spawnPointID;
            FadeColor = fadeColor == default ? Color.black : fadeColor;
        }
    }

    public class StartGameRequestEvent : BaseEvent
    {
        public SceneReference NewGameScene { get; private set; }

        public StartGameRequestEvent(SceneReference newGameScene, string sourceID = "Unknown")
            : base(sourceID)
        {
            NewGameScene = newGameScene;
        }
    }

    public class SceneLoadedEvent : BaseEvent
    {
        public string SceneName { get; private set; }

        public SceneLoadedEvent(string sceneName, string sourceID = "SceneFlowSystem")
            : base(sourceID)
        {
            SceneName = sceneName;
        }
    }

    public class SceneTransitionStartedEvent : BaseEvent
    {
        public SceneTransitionStartedEvent(string sourceID = "SceneFlowSystem")
            : base(sourceID) { }
    }

    public class SceneUnloadedEvent : BaseEvent
    {
        public string SceneName { get; private set; }

        public SceneUnloadedEvent(string sceneName, string sourceID = "SceneFlowSystem")
            : base(sourceID)
        {
            SceneName = sceneName;
        }
    }
}
