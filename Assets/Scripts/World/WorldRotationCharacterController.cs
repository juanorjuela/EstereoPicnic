using UnityEngine;

/// <summary>
/// Handles character physics and gravity for the world rotation system.
/// Character is passive - no direct movement input, only gravity and ground detection.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class WorldRotationCharacterController : MonoBehaviour
{
    [Header("Gravity Settings")]
    [Tooltip("Multiplier for gravity")]
    [SerializeField] private float gravityMultiplier = 2f;
    
    [Header("Ground Check")]
    [Tooltip("Distance to check for ground below character")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    [Tooltip("Layer mask for ground detection")]
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    
    // Components
    private CharacterController characterController;
    
    // Movement state
    private float verticalVelocity;
    private bool isGrounded;
    
    // Public properties
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => verticalVelocity;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    
    private void Start()
    {
        // Snap character to ground on start
        SnapToGround();
    }
    
    private void Update()
    {
        // Check if CharacterController is enabled and GameObject is active
        if (characterController == null || !characterController.enabled || !gameObject.activeInHierarchy)
            return;
        
        HandleGravity();
        CheckGrounded();
    }
    
    /// <summary>
    /// Snaps the character to the ground below it
    /// </summary>
    private void SnapToGround()
    {
        float rayDistance = characterController.height / 2f + 1f;
        Vector3 rayStart = transform.position + characterController.center;
        
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayerMask))
        {
            float bottomOfController = transform.position.y + characterController.center.y - characterController.height / 2f;
            float groundY = hit.point.y;
            
            if (bottomOfController > groundY)
            {
                float snapDistance = bottomOfController - groundY;
                transform.position = new Vector3(transform.position.x, transform.position.y - snapDistance, transform.position.z);
            }
        }
    }
    
    /// <summary>
    /// Checks if the character is grounded
    /// </summary>
    private void CheckGrounded()
    {
        bool controllerGrounded = characterController.isGrounded;
        
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
        
        isGrounded = controllerGrounded || sphereCastGrounded;
        
        // Additional check for very close to ground
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
    /// Handles gravity
    /// </summary>
    private void HandleGravity()
    {
        bool wasGrounded = characterController.isGrounded;
        
        if (wasGrounded)
        {
            // Reset vertical velocity when grounded
            if (verticalVelocity < 0)
            {
                verticalVelocity = -5f; // Stronger negative value to keep grounded
            }
        }
        else
        {
            // Apply gravity if not grounded
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            
            // Clamp maximum fall speed
            float maxFallSpeed = -50f;
            if (verticalVelocity < maxFallSpeed)
            {
                verticalVelocity = maxFallSpeed;
            }
        }
    }
    
    /// <summary>
    /// Applies vertical movement (gravity) to the character
    /// Called by WorldRotationCharacterFollower
    /// </summary>
    public void ApplyVerticalMovement()
    {
        // Check if CharacterController is enabled and GameObject is active
        if (characterController == null || !characterController.enabled || !gameObject.activeInHierarchy)
            return;
        
        Vector3 movement = new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        characterController.Move(movement);
        
        // Snap to ground if grounded
        if (isGrounded && verticalVelocity <= 0)
        {
            if (Physics.Raycast(transform.position + characterController.center, Vector3.down, 
                out RaycastHit groundHit, characterController.height / 2f + 0.5f, groundLayerMask))
            {
                float bottomOfController = transform.position.y + characterController.center.y - characterController.height / 2f;
                float groundY = groundHit.point.y;
                
                if (bottomOfController > groundY + 0.1f)
                {
                    float snapDistance = bottomOfController - groundY;
                    transform.position = new Vector3(transform.position.x, transform.position.y - snapDistance, transform.position.z);
                }
            }
        }
    }
}
