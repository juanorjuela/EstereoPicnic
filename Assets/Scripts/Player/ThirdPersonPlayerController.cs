using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person player controller with movement, jumping, sprinting, and state tracking.
/// Uses Unity's new Input System and CharacterController for movement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Walking speed in units per second")]
    [SerializeField] private float walkSpeed = 3f;
    
    [Tooltip("Running speed in units per second")]
    [SerializeField] private float runSpeed = 6f;
    
    [Tooltip("Sprinting speed in units per second")]
    [SerializeField] private float sprintSpeed = 9f;
    
    [Tooltip("How fast the character rotates to face movement direction")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [Tooltip("Height of the jump")]
    [SerializeField] private float jumpHeight = 2f;
    
    [Tooltip("Multiplier for gravity")]
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Ground Check")]
    [Tooltip("Distance to check for ground below character")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    [Tooltip("Layer mask for ground detection")]
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer

    [Header("Input System")]
    [Tooltip("Input Actions asset reference - drag InputSystem_Actions.inputactions here")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Header("Animation")]
    [Tooltip("Animator component - will auto-find if not assigned")]
    [SerializeField] private Animator animator;

    // Animator parameter names (must match Animator Controller parameters)
    private static readonly int AnimatorSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimatorIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimatorMovementState = Animator.StringToHash("MovementState");

    // Components
    private CharacterController characterController;
    private Camera mainCamera;
    private InputSystem_Actions inputActions;

    // Input values
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool sprintHeld;

    // Movement state
    private Vector3 moveDirection;
    private float verticalVelocity;
    private bool isGrounded;
    private float currentSpeed;

    // Movement states enum
    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Sprinting,
        Jumping,
        Falling
    }

    // Public properties for state tracking
    public MovementState CurrentMovementState { get; private set; }
    public bool IsGrounded => isGrounded;
    public float CurrentSpeed => currentSpeed;
    public Vector3 MoveDirection => moveDirection;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        // Find Animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                // Try to find in children (character model might be a child object)
                animator = GetComponentInChildren<Animator>();
            }
        }

        // Initialize input actions
        // The InputSystem_Actions class is auto-generated and contains all actions
        // We create a new instance which will work regardless of asset assignment
        inputActions = new InputSystem_Actions();
        
        // Note: The inputActionsAsset field is for reference/organization
        // The generated InputSystem_Actions class has all actions built-in
    }

    private void Start()
    {
        // Snap character to ground on start to fix initial floating position
        SnapToGround();
    }
    
    /// <summary>
    /// Snaps the character to the ground below it
    /// </summary>
    private void SnapToGround()
    {
        // Cast a ray downward from the character's position
        float rayDistance = characterController.height / 2f + 1f;
        Vector3 rayStart = transform.position + characterController.center;
        
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayerMask))
        {
            // Calculate where the bottom of the character controller should be
            float bottomOfController = transform.position.y + characterController.center.y - characterController.height / 2f;
            float groundY = hit.point.y;
            
            // If there's a gap, snap the character down
            if (bottomOfController > groundY)
            {
                float snapDistance = bottomOfController - groundY;
                transform.position = new Vector3(transform.position.x, transform.position.y - snapDistance, transform.position.z);
            }
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Jump.performed += OnJump;
            inputActions.Player.Jump.canceled += OnJump;
            inputActions.Player.Sprint.performed += OnSprint;
            inputActions.Player.Sprint.canceled += OnSprint;
        }
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Jump.performed -= OnJump;
            inputActions.Player.Jump.canceled -= OnJump;
            inputActions.Player.Sprint.performed -= OnSprint;
            inputActions.Player.Sprint.canceled -= OnSprint;
            inputActions.Disable();
        }
    }

    private void Update()
    {
        HandleGravity();
        CheckGrounded();
        HandleMovement();
        UpdateMovementState();
        UpdateAnimator();
    }

    /// <summary>
    /// Checks if the character is grounded using CharacterController's built-in check and sphere cast
    /// </summary>
    private void CheckGrounded()
    {
        // Use CharacterController's built-in grounded check as primary
        // This is more reliable than manual raycasts
        bool controllerGrounded = characterController.isGrounded;
        
        // Additional sphere cast check for more precise ground detection
        Vector3 spherePosition = transform.position + characterController.center;
        float sphereRadius = characterController.radius - characterController.skinWidth;
        float sphereDistance = characterController.height / 2f + groundCheckDistance;

        bool sphereCastGrounded = Physics.SphereCast(
            spherePosition,
            sphereRadius,
            Vector3.down,
            out RaycastHit hit,
            sphereDistance,
            groundLayerMask
        );

        // Combine both checks - if either says we're grounded, we are
        isGrounded = controllerGrounded || sphereCastGrounded;

        // Additional check: if we're moving down and very close to ground, consider grounded
        if (!isGrounded && verticalVelocity <= 0 && hit.collider != null)
        {
            float distanceToGround = hit.distance;
            if (distanceToGround < 0.15f)
            {
                isGrounded = true;
            }
        }
    }

    /// <summary>
    /// Handles horizontal movement based on input
    /// </summary>
    private void HandleMovement()
    {
        // Since camera always follows behind player, use player's forward direction for movement
        // This prevents rotation feedback loop where camera rotation causes player rotation
        Vector3 playerForward = transform.forward;
        Vector3 playerRight = transform.right;

        // Project onto horizontal plane
        playerForward.y = 0f;
        playerRight.y = 0f;
        playerForward.Normalize();
        playerRight.Normalize();

        // Calculate movement direction relative to player's facing direction
        moveDirection = (playerForward * moveInput.y + playerRight * moveInput.x).normalized;

        // Determine current speed based on input and sprint state
        // Use a small deadzone to prevent drift from small input values
        float inputMagnitude = moveInput.magnitude;
        if (inputMagnitude > 0.01f) // Small deadzone to prevent accidental movement
        {
            if (sprintHeld && inputMagnitude > 0.1f)
            {
                currentSpeed = sprintSpeed;
            }
            else if (inputMagnitude > 0.5f)
            {
                currentSpeed = runSpeed;
            }
            else if (inputMagnitude > 0.1f)
            {
                currentSpeed = walkSpeed;
            }
            else
            {
                // Very small input - treat as idle
                currentSpeed = 0f;
            }
        }
        else
        {
            currentSpeed = 0f;
        }

        // Apply movement
        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;

        characterController.Move(movement);
        
        // Snap to ground if grounded and not jumping
        if (isGrounded && verticalVelocity <= 0 && !jumpPressed)
        {
            // Use a small downward raycast to find the exact ground position
            if (Physics.Raycast(transform.position + characterController.center, Vector3.down, 
                out RaycastHit groundHit, characterController.height / 2f + 0.5f, groundLayerMask))
            {
                // Calculate where the bottom of the character controller should be
                float bottomOfController = transform.position.y + characterController.center.y - characterController.height / 2f;
                float groundY = groundHit.point.y;
                
                // If there's a gap, snap the character down
                if (bottomOfController > groundY + 0.1f)
                {
                    float snapDistance = bottomOfController - groundY;
                    transform.position = new Vector3(transform.position.x, transform.position.y - snapDistance, transform.position.z);
                }
            }
        }

        // Rotate character to face movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Handles gravity and jumping
    /// </summary>
    private void HandleGravity()
    {
        // Check grounded state first (using CharacterController's built-in check)
        bool wasGrounded = characterController.isGrounded;
        
        if (wasGrounded)
        {
            // Reset vertical velocity when grounded (unless jumping)
            // Use a stronger downward force to ensure we stay grounded
            if (verticalVelocity < 0)
            {
                verticalVelocity = -5f; // Stronger negative value to keep grounded
            }

            // Handle jump
            if (jumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y * gravityMultiplier);
                jumpPressed = false; // Consume jump input
                wasGrounded = false; // We're jumping now
            }
        }
        
        // Apply gravity if not grounded
        if (!wasGrounded)
        {
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            
            // Clamp maximum fall speed to prevent excessive falling
            float maxFallSpeed = -50f;
            if (verticalVelocity < maxFallSpeed)
            {
                verticalVelocity = maxFallSpeed;
            }
        }
    }

    /// <summary>
    /// Updates the current movement state based on character conditions
    /// </summary>
    private void UpdateMovementState()
    {
        if (!isGrounded)
        {
            // In air
            if (verticalVelocity > 0)
            {
                CurrentMovementState = MovementState.Jumping;
            }
            else
            {
                CurrentMovementState = MovementState.Falling;
            }
        }
        else
        {
            // On ground
            // Use a more strict threshold to ensure idle state when not moving
            if (currentSpeed < 0.01f || moveInput.magnitude < 0.01f)
            {
                CurrentMovementState = MovementState.Idle;
            }
            else if (sprintHeld && currentSpeed >= sprintSpeed * 0.9f)
            {
                CurrentMovementState = MovementState.Sprinting;
            }
            else if (currentSpeed >= runSpeed * 0.9f)
            {
                CurrentMovementState = MovementState.Running;
            }
            else
            {
                CurrentMovementState = MovementState.Walking;
            }
        }
    }

    /// <summary>
    /// Updates Animator parameters based on current movement state
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        // Update Speed parameter (normalized 0-1 based on max sprint speed)
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / sprintSpeed);
        animator.SetFloat(AnimatorSpeed, normalizedSpeed);

        // Update IsGrounded parameter
        animator.SetBool(AnimatorIsGrounded, isGrounded);

        // Update MovementState parameter (as integer: 0=Idle, 1=Walking, 2=Running, 3=Sprinting, 4=Jumping, 5=Falling)
        animator.SetInteger(AnimatorMovementState, (int)CurrentMovementState);
    }

    // Input System callbacks
    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();
        
        // Apply deadzone to prevent tiny input values from causing movement
        float magnitude = inputValue.magnitude;
        if (magnitude < 0.01f)
        {
            moveInput = Vector2.zero; // Ensure it's exactly zero
        }
        else
        {
            moveInput = inputValue;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            jumpPressed = true;
        }
        else if (context.canceled)
        {
            jumpPressed = false;
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        sprintHeld = context.performed || context.ReadValueAsButton();
    }

    // Gizmos for debugging ground check
    private void OnDrawGizmosSelected()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            Vector3 spherePosition = transform.position + characterController.center;
            float sphereRadius = characterController.radius;
            float sphereDistance = characterController.height / 2f - characterController.radius + groundCheckDistance;

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(spherePosition + Vector3.down * sphereDistance, sphereRadius);
        }
    }
}
