using UnityEngine;

namespace Infra2DAction
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovementSystem : MonoBehaviour
    {
        [Header("Walk / Run")]
        [SerializeField] private float _walkSpeed = 4f;
        [SerializeField] private float _runSpeed = 8f;
        [SerializeField] private float _runInputThreshold = 0.5f;
        [SerializeField] private float _accelerationTime = 0.08f;
        [SerializeField] private float _walkStopFriction = 15f;
        [SerializeField] private float _runStopFriction = 25f;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 13f;
        [SerializeField] private float _jumpCutMultiplier = 0.5f;
        [SerializeField] private float _coyoteTime = 0.12f;
        [SerializeField] private float _jumpBufferTime = 0.12f;
        [SerializeField] private float _doubleJumpForce = 11f;

        [Header("Gravity")]
        [SerializeField] private float _baseGravity = 50f;
        [SerializeField] private float _fallGravityMultiplier = 1.3f;
        [SerializeField] private float _lowJumpMultiplier = 2f;
        [SerializeField] private float _maxFallSpeed = 18f;

        [Header("Dash")]
        [SerializeField] private float _dashSpeed = 20f;
        [SerializeField] private float _dashDuration = 0.15f;
        [SerializeField] private float _dashCooldown = 0.4f;
        [SerializeField] private float _dashEndDropForce = -1.5f;

        [Header("Wall Jump")]
        [SerializeField] private float _wallSlideSpeed = 2f;
        [SerializeField] private float _wallJumpForce = 13f;
        [SerializeField] private float _wallJumpCooldown = 0.2f;

        [Header("Ground/Wall Detection")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _wallCheckFront;
        [SerializeField] private float _groundCheckRadius = 0.15f;
        [SerializeField] private float _wallCheckDistance = 0.2f;
        [SerializeField] private LayerMask _groundMask;

        private Rigidbody2D _rb;
        private SurfacePhysicsSystem _surfacePhysics;
        private PlayerStateSystem _stateSystem;
        private PlayerAbilityUnlocks _unlocks;
        private PlayerLockCoordinator _lockCoordinator;

        private Vector2 _moveInput;
        private bool _isGrounded;
        private bool _isTouchingWall;
        private int _facingDirection = 1;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _hasDoubleJumped;
        private bool _jumpHeld;

        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private Vector2 _dashDirection;

        private bool _isWallSliding;
        private float _wallJumpCooldownTimer;
        private bool _wallJumpInputReleased = true;

        private bool IsLocked => _lockCoordinator != null && _lockCoordinator.IsMovementLocked();
        private Vector2 EffectiveMoveInput => IsLocked ? Vector2.zero : _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _surfacePhysics = GetComponent<SurfacePhysicsSystem>();
            _stateSystem = GetComponent<PlayerStateSystem>();
            _unlocks = GetComponent<PlayerAbilityUnlocks>();
            _lockCoordinator = GetComponent<PlayerLockCoordinator>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<MoveInputEvent>(OnMoveInput);
            EventBus.Subscribe<JumpPressedEvent>(OnJumpPressed);
            EventBus.Subscribe<JumpReleasedEvent>(OnJumpReleased);
            EventBus.Subscribe<DashPressedEvent>(OnDashPressed);
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MoveInputEvent>(OnMoveInput);
            EventBus.Unsubscribe<JumpPressedEvent>(OnJumpPressed);
            EventBus.Unsubscribe<JumpReleasedEvent>(OnJumpReleased);
            EventBus.Unsubscribe<DashPressedEvent>(OnDashPressed);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void Update()
        {
            UpdateDetection();
            UpdateTimers();
        }

        private void FixedUpdate()
        {
            if (_isDashing)
            {
                ApplyDash();
                return;
            }

            ApplyHorizontalMovement();
            ApplyGravity();
            ApplyWallSlide();
            ApplyDirectionalSurfaceForce();
            ClampFallSpeed();
        }

        public bool CanDashNow()
            => _unlocks != null && _unlocks.CanDash && _dashCooldownTimer <= 0f && !_isDashing &&
               !(_surfacePhysics != null && _surfacePhysics.IsInWater());

        public bool CanDoubleJumpNow()
            => _unlocks != null && _unlocks.CanDoubleJump && !_isGrounded && !_hasDoubleJumped;

        public bool CanWallJumpNow()
            => _unlocks != null && _unlocks.CanWallJump && _isTouchingWall && !_isGrounded;

        private void OnMoveInput(MoveInputEvent e)
        {
            _moveInput = e.Direction;
            if (!IsLocked && Mathf.Abs(_moveInput.x) > 0.01f)
                _facingDirection = _moveInput.x > 0 ? 1 : -1;
        }

        private void OnJumpPressed(JumpPressedEvent e)
        {
            if (IsLocked) return;

            _jumpBufferTimer = _jumpBufferTime;
            _jumpHeld = true;

            if (_surfacePhysics != null && _surfacePhysics.IsInWater())
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce * 0.6f);
        }

        private void OnJumpReleased(JumpReleasedEvent e)
        {
            _jumpHeld = false;

            if (_rb.linearVelocity.y > 0f)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * _jumpCutMultiplier);
        }

        private void OnDashPressed(DashPressedEvent e)
        {
            if (IsLocked || !CanDashNow()) return;
            StartDash();
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            bool frozen = e.NewState == GameStateType.Pause ||
                          e.NewState == GameStateType.Dialogue ||
                          e.NewState == GameStateType.Cutscene ||
                          e.NewState == GameStateType.Menu;

            if (frozen) _rb.linearVelocity = Vector2.zero;
        }

        private void UpdateDetection()
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundMask);

            Vector2 wallCheckDir = new Vector2(_facingDirection, 0);
            _isTouchingWall = Physics2D.Raycast(_wallCheckFront.position, wallCheckDir, _wallCheckDistance, _groundMask);

            if (!wasGrounded && _isGrounded)
            {
                EventBus.Raise(new PlayerLandedEvent(_rb.linearVelocity.y));
                _hasDoubleJumped = false;
                _coyoteTimer = _coyoteTime;
            }

            if (_isGrounded) _coyoteTimer = _coyoteTime;
        }

        private void UpdateTimers()
        {
            if (_coyoteTimer > 0f) _coyoteTimer -= Time.deltaTime;
            if (_jumpBufferTimer > 0f) _jumpBufferTimer -= Time.deltaTime;
            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;
            if (_wallJumpCooldownTimer > 0f) _wallJumpCooldownTimer -= Time.deltaTime;

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f && !_isDashing)
            {
                ExecuteJump(false);
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }
            else if (_jumpBufferTimer > 0f && CanDoubleJumpNow() && _coyoteTimer <= 0f)
            {
                ExecuteJump(true);
                _jumpBufferTimer = 0f;
                _hasDoubleJumped = true;
            }

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f) EndDash();
            }
        }

        private void ApplyHorizontalMovement()
        {
            Vector2 input = EffectiveMoveInput;
            bool isRunning = Mathf.Abs(input.x) > _runInputThreshold;
            float baseSpeed = isRunning ? _runSpeed : _walkSpeed;

            float speedMultiplier = _surfacePhysics != null ? _surfacePhysics.GetMaxSpeedMultiplier() : 1f;
            float targetSpeed = input.x * baseSpeed * speedMultiplier;

            float currentFriction = _surfacePhysics != null ? _surfacePhysics.GetFrictionMultiplier() : 1f;

            if (Mathf.Abs(input.x) < 0.01f)
            {
                float stopFriction = isRunning ? _runStopFriction : _walkStopFriction;
                float newX = Mathf.MoveTowards(_rb.linearVelocity.x, 0f, stopFriction * currentFriction * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
            }
            else
            {
                float accelRate = (baseSpeed * speedMultiplier) / Mathf.Max(_accelerationTime, 0.01f);
                float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
            }

            EventBus.Raise(new PlayerMovedEvent(_rb.linearVelocity, isRunning));
        }

        private void ApplyGravity()
        {
            if (_isGrounded) return;

            float gravityMultiplier = _surfacePhysics != null ? _surfacePhysics.GetGravityMultiplier() : 1f;

            if (_rb.linearVelocity.y < 0f)
            {
                float gravity = _baseGravity * _fallGravityMultiplier * gravityMultiplier;
                _rb.linearVelocity += Vector2.down * gravity * Time.fixedDeltaTime;

                EventBus.Raise(new PlayerFallingEvent());
            }
            else if (_rb.linearVelocity.y > 0f && !_jumpHeld)
            {
                float gravity = _baseGravity * _lowJumpMultiplier * gravityMultiplier;
                _rb.linearVelocity += Vector2.down * gravity * Time.fixedDeltaTime;
            }
            else
            {
                float gravity = _baseGravity * gravityMultiplier;
                _rb.linearVelocity += Vector2.down * gravity * Time.fixedDeltaTime;
            }
        }

        private void ClampFallSpeed()
        {
            if (_rb.linearVelocity.y < -_maxFallSpeed)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -_maxFallSpeed);
        }

        private void ExecuteJump(bool isDoubleJump)
        {
            float force = isDoubleJump ? _doubleJumpForce : _jumpForce;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, force);

            EventBus.Raise(new PlayerJumpedEvent(isDoubleJump));
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = _dashDuration;
            _dashCooldownTimer = _dashCooldown;

            Vector2 input = EffectiveMoveInput;
            Vector2 dir = input.sqrMagnitude > 0.01f ? input.normalized : new Vector2(_facingDirection, 0f);
            _dashDirection = dir;

            EventBus.Raise(new PlayerDashedEvent(_dashDirection));
        }

        private void ApplyDash()
        {
            _rb.linearVelocity = _dashDirection * _dashSpeed;
        }

        private void EndDash()
        {
            _isDashing = false;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _dashEndDropForce);
        }

        private void ApplyWallSlide()
        {
            Vector2 input = EffectiveMoveInput;
            bool wantsWallSlide = _isTouchingWall && !_isGrounded && _rb.linearVelocity.y < 0f;
            bool pressingTowardsWall = Mathf.Sign(input.x) == _facingDirection && Mathf.Abs(input.x) > 0.1f;

            _isWallSliding = wantsWallSlide && pressingTowardsWall;

            if (_isWallSliding)
            {
                _rb.linearVelocity = new Vector2(0f, -_wallSlideSpeed);
                _stateSystem?.TransitionTo(PlayerStateType.WallSliding);
                _wallJumpInputReleased = false;
            }
            else if (!pressingTowardsWall)
            {
                _wallJumpInputReleased = true;
            }

            if (_isWallSliding && _wallJumpInputReleased && _jumpBufferTimer > 0f &&
                _wallJumpCooldownTimer <= 0f && CanWallJumpNow())
            {
                ExecuteWallJump();
            }
        }

        private void ExecuteWallJump()
        {
            _rb.linearVelocity = new Vector2(0f, _wallJumpForce);
            _wallJumpCooldownTimer = _wallJumpCooldown;
            _jumpBufferTimer = 0f;
            _isWallSliding = false;

            EventBus.Raise(new PlayerWallJumpEvent());
        }

        private void ApplyDirectionalSurfaceForce()
        {
            if (_surfacePhysics == null) return;

            Vector2 force = _surfacePhysics.GetDirectionalForce();
            if (force != Vector2.zero)
                _rb.linearVelocity += force * Time.fixedDeltaTime;
        }
    }
}
