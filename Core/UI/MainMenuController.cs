using UnityEngine;
using UnityEngine.UI;

namespace Infra2DAction
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Botones")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _quitButton;

        [Header("PanelID de los paneles registrados en UICore")]
        [SerializeField] private string _settingsPanelID = "Settings";

        private void Awake()
        {
            if (_continueButton != null) _continueButton.onClick.AddListener(() => OnSaveSlotButton(false));
            if (_newGameButton != null) _newGameButton.onClick.AddListener(() => OnSaveSlotButton(true));
            if (_optionsButton != null) _optionsButton.onClick.AddListener(OnOptionsClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);

            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.MainMenu, "MainMenuController"));
        }

        private void OnSaveSlotButton(bool overwriteMode)
            => EventBus.Raise(new SaveSlotPanelRequestEvent(overwriteMode, "MainMenuController"));

        private void OnOptionsClicked()
            => EventBus.Raise(new UIOpenPanelRequestEvent(_settingsPanelID, "MainMenuController"));

        private void OnQuitClicked()
            => EventBus.Raise(new QuitGameRequestEvent("MainMenuController"));
    }
}
