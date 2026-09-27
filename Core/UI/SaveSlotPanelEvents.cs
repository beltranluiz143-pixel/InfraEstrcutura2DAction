namespace Infra2DAction
{
    public class SaveSlotPanelRequestEvent : BaseEvent
    {
        public bool OverwriteMode { get; private set; }

        public SaveSlotPanelRequestEvent(bool overwriteMode, string sourceID = "MainMenuController")
            : base(sourceID)
        {
            OverwriteMode = overwriteMode;
        }
    }
}
