using UnityEngine;

namespace Infra2DAction
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator _animator;

        [Header("Identity")]
        [SerializeField] private CharacterAnimationID _characterID;

        [Header("Crossfade")]
        [SerializeField] private float _crossfadeDuration = 0.1f;

        private bool _isCutsceneOverride;
        private bool _isShieldBlocking;

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerMovedEvent>(OnMoved);
            EventBus.Subscribe<PlayerJumpedEvent>(OnJumped);
            EventBus.Subscribe<PlayerDashedEvent>(OnDashed);
            EventBus.Subscribe<PlayerLandedEvent>(OnLanded);
            EventBus.Subscribe<PlayerWallJumpEvent>(OnWallJump);
            EventBus.Subscribe<PlayerFallingEvent>(OnFalling);

            EventBus.Subscribe<PlayerAttackEvent>(OnAttack);
            EventBus.Subscribe<ShieldActivatedEvent>(OnShieldActivated);
            EventBus.Subscribe<ShieldDeactivatedEvent>(OnShieldDeactivated);
            EventBus.Subscribe<ShieldBrokenEvent>(OnShieldBroken);
            EventBus.Subscribe<PlayerHealedEvent>(OnHealed);
            EventBus.Subscribe<EnemyPacifiedEvent>(OnPacified);
            EventBus.Subscribe<PlayerDamagedEvent>(OnDamaged);
            EventBus.Subscribe<PlayerDeathEvent>(OnDeath);

            EventBus.Subscribe<ResourceCollectedEvent>(OnResourceCollected);

            EventBus.Subscribe<PlayerEnteredWaterEvent>(OnEnteredWater);
            EventBus.Subscribe<PlayerExitedWaterEvent>(OnExitedWater);

            EventBus.Subscribe<CutsceneAnimationOverrideEvent>(OnCutsceneOverride);
            EventBus.Subscribe<CutsceneForceAnimationEvent>(OnCutsceneForceAnimation);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerMovedEvent>(OnMoved);
            EventBus.Unsubscribe<PlayerJumpedEvent>(OnJumped);
            EventBus.Unsubscribe<PlayerDashedEvent>(OnDashed);
            EventBus.Unsubscribe<PlayerLandedEvent>(OnLanded);
            EventBus.Unsubscribe<PlayerWallJumpEvent>(OnWallJump);
            EventBus.Unsubscribe<PlayerFallingEvent>(OnFalling);

            EventBus.Unsubscribe<PlayerAttackEvent>(OnAttack);
            EventBus.Unsubscribe<ShieldActivatedEvent>(OnShieldActivated);
            EventBus.Unsubscribe<ShieldDeactivatedEvent>(OnShieldDeactivated);
            EventBus.Unsubscribe<ShieldBrokenEvent>(OnShieldBroken);
            EventBus.Unsubscribe<PlayerHealedEvent>(OnHealed);
            EventBus.Unsubscribe<EnemyPacifiedEvent>(OnPacified);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnDamaged);
            EventBus.Unsubscribe<PlayerDeathEvent>(OnDeath);

            EventBus.Unsubscribe<ResourceCollectedEvent>(OnResourceCollected);

            EventBus.Unsubscribe<PlayerEnteredWaterEvent>(OnEnteredWater);
            EventBus.Unsubscribe<PlayerExitedWaterEvent>(OnExitedWater);

            EventBus.Unsubscribe<CutsceneAnimationOverrideEvent>(OnCutsceneOverride);
            EventBus.Unsubscribe<CutsceneForceAnimationEvent>(OnCutsceneForceAnimation);
        }

        private bool CanPlayNormalAnimation() => _animator != null && !_isCutsceneOverride && !_isShieldBlocking;

        private void PlayCrossfade(string stateName)
        {
            if (_animator == null || string.IsNullOrEmpty(stateName)) return;
            _animator.CrossFade(stateName, _crossfadeDuration);
        }

        private void OnMoved(PlayerMovedEvent e)
        {
            if (!CanPlayNormalAnimation()) return;

            _animator.SetFloat("Speed", Mathf.Abs(e.Velocity.x));
            _animator.SetFloat("VerticalVelocity", e.Velocity.y);
            _animator.SetBool("IsRunning", e.IsRunning);
            _animator.SetBool("IsWalking", Mathf.Abs(e.Velocity.x) > 0.01f && !e.IsRunning);

            if (Mathf.Abs(e.Velocity.x) > 0.01f)
            {
                float direction = Mathf.Sign(e.Velocity.x);
                transform.localScale = new Vector3(direction, 1f, 1f);
            }
        }

        private void OnJumped(PlayerJumpedEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            PlayCrossfade(e.IsDoubleJump ? "DoubleJump" : "Jump");
        }

        private void OnDashed(PlayerDashedEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            PlayCrossfade("Dash");
        }

        private void OnLanded(PlayerLandedEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            _animator.SetBool("IsGrounded", true);
            PlayCrossfade("Land");
        }

        private void OnFalling(PlayerFallingEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            _animator.SetBool("IsGrounded", false);
            PlayCrossfade("Fall");
        }

        private void OnWallJump(PlayerWallJumpEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            PlayCrossfade("WallJump");
        }

        private void OnAttack(PlayerAttackEvent e)
        {
            if (!CanPlayNormalAnimation()) return;

            string state = e.Variant switch
            {
                AttackVariant.Up => "AttackUp",
                AttackVariant.Down => "AttackDown",
                AttackVariant.Air => "AttackAir",
                _ => "AttackNormal"
            };
            PlayCrossfade(state);
        }

        private void OnShieldActivated(ShieldActivatedEvent e)
        {
            if (_animator == null) return;
            _isShieldBlocking = true;
            PlayCrossfade("Shield_Activate");
            _animator.SetBool("ShieldHold", true);
        }

        private void OnShieldDeactivated(ShieldDeactivatedEvent e)
        {
            if (_animator == null) return;
            _isShieldBlocking = false;
            _animator.SetBool("ShieldHold", false);
        }

        private void OnShieldBroken(ShieldBrokenEvent e)
        {
            if (_animator == null) return;
            _isShieldBlocking = false;
            _animator.SetBool("ShieldHold", false);
            PlayCrossfade("Shield_Break");
        }

        private void OnHealed(PlayerHealedEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            PlayCrossfade("Heal");
        }

        private void OnPacified(EnemyPacifiedEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            PlayCrossfade("Pacify");
        }

        private void OnDamaged(PlayerDamagedEvent e)
        {
            if (_animator == null) return;
            _isShieldBlocking = false;
            _isCutsceneOverride = false;
            _animator.SetBool("ShieldHold", false);

            PlayCrossfade("Hit");
        }

        private void OnDeath(PlayerDeathEvent e)
        {
            PlayCrossfade("Death");
        }

        private void OnResourceCollected(ResourceCollectedEvent e)
        {
            if (!CanPlayNormalAnimation()) return;
            PlayCrossfade("ResourceCollect");
        }

        private void OnEnteredWater(PlayerEnteredWaterEvent e)
        {
            if (_animator != null) _animator.SetBool("IsSwimming", true);
        }

        private void OnExitedWater(PlayerExitedWaterEvent e)
        {
            if (_animator != null) _animator.SetBool("IsSwimming", false);
        }

        private void OnCutsceneOverride(CutsceneAnimationOverrideEvent e)
        {
            _isCutsceneOverride = e.IsActive;
        }

        private void OnCutsceneForceAnimation(CutsceneForceAnimationEvent e)
        {
            if (_characterID == null || e.CharacterID != _characterID.CharacterID) return;

            PlayCrossfade(e.AnimationTrigger);
        }
    }
}
