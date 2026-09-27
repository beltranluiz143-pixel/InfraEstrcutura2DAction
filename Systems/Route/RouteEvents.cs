namespace Infra2DAction
{
    public class RouteChangedEvent : BaseEvent
    {
        public string PreviousRouteID { get; private set; }
        public string NewRouteID { get; private set; }

        public RouteChangedEvent(string previousRouteID, string newRouteID, string sourceID = "RouteSystem")
            : base(sourceID)
        {
            PreviousRouteID = previousRouteID;
            NewRouteID = newRouteID;
        }
    }

    public class RouteActionTriggeredEvent : BaseEvent
    {
        public RouteActionData ActionData { get; private set; }

        public RouteActionTriggeredEvent(RouteActionData actionData, string sourceID = "Unknown")
            : base(sourceID)
        {
            ActionData = actionData;
        }
    }
}
