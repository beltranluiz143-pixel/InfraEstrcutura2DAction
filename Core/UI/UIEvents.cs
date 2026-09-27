namespace Infra2DAction
{
    public class UIPanelOpenedEvent : BaseEvent
    {
        public string PanelID { get; private set; }

        public UIPanelOpenedEvent(string panelID, string sourceID = "UICore")
            : base(sourceID)
        {
            PanelID = panelID;
        }
    }

    public class UIPanelClosedEvent : BaseEvent
    {
        public string PanelID { get; private set; }

        public UIPanelClosedEvent(string panelID, string sourceID = "UICore")
            : base(sourceID)
        {
            PanelID = panelID;
        }
    }

    public class UIOpenPanelRequestEvent : BaseEvent
    {
        public string PanelID { get; private set; }

        public UIOpenPanelRequestEvent(string panelID, string sourceID = "Unknown")
            : base(sourceID)
        {
            PanelID = panelID;
        }
    }

    public class UIClosePanelRequestEvent : BaseEvent
    {
        public UIClosePanelRequestEvent(string sourceID = "Unknown") : base(sourceID) { }
    }

    public class UIPanelCloseRequestEvent : BaseEvent
    {
        public string PanelID { get; private set; }

        public UIPanelCloseRequestEvent(string panelID, string sourceID = "Unknown")
            : base(sourceID)
        {
            PanelID = panelID;
        }
    }

    public class QuitGameRequestEvent : BaseEvent
    {
        public QuitGameRequestEvent(string sourceID = "PausePanel") : base(sourceID) { }
    }
}
