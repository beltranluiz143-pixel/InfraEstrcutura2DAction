using UnityEngine;

namespace Infra2DAction
{
    public class CharacterAnimationID : MonoBehaviour
    {
        [SerializeField] private string _characterID;
        public string CharacterID => _characterID;
    }
}
