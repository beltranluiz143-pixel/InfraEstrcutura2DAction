namespace Infra2DAction
{
    public class MapPinsChangedEvent : BaseEvent
    {
        public MapPinsChangedEvent(string sourceID = "MapSystem") : base(sourceID) { }
    }
}
