using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class DialogueSystem : MonoBehaviour
    {
        [Header("Typewriter")]
        [SerializeField] private float _baseCharDelay = 0.03f;
        [SerializeField] private float _advanceDelay = 0.2f;

        [Header("Default Panel ID")]
        [SerializeField] private string _defaultDialoguePanelID = "Dialogue";

        private DialogueData _currentDialogue;
        private int _currentLineIndex;
        private bool _isTyping;
        private bool _canAdvance;
        private bool _waitingForChoice;
        private float _dialogueSpeedMultiplier = 1f;
        private string _activePanelID;

        private List<DialogueData.DialogueChoice> _activeChoices;

        private GlobalVariablesSystem _globalVars;

        public void Initialize(GlobalVariablesSystem globalVars)
        {
            _globalVars = globalVars;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueStartRequestEvent>(OnDialogueStartRequested);
            EventBus.Subscribe<DialogueAdvanceEvent>(OnAdvancePressed);
            EventBus.Subscribe<DialogueChoiceSelectedEvent>(OnChoiceSelected);
            EventBus.Subscribe<AccessibilitySettingsChangedEvent>(OnAccessibilityChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueStartRequestEvent>(OnDialogueStartRequested);
            EventBus.Unsubscribe<DialogueAdvanceEvent>(OnAdvancePressed);
            EventBus.Unsubscribe<DialogueChoiceSelectedEvent>(OnChoiceSelected);
            EventBus.Unsubscribe<AccessibilitySettingsChangedEvent>(OnAccessibilityChanged);
        }

        private void OnAccessibilityChanged(AccessibilitySettingsChangedEvent e)
        {
            _dialogueSpeedMultiplier = e.DialogueSpeed;
        }

        private void OnDialogueStartRequested(DialogueStartRequestEvent e)
        {
            if (_currentDialogue != null) return;

            if (e.Data == null || e.Data.Lines.Count == 0)
            {
                DebugSystem.LogWarning("DialogueData vacio o nulo.", "Dialogue", "DialogueSystem");
                return;
            }

            _currentDialogue = e.Data;
            _currentLineIndex = 0;
            _waitingForChoice = false;
            _canAdvance = false;

            _activePanelID = string.IsNullOrEmpty(e.Data.DialoguePanelID)
                ? _defaultDialoguePanelID
                : e.Data.DialoguePanelID;

            EventBus.Raise(new UIOpenPanelRequestEvent(_activePanelID, "DialogueSystem"));
            EventBus.Raise(new DialogueStartedEvent());

            ShowLine(_currentLineIndex);
        }

        private void ShowLine(int index)
        {
            if (index >= _currentDialogue.Lines.Count)
            {
                EndDialogue();
                return;
            }

            DialogueData.DialogueLine line = ResolveLine(_currentDialogue.Lines[index]);

            StopAllCoroutines();
            StartCoroutine(TypeLine(line));
        }

        private DialogueData.DialogueLine ResolveLine(DialogueData.DialogueLine line)
        {
            if (line.Condition == null) return line;

            bool conditionMet = _globalVars != null && _globalVars.EvaluateCondition(line.Condition);

            if (!conditionMet && line.AlternativeLine != null)
                return line.AlternativeLine;

            return line;
        }

        private IEnumerator TypeLine(DialogueData.DialogueLine line)
        {
            _isTyping = true;
            _canAdvance = false;

            string accumulated = "";
            float delay = _baseCharDelay / Mathf.Max(_dialogueSpeedMultiplier, 0.1f);

            foreach (char c in line.Text)
            {
                accumulated += c;
                EventBus.Raise(new DialogueLineShownEvent(line.CharacterID, accumulated, line.Portrait));
                yield return new WaitForSeconds(delay);
            }

            _isTyping = false;

            yield return new WaitForSeconds(_advanceDelay);
            _canAdvance = true;

            if (line.Choices != null && line.Choices.Count > 0)
                ShowChoices(line.Choices);
        }

        private void ShowChoices(List<DialogueData.DialogueChoice> choices)
        {
            _waitingForChoice = true;
            _activeChoices = choices;

            List<string> texts = new List<string>();
            foreach (var choice in choices) texts.Add(choice.Text);

            EventBus.Raise(new DialogueChoicesShownEvent(texts));
        }

        private void OnAdvancePressed(DialogueAdvanceEvent e)
        {
            if (_currentDialogue == null || _waitingForChoice) return;

            if (_isTyping)
            {
                StopAllCoroutines();
                CompleteCurrentLineInstantly();
                return;
            }

            if (!_canAdvance) return;

            _currentLineIndex++;
            ShowLine(_currentLineIndex);
        }

        private void CompleteCurrentLineInstantly()
        {
            DialogueData.DialogueLine line = ResolveLine(_currentDialogue.Lines[_currentLineIndex]);

            EventBus.Raise(new DialogueLineShownEvent(line.CharacterID, line.Text, line.Portrait));
            _isTyping = false;
            _canAdvance = true;

            if (line.Choices != null && line.Choices.Count > 0)
                ShowChoices(line.Choices);
        }

        private void OnChoiceSelected(DialogueChoiceSelectedEvent e)
        {
            if (_currentDialogue == null || !_waitingForChoice || _activeChoices == null) return;
            if (e.ChoiceIndex < 0 || e.ChoiceIndex >= _activeChoices.Count) return;

            DialogueData.DialogueChoice choice = _activeChoices[e.ChoiceIndex];
            ApplyDecision(choice);

            _waitingForChoice = false;
            _activeChoices = null;

            int nextIndex = choice.NextLineIndex >= 0 ? choice.NextLineIndex : _currentLineIndex + 1;
            _currentLineIndex = nextIndex;
            ShowLine(_currentLineIndex);
        }

        private void ApplyDecision(DialogueData.DialogueChoice choice)
        {
            int amount = Mathf.Max(1, Mathf.RoundToInt(choice.DecisionAmount));

            switch (choice.Decision)
            {
                case DialogueData.DecisionType.AddItem:
                    if (!string.IsNullOrEmpty(choice.DecisionTargetID))
                        for (int i = 0; i < amount; i++)
                            EventBus.Raise(new ItemCollectedEvent(choice.DecisionTargetID, "DialogueSystem"));
                    break;

                case DialogueData.DecisionType.RemoveItem:
                    if (!string.IsNullOrEmpty(choice.DecisionTargetID))
                        EventBus.Raise(new ItemRemoveRequestEvent(choice.DecisionTargetID, amount, "DialogueSystem"));
                    break;

                case DialogueData.DecisionType.GiveMoney:
                    EventBus.Raise(new CurrencyAddRequestEvent(Mathf.RoundToInt(choice.DecisionAmount), "DialogueSystem"));
                    break;

                case DialogueData.DecisionType.RouteEffect:
                    if (choice.RouteAction != null)
                        EventBus.Raise(new RouteActionTriggeredEvent(choice.RouteAction, "DialogueSystem"));
                    break;

                case DialogueData.DecisionType.NpcHelped:
                    if (!string.IsNullOrEmpty(choice.DecisionTargetID))
                        EventBus.Raise(new NpcHelpedEvent(choice.DecisionTargetID, "DialogueSystem"));
                    break;
            }
        }

        private void EndDialogue()
        {
            if (!string.IsNullOrEmpty(_activePanelID))
                EventBus.Raise(new UIPanelCloseRequestEvent(_activePanelID, "DialogueSystem"));

            _currentDialogue = null;
            _activePanelID = null;
            _canAdvance = false;
            _isTyping = false;
            _waitingForChoice = false;

            EventBus.Raise(new DialogueEndedEvent());
        }
    }
}
