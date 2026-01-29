using UnityEngine;

/// <summary>
/// Makes the character follow world movement to stay centered on screen.
/// Character moves toward screen center (viewport 0.5, 0.5) as the world rotates.
/// Features distance-based speed calculation and sprint logic.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class WorldRotationCharacterFollower : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the WORLD object (parent of GrassSphere)")]
    [SerializeField] private Transform worldTransform;
    
    [Tooltip("Reference to the GrassSphere GameObject (has Capsule Collider)")]
    [SerializeField] private GameObject grassSphere;
    
    [Tooltip("Reference to the camera")]
    [SerializeField] private Camera mainCamera;
    
    [Tooltip("Reference to WorldRotationController for rotation speed")]
    [SerializeField] private WorldRotationController worldRotationController;
    
    [Header("Movement Settings")]
    [Tooltip("Walking speed in units per second")]
    [SerializeField] private float walkSpeed = 3f;
    
    [Tooltip("Running speed in units per second")]
    [SerializeField] private float runSpeed = 6f;
    
    [Tooltip("Sprinting speed in units per second")]
    [SerializeField] private float sprintSpeed = 9f;
    
    [Header("Speed Thresholds")]
    [Tooltip("Distance from center to start running (normalized viewport distance)")]
    [SerializeField] private float runDistanceThreshold = 0.1f;
    
    [Tooltip("Distance from center to start sprinting (normalized viewport distance)")]
    [SerializeField] private float sprintDistanceThreshold = 0.2f;
    
    [Tooltip("World rotation speed threshold to trigger sprint (degrees per second)")]
    [SerializeField] private float worldSpeedSprintThreshold = 15f;
    
    [Header("Animation")]
    [Tooltip("Animator component - will auto-find if not assigned")]
    [SerializeField] private Animator animator;
    
    // Animator parameter names
    private static readonly int AnimatorSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimatorIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimatorMovementState = Animator.StringToHash("MovementState");
    
    // Movement states enum (matches ThirdPersonPlayerController)
    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Sprinting,
        Jumping,
        Falling
    }
    
    // Components
    private CharacterController characterController;
    private WorldRotationCharacterController characterPhysics;
    private CapsuleCollider sphereCollider;
    
    // Movement state
    private float currentSpeed;
    public MovementState CurrentMovementState { get; private set; }
    public float CurrentSpeed => currentSpeed;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        characterPhysics = GetComponent<WorldRotationCharacterController>();
        
        // Find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        
        // Get sphere collider from GrassSphere
        if (grassSphere != null)
        {
            sphereCollider = grassSphere.GetComponent<CapsuleCollider>();
        }
    }
    
    private void Update()
    {
        // Check if CharacterController is enabled and GameObject is active
        if (characterController == null || !characterController.enabled || !gameObject.activeInHierarchy)
            return;
        
        HandleFollowing();
        UpdateMovementState();
        UpdateAnimator();
        
        // Apply vertical movement (gravity)
        if (characterPhysics != null)
        {
            characterPhysics.ApplyVerticalMovement();
        }
    }
    
    /// <summary>
    /// Handles character movement to stay centered on screen
    /// </summary>
    private void HandleFollowing()
    {
        if (mainCamera == null || worldTransform == null)
            return;
        
        // Get character's current screen position
        Vector3 characterScreenPos = mainCamera.WorldToViewportPoint(transform.position);
        
        // Calculate distance from screen center (0.5, 0.5)
        Vector2 centerOffset = new Vector2(
            characterScreenPos.x - 0.5f,
            characterScreenPos.y - 0.5f
        );
        
        float distanceFromCenter = centerOffset.magnitude;
        
        // Calculate target position on sphere surface
        Vector3 targetWorldPos = GetTargetPositionOnSphere();
        
        if (targetWorldPos == Vector3.zero)
        {
            // Couldn't find target, stay idle
            currentSpeed = 0f;
            return;
        }
        
        // Calculate direction to target
        Vector3 directionToTarget = (targetWorldPos - transform.position).normalized;
        
        // Project direction onto sphere surface (remove vertical component for horizontal movement)
        // We'll move along the sphere surface
        Vector3 horizontalDirection = directionToTarget;
        horizontalDirection.y = 0f;
        horizontalDirection.Normalize();
        
        // Calculate movement speed based on distance
        CalculateSpeed(distanceFromCenter);
        
        // Apply movement
        if (currentSpeed > 0.01f && directionToTarget.magnitude > 0.1f)
        {
            // Check if CharacterController is enabled and GameObject is active
            if (characterController != null && characterController.enabled && gameObject.activeInHierarchy)
            {
                Vector3 movement = directionToTarget * currentSpeed * Time.deltaTime;
                movement.y = 0f; // Vertical movement handled by character physics
                
                characterController.Move(movement);
            }
        }
        else
        {
            currentSpeed = 0f;
        }
    }
    
    /// <summary>
    /// Gets the target position on the sphere surface (screen center projected onto sphere)
    /// </summary>
    private Vector3 GetTargetPositionOnSphere()
    {
        if (mainCamera == null || sphereCollider == null)
            return Vector3.zero;
        
        // Cast ray from camera through viewport center
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        // Get sphere radius from collider
        float sphereRadius = sphereCollider.radius;
        Vector3 sphereCenter = worldTransform.position + sphereCollider.center;
        
        // Calculate intersection with sphere
        Vector3 toRayOrigin = ray.origin - sphereCenter;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(toRayOrigin, ray.direction);
        float c = Vector3.Dot(toRayOrigin, toRayOrigin) - sphereRadius * sphereRadius;
        
        float discriminant = b * b - 4f * a * c;
        
        if (discriminant < 0f)
        {
            // Ray doesn't intersect sphere, use closest point on sphere surface
            Vector3 toCharacter = transform.position - sphereCenter;
            toCharacter = toCharacter.normalized * sphereRadius;
            return sphereCenter + toCharacter;
        }
        
        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtDiscriminant) / (2f * a);
        float t2 = (-b + sqrtDiscriminant) / (2f * a);
        
        // Use the closer intersection point
        float t = t1 > 0f ? t1 : t2;
        
        if (t > 0f)
        {
            Vector3 intersectionPoint = ray.origin + ray.direction * t;
            
            // Project onto sphere surface to ensure exact radius
            Vector3 toIntersection = intersectionPoint - sphereCenter;
            toIntersection = toIntersection.normalized * sphereRadius;
            
            return sphereCenter + toIntersection;
        }
        
        // Fallback: project character position onto sphere
        Vector3 toChar = transform.position - sphereCenter;
        toChar = toChar.normalized * sphereRadius;
        return sphereCenter + toChar;
    }
    
    /// <summary>
    /// Calculates movement speed based on distance from center and world rotation speed
    /// </summary>
    private void CalculateSpeed(float distanceFromCenter)
    {
        // Get world rotation speed
        float worldSpeed = worldRotationController != null ? worldRotationController.CurrentRotationSpeed : 0f;
        
        // Determine if we should sprint based on world speed
        bool shouldSprintFromWorldSpeed = worldSpeed > worldSpeedSprintThreshold;
        
        // Calculate speed based on distance
        if (distanceFromCenter < 0.01f)
        {
            // Very close to center - idle
            currentSpeed = 0f;
        }
        else if (distanceFromCenter >= sprintDistanceThreshold || shouldSprintFromWorldSpeed)
        {
            // Far from center or world rotating fast - sprint
            currentSpeed = sprintSpeed;
        }
        else if (distanceFromCenter >= runDistanceThreshold)
        {
            // Medium distance - run
            currentSpeed = runSpeed;
        }
        else
        {
            // Close but not at center - walk
            // Interpolate between walk and run based on distance
            float normalizedDistance = distanceFromCenter / runDistanceThreshold;
            currentSpeed = Mathf.Lerp(walkSpeed, runSpeed, normalizedDistance);
        }
    }
    
    /// <summary>
    /// Updates the current movement state based on speed
    /// </summary>
    private void UpdateMovementState()
    {
        bool isGrounded = characterPhysics != null ? characterPhysics.IsGrounded : characterController.isGrounded;
        
        if (!isGrounded)
        {
            float verticalVel = characterPhysics != null ? characterPhysics.VerticalVelocity : 0f;
            CurrentMovementState = verticalVel > 0 ? MovementState.Jumping : MovementState.Falling;
        }
        else
        {
            if (currentSpeed < 0.01f)
            {
                CurrentMovementState = MovementState.Idle;
            }
            else if (currentSpeed >= sprintSpeed * 0.9f)
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
        bool isGrounded = characterPhysics != null ? characterPhysics.IsGrounded : characterController.isGrounded;
        animator.SetBool(AnimatorIsGrounded, isGrounded);
        
        // Update MovementState parameter (as integer: 0=Idle, 1=Walking, 2=Running, 3=Sprinting, 4=Jumping, 5=Falling)
        animator.SetInteger(AnimatorMovementState, (int)CurrentMovementState);
    }
}
