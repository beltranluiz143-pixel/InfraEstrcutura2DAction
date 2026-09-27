using UnityEngine;

namespace Infra2DAction
{
    public class PlayerRegisteredEvent : BaseEvent
    {
        public GameObject PlayerObject { get; private set; }

        public PlayerRegisteredEvent(GameObject playerObject, string sourceID = "PlayerSystem")
            : base(sourceID)
        {
            PlayerObject = playerObject;
        }
    }
}
