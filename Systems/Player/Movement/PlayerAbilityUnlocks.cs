using UnityEngine;

namespace Infra2DAction
{
    public class PlayerAbilityUnlocks : MonoBehaviour
    {
        [Header("Ability Flag Keys")]
        [SerializeField] private string _dashFlagKey = "ABILITY_DASH_UNLOCKED";
        [SerializeField] private string _doubleJumpFlagKey = "ABILITY_DOUBLEJUMP_UNLOCKED";
        [SerializeField] private string _wallJumpFlagKey = "ABILITY_WALLJUMP_UNLOCKED";

        private GlobalVariablesSystem _globalVars;

        public void Initialize(GlobalVariablesSystem globalVars)
        {
            _globalVars = globalVars;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AbilityUnlockRequestEvent>(OnAbilityUnlockRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AbilityUnlockRequestEvent>(OnAbilityUnlockRequested);
        }

        private void OnAbilityUnlockRequested(AbilityUnlockRequestEvent e)
        {
            _globalVars?.SetFlag(e.AbilityFlagKey, true);
        }

        public bool CanDash => _globalVars != null && _globalVars.GetFlag(_dashFlagKey);
        public bool CanDoubleJump => _globalVars != null && _globalVars.GetFlag(_doubleJumpFlagKey);
        public bool CanWallJump => _globalVars != null && _globalVars.GetFlag(_wallJumpFlagKey);
    }
}
