using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the rotation of the WORLD sphere based on player input.
/// Rotates the world around its center pivot using WASD/Arrow keys.
/// Features smooth acceleration/deceleration and analog-like input support.
/// </summary>
public class WorldRotationController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Pitch rotation speed (W/S) in degrees per second")]
    [SerializeField] private float pitchRotationSpeed = 30f;
    
    [Tooltip("Yaw rotation speed (A/D) in degrees per second")]
    [SerializeField] private float yawRotationSpeed = 60f;
    
    [Tooltip("How fast rotation accelerates when input is held")]
    [SerializeField] private float accelerationRate = 15f;
    
    [Tooltip("How fast rotation decelerates when input is released")]
    [SerializeField] private float decelerationRate = 20f;
    
    [Header("Input System")]
    [Tooltip("Input Actions asset reference - drag InputSystem_Actions.inputactions here")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    
    [Header("Rotation Pivot")]
    [Tooltip("The pivot point to rotate around. If not assigned, uses transform position.")]
    [SerializeField] private Transform rotationPivot;
    
    [Tooltip("Custom pivot position (only used if Rotation Pivot is not assigned)")]
    [SerializeField] private Vector3 customPivotPosition = Vector3.zero;
    
    [Header("Rotation Axes")]
    [Tooltip("W/S controls pitch (X-axis rotation)")]
    [SerializeField] private bool wSControlsPitch = true;
    
    [Tooltip("A/D controls yaw (Y-axis rotation)")]
    [SerializeField] private bool aDControlsYaw = true;
    
    // Input System
    private InputSystem_Actions inputActions;
    
    // Current rotation velocities (degrees per second)
    private float currentPitchVelocity;
    private float currentYawVelocity;
    
    // Input values
    private Vector2 rotationInput;
    
    // Public property for rotation speed (used by character follower)
    public float CurrentRotationSpeed => Mathf.Sqrt(currentPitchVelocity * currentPitchVelocity + currentYawVelocity * currentYawVelocity);
    
    private void Awake()
    {
        // Initialize input actions
        inputActions = new InputSystem_Actions();
    }
    
    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
        }
    }
    
    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Disable();
        }
    }
    
    private void Update()
    {
        HandleRotation();
    }
    
    /// <summary>
    /// Handles world rotation with smooth acceleration/deceleration
    /// </summary>
    private void HandleRotation()
    {
        // Get input values
        float pitchInput = 0f;
        float yawInput = 0f;
        
        if (wSControlsPitch)
        {
            pitchInput = -rotationInput.y; // W/S (forward/backward) - inverted
        }
        
        if (aDControlsYaw)
        {
            yawInput = -rotationInput.x; // A/D (left/right) - inverted
        }
        
        // Calculate target velocities based on input
        float targetPitchVelocity = pitchInput * pitchRotationSpeed;
        float targetYawVelocity = yawInput * yawRotationSpeed;
        
        // Smooth acceleration/deceleration
        if (Mathf.Abs(pitchInput) > 0.01f)
        {
            // Accelerate toward target
            currentPitchVelocity = Mathf.MoveTowards(
                currentPitchVelocity,
                targetPitchVelocity,
                accelerationRate * Time.deltaTime
            );
        }
        else
        {
            // Decelerate to zero
            currentPitchVelocity = Mathf.MoveTowards(
                currentPitchVelocity,
                0f,
                decelerationRate * Time.deltaTime
            );
        }
        
        if (Mathf.Abs(yawInput) > 0.01f)
        {
            // Accelerate toward target
            currentYawVelocity = Mathf.MoveTowards(
                currentYawVelocity,
                targetYawVelocity,
                accelerationRate * Time.deltaTime
            );
        }
        else
        {
            // Decelerate to zero
            currentYawVelocity = Mathf.MoveTowards(
                currentYawVelocity,
                0f,
                decelerationRate * Time.deltaTime
            );
        }
        
        // Apply rotation around defined pivot point
        Vector3 pivotPoint = GetPivotPoint();
        
        // Rotate around X-axis (pitch) and Y-axis (yaw) around the pivot
        if (Mathf.Abs(currentPitchVelocity) > 0.01f || Mathf.Abs(currentYawVelocity) > 0.01f)
        {
            // Rotate around pivot point
            // First rotate around Y-axis (yaw), then around X-axis (pitch)
            transform.RotateAround(pivotPoint, Vector3.up, currentYawVelocity * Time.deltaTime);
            transform.RotateAround(pivotPoint, transform.right, currentPitchVelocity * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Gets the pivot point for rotation
    /// </summary>
    private Vector3 GetPivotPoint()
    {
        if (rotationPivot != null)
        {
            return rotationPivot.position;
        }
        
        // If custom pivot position is set (not zero), use it relative to world origin
        if (customPivotPosition != Vector3.zero)
        {
            return customPivotPosition;
        }
        
        // Default to transform position
        return transform.position;
    }
    
    /// <summary>
    /// Input callback for movement input
    /// </summary>
    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();
        
        // Apply deadzone to prevent tiny input values
        float magnitude = inputValue.magnitude;
        if (magnitude < 0.01f)
        {
            rotationInput = Vector2.zero;
        }
        else
        {
            rotationInput = inputValue;
        }
    }
    
    /// <summary>
    /// Draws gizmo to visualize pivot point in Scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 pivot = GetPivotPoint();
        
        // Draw pivot point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pivot, 0.5f);
        
        // Draw line from transform to pivot
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, pivot);
    }
}
