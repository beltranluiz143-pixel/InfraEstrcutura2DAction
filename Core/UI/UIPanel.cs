using UnityEngine;
using UnityEngine.EventSystems;

namespace Infra2DAction
{
    public class UIPanel : MonoBehaviour
    {
        [Header("Identification")]
        [SerializeField] private string _panelID;

        [Header("Audio (opcional)")]
        [SerializeField] private string _openSoundID = "SFX_UI_OPEN";
        [SerializeField] private string _closeSoundID = "SFX_UI_CLOSE";

        [Header("Navigation")]
        [SerializeField] protected GameObject _firstSelectedButton;

        public string PanelID => _panelID;
        public bool IsOpen { get; private set; }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            IsOpen = true;

            EventBus.Raise(new UIPanelOpenedEvent(_panelID));

            if (!string.IsNullOrEmpty(_openSoundID))
                EventBus.Raise(new SFXPlayRequestEvent(_openSoundID, transform.position, _panelID));

            SelectFirstButton();
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
            IsOpen = false;

            EventBus.Raise(new UIPanelClosedEvent(_panelID));

            if (!string.IsNullOrEmpty(_closeSoundID))
                EventBus.Raise(new SFXPlayRequestEvent(_closeSoundID, transform.position, _panelID));
        }

        public virtual void Show() => Open();
        public virtual void Hide() => Close();

        protected void SelectFirstButton()
        {
            if (_firstSelectedButton == null) return;
            EventSystem.current?.SetSelectedGameObject(null);
            EventSystem.current?.SetSelectedGameObject(_firstSelectedButton);
        }
    }
}
