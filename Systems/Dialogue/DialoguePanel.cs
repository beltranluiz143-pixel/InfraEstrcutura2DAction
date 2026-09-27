using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Infra2DAction
{
    public class DialoguePanel : UIPanel
    {
        [Header("Text & Portrait")]
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private Image _portraitImage;

        [Header("Choices")]
        [SerializeField] private GameObject _choiceButtonPrefab;
        [SerializeField] private Transform _choiceContainer;

        [Header("Audio")]
        [SerializeField] private string _sfxOnLineShown = "SFX_DIALOGUE_BLIP";

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueLineShownEvent>(OnLineShown);
            EventBus.Subscribe<DialogueChoicesShownEvent>(OnChoicesShown);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueLineShownEvent>(OnLineShown);
            EventBus.Unsubscribe<DialogueChoicesShownEvent>(OnChoicesShown);
        }

        private void OnLineShown(DialogueLineShownEvent e)
        {
            if (_dialogueText != null) _dialogueText.text = e.FullText;

            if (_portraitImage != null)
            {
                bool hasPortrait = e.Portrait != null;
                _portraitImage.gameObject.SetActive(hasPortrait);
                if (hasPortrait) _portraitImage.sprite = e.Portrait;
            }

            if (!string.IsNullOrEmpty(_sfxOnLineShown))
                EventBus.Raise(new SFXPlayRequestEvent(_sfxOnLineShown, transform.position, "DialoguePanel"));

            ClearChoices();
        }

        private void OnChoicesShown(DialogueChoicesShownEvent e)
        {
            ClearChoices();

            if (_choiceButtonPrefab == null || _choiceContainer == null) return;

            Button firstButton = null;

            for (int i = 0; i < e.ChoiceTexts.Count; i++)
            {
                int index = i;
                GameObject buttonObject = Instantiate(_choiceButtonPrefab, _choiceContainer);

                TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
                if (buttonText != null) buttonText.text = e.ChoiceTexts[i];

                Button button = buttonObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() =>
                        EventBus.Raise(new DialogueChoiceSelectedEvent(index, "DialoguePanel")));

                    if (firstButton == null) firstButton = button;
                }
            }

            if (firstButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            }
        }

        private void ClearChoices()
        {
            if (_choiceContainer == null) return;

            foreach (Transform child in _choiceContainer)
                Destroy(child.gameObject);
        }
    }
}
