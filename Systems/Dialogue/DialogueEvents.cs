using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class DialogueStartRequestEvent : BaseEvent
    {
        public DialogueData Data { get; private set; }

        public DialogueStartRequestEvent(DialogueData data, string sourceID = "Unknown")
            : base(sourceID)
        {
            Data = data;
        }
    }

    public class DialogueStartedEvent : BaseEvent
    {
        public DialogueStartedEvent(string sourceID = "DialogueSystem") : base(sourceID) { }
    }

    public class DialogueEndedEvent : BaseEvent
    {
        public DialogueEndedEvent(string sourceID = "DialogueSystem") : base(sourceID) { }
    }

    public class DialogueLineShownEvent : BaseEvent
    {
        public string CharacterID { get; private set; }
        public string FullText { get; private set; }
        public Sprite Portrait { get; private set; }

        public DialogueLineShownEvent(string characterID, string fullText, Sprite portrait, string sourceID = "DialogueSystem")
            : base(sourceID)
        {
            CharacterID = characterID;
            FullText = fullText;
            Portrait = portrait;
        }
    }

    public class DialogueChoicesShownEvent : BaseEvent
    {
        public List<string> ChoiceTexts { get; private set; }

        public DialogueChoicesShownEvent(List<string> choiceTexts, string sourceID = "DialogueSystem")
            : base(sourceID)
        {
            ChoiceTexts = choiceTexts;
        }
    }

    public class DialogueChoiceSelectedEvent : BaseEvent
    {
        public int ChoiceIndex { get; private set; }

        public DialogueChoiceSelectedEvent(int choiceIndex, string sourceID = "UI")
            : base(sourceID)
        {
            ChoiceIndex = choiceIndex;
        }
    }
}
