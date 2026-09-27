using UnityEngine;
using UnityEngine.UI;

namespace Infra2DAction
{
    public class PausePanel : UIPanel
    {
        [Header("Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _quitButton;

        [Header("Navigation")]
        [SerializeField] private string _settingsPanelID = "Settings";
        [SerializeField] private SceneReference _mainMenuScene;

        private void Awake()
        {
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinueClicked);
            if (_optionsButton != null) _optionsButton.onClick.AddListener(OnOptionsClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnContinueClicked()
        {
            EventBus.Raise(new PausePressedEvent("PausePanel"));
        }

        private void OnOptionsClicked()
        {
            EventBus.Raise(new UIOpenPanelRequestEvent(_settingsPanelID, "PausePanel"));
        }

        private void OnQuitClicked()
        {
            if (_mainMenuScene != null)
                EventBus.Raise(new SceneTransitionRequestEvent(_mainMenuScene, _mainMenuScene.DefaultSpawnPointID,
                                                               Color.black, "PausePanel"));
            else
                EventBus.Raise(new QuitGameRequestEvent("PausePanel"));
        }
    }
}
