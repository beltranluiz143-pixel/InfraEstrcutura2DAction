using UnityEngine;

namespace Infra2DAction
{
    public class NPCInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private NPCData _npcData;

        public void OnInteract()
        {
            if (_npcData == null)
            {
                DebugSystem.LogWarning("NPCInteractable sin NPCData asignado.", "Interaction", "NPCInteractable");
                return;
            }

            EventBus.Raise(new NPCInteractionRequestEvent(_npcData, "NPCInteractable"));
        }
    }
}
