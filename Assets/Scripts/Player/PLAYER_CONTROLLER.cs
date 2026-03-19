using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple third-person player controller: WASD (camera-relative), Shift to run, Space to jump.
/// Uses CharacterController and drives Animator (Speed, IsGrounded, MovementState, MovementX/Z).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PLAYER_CONTROLLER : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    [Header("Gravity")]
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Jump")]
    [Tooltip("Allow jump when grounded (Spacebar).")]
    [SerializeField] private bool enableJump = true;
    [Tooltip("Jump height in meters. Initial upward velocity is derived from this and gravity.")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Animation")]
    [Tooltip("Optional. Auto-finds on this object or in children if not set.")]
    [SerializeField] private Animator animator;

    [Header("Input Mode")]
    [Tooltip("When enabled, use mouse delta instead of keyboard movement input.")]
    [SerializeField] private bool useMouseInput = false;
    [Tooltip("Horizontal mouse sensitivity for strafing movement.")]
    [SerializeField] private float mouseXSensitivity = 1f;
    [Tooltip("Vertical mouse sensitivity for forward/back movement.")]
    [SerializeField] private float mouseYSensitivity = 1f;
    [Tooltip("Invert vertical (forward/back) movement input.")]
    [SerializeField] private bool invertVerticalMovement = false;

    [Header("Debug")]
    [Tooltip("Log animator status and parameter values to Console (throttled when moving).")]
    [SerializeField] private bool debugAnimator = true;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int MovementStateHash = Animator.StringToHash("MovementState");
    private static readonly int MovementXHash = Animator.StringToHash("MovementX");
    private static readonly int MovementZHash = Animator.StringToHash("MovementZ");

    private CharacterController _controller;
    private Camera _camera;
    private InputSystem_Actions _input;
    private Vector2 _moveInput;
    private bool _sprintHeld;
    private Vector3 _moveDirection;
    private float _verticalVelocity;
    private float _currentSpeed;
    private bool _jumpRequested;

    public enum MovementState
    {
        Idle = 0,
        Walking = 1,
        Running = 2,
        Sprinting = 3,
        Jumping = 4,
        Falling = 5
    }

    private MovementState _currentMovementState;
    public MovementState CurrentMovementState => _currentMovementState;
    public float CurrentSpeed => _currentSpeed;

    private float _debugLogTimer;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _camera = Camera.main;
        _input = new InputSystem_Actions();
    }

    private void Start()
    {
        // Find animator in Start (after all objects are initialized)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
        
        if (animator != null)
        {
            // Ensure animator is enabled and root motion is disabled so CharacterController drives movement.
            if (!animator.enabled)
            {
                animator.enabled = true;
            }
            animator.applyRootMotion = false;

            // Initialize animator parameters on start
            animator.SetFloat(SpeedHash, 0f);
            animator.SetBool(IsGroundedHash, true);
            animator.SetInteger(MovementStateHash, 0);
            animator.SetFloat(MovementXHash, 0f);
            animator.SetFloat(MovementZHash, 0f);
        }

        // Debug: animator found and enabled
        if (debugAnimator)
        {
            if (animator == null)
                Debug.LogWarning("[PLAYER_CONTROLLER] Animator not found on " + gameObject.name + " or its children. Assign it in the Inspector or ensure an Animator is on this object or a child.");
            else
                Debug.Log("[PLAYER_CONTROLLER] Animator found and enabled: " + animator.gameObject.name + " (enabled=" + animator.enabled + ", controller=" + (animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "null") + ")");
        }
    }

    private void OnEnable()
    {
        _input?.Enable();
        if (_input != null)
        {
            _input.Player.Move.performed += OnMove;
            _input.Player.Move.canceled += OnMove;
            _input.Player.Sprint.performed += OnSprint;
            _input.Player.Sprint.canceled += OnSprint;
            _input.Player.Jump.performed += OnJump;
        }
    }

    private void OnDisable()
    {
        if (_input != null)
        {
            _input.Player.Move.performed -= OnMove;
            _input.Player.Move.canceled -= OnMove;
            _input.Player.Sprint.performed -= OnSprint;
            _input.Player.Sprint.canceled -= OnSprint;
            _input.Player.Jump.performed -= OnJump;
            _input.Disable();
        }
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
        if (_moveInput.magnitude < 0.01f)
            _moveInput = Vector2.zero;
    }

    private void OnSprint(InputAction.CallbackContext ctx)
    {
        _sprintHeld = ctx.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            _jumpRequested = true;
    }

    private Vector2 GetMouseInputVector()
    {
        if (!useMouseInput || Mouse.current == null)
            return _moveInput;

        Vector2 delta = Mouse.current.delta.ReadValue();
        Vector2 scaled = new Vector2(
            delta.x * mouseXSensitivity,
            delta.y * mouseYSensitivity
        );

        float magnitude = scaled.magnitude;
        if (magnitude < 0.01f)
            return Vector2.zero;

        // Clamp so downstream code still sees a -1..1-style input vector
        return Vector2.ClampMagnitude(scaled, 1f);
    }

    private void Update()
    {
        bool isGrounded = _controller.isGrounded;
        if (isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        if (enableJump && isGrounded && _jumpRequested)
        {
            _verticalVelocity = Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(Physics.gravity.y) * gravityMultiplier);
            _jumpRequested = false;
        }

        // Decide which input source to use this frame
        _moveInput = useMouseInput ? GetMouseInputVector() : _moveInput;

        // Optional inversion for forward/back input
        if (invertVerticalMovement && Mathf.Abs(_moveInput.y) > 0.0001f)
        {
            _moveInput.y *= -1f;
        }

        // Movement direction is camera-relative so that \"forward\" always moves away from the camera.
        Vector3 camForward = _camera != null ? _camera.transform.forward : transform.forward;
        Vector3 camRight = _camera != null ? _camera.transform.right : transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        _moveDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

        float inputMagnitude = _moveInput.magnitude;
        // Set speed based on input - use a very small threshold to catch all input
        if (inputMagnitude > 0.001f)
            _currentSpeed = _sprintHeld ? runSpeed : walkSpeed;
        else
            _currentSpeed = 0f;

        Vector3 motion = _moveDirection * _currentSpeed * Time.deltaTime;
        _verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        motion.y = _verticalVelocity * Time.deltaTime;
        _controller.Move(motion);

        // Smoothly rotate character to face movement direction when there is meaningful input.
        if (_moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);
        }

        // Update movement state - use same threshold as speed check; include air states for animator
        if (!isGrounded)
            _currentMovementState = _verticalVelocity > 0f ? MovementState.Jumping : MovementState.Falling;
        else if (inputMagnitude < 0.001f)
            _currentMovementState = MovementState.Idle;
        else if (_sprintHeld)
            _currentMovementState = MovementState.Sprinting;
        else if (_currentSpeed >= runSpeed * 0.9f)
            _currentMovementState = MovementState.Running;
        else
            _currentMovementState = MovementState.Walking;

        UpdateAnimator(isGrounded);
    }

    private void UpdateAnimator(bool isGrounded)
    {
        // Try to find animator if still null (in case it was added later)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null || !animator.enabled) return;
        
        // Calculate normalized speed: 0 = idle, 0-0.5 = walk, 0.5-1 = run/sprint
        // Use runSpeed as max for normalization so walkSpeed (3) / runSpeed (6) = 0.5
        float normalizedSpeed = runSpeed > 0f ? Mathf.Clamp01(_currentSpeed / runSpeed) : 0f;

        // 2D blend parameters based on input direction (already camera-relative).
        Vector2 moveForBlend = _moveInput.sqrMagnitude > 1f ? _moveInput.normalized : _moveInput;

        // Always update all parameters every frame to ensure animator responds
        animator.SetFloat(SpeedHash, normalizedSpeed);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetInteger(MovementStateHash, (int)_currentMovementState);
        animator.SetFloat(MovementXHash, moveForBlend.x);
        animator.SetFloat(MovementZHash, moveForBlend.y);

        // Debug: log parameters periodically so you can see what's being sent (and why it might stay Idle)
        if (debugAnimator)
        {
            _debugLogTimer += Time.deltaTime;
            if (_debugLogTimer >= 0.5f)
            {
                _debugLogTimer = 0f;
                Debug.Log("[PLAYER_CONTROLLER] Animator params -> Speed=" + normalizedSpeed.ToString("F2") + ", MovementState=" + _currentMovementState + ", IsGrounded=" + isGrounded + " | input=" + _moveInput + ", currentSpeed=" + _currentSpeed.ToString("F2") + " | animator on: " + animator.gameObject.name);
            }
        }
    }
}
