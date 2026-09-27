namespace Infra2DAction
{
    public class SurfaceChangedEvent : BaseEvent
    {
        public string PreviousSurfaceID { get; private set; }
        public string NewSurfaceID { get; private set; }

        public SurfaceChangedEvent(string previous, string newSurface, string sourceID = "SurfacePhysicsSystem")
            : base(sourceID)
        {
            PreviousSurfaceID = previous;
            NewSurfaceID = newSurface;
        }
    }

    public class PlayerEnteredWaterEvent : BaseEvent
    {
        public PlayerEnteredWaterEvent(string sourceID = "SurfacePhysicsSystem") : base(sourceID) { }
    }

    public class PlayerExitedWaterEvent : BaseEvent
    {
        public PlayerExitedWaterEvent(string sourceID = "SurfacePhysicsSystem") : base(sourceID) { }
    }
}
