using UnityEngine;

/// <summary>
/// Fixed position camera controller for world rotation system.
/// Camera stays at fixed position and looks at character.
/// Features dynamic Field of View zoom based on character distance from screen center.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WorldRotationCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The target character to look at")]
    [SerializeField] private Transform target;
    
    [Header("Camera Position")]
    [Tooltip("Fixed world position for camera. Set this in the scene or via code.")]
    [SerializeField] private Vector3 cameraPosition = new Vector3(0f, 3f, -4f);
    
    [Tooltip("Lock camera position completely (no movement at all)")]
    [SerializeField] private bool lockPosition = true;
    
    [Header("Position Follow (Optional)")]
    [Tooltip("Enable camera position following (can cause unwanted movement)")]
    [SerializeField] private bool enablePositionFollow = false;
    
    [Tooltip("How much the camera moves when following (0 = fixed position, 1 = full follow)")]
    [SerializeField, Range(0f, 1f)] private float followAmount = 0.5f;
    
    [Tooltip("How fast the camera follows the character (delay in seconds)")]
    [SerializeField, Range(0.1f, 1f)] private float followDelay = 0.3f;
    
    [Header("Rotation Settings")]
    [Tooltip("How fast the camera rotates to look at character")]
    [SerializeField, Range(1f, 50f)] private float rotationSmoothness = 10f;
    
    [Tooltip("Enable smooth rotation (disable for instant look-at)")]
    [SerializeField] private bool smoothRotation = true;
    
    [Header("Angle Offsets")]
    [Tooltip("Angle offset upward in degrees (adds to look direction)")]
    [SerializeField, Range(-45f, 45f)] private float upwardAngleOffset = 5f;
    
    [Tooltip("Additional horizontal angle offset in degrees")]
    [SerializeField, Range(-45f, 45f)] private float horizontalAngleOffset = 0f;
    
    [Tooltip("Additional roll angle offset in degrees")]
    [SerializeField, Range(-45f, 45f)] private float rollAngleOffset = 0f;
    
    [Header("Zoom Settings")]
    [Tooltip("Enable dynamic zoom based on character distance")]
    [SerializeField] private bool enableDynamicZoom = true;
    
    [Tooltip("Base Field of View (when character is at center)")]
    [SerializeField, Range(30f, 120f)] private float baseFieldOfView = 66f;
    
    [Tooltip("Maximum Field of View (when character is far from center)")]
    [SerializeField, Range(30f, 120f)] private float maxFieldOfView = 76f;
    
    [Tooltip("Maximum distance from center to trigger full zoom (normalized viewport distance)")]
    [SerializeField, Range(0.1f, 1f)] private float maxZoomDistance = 0.3f;
    
    [Tooltip("How smooth the zoom transition is")]
    [SerializeField, Range(0.1f, 10f)] private float zoomSmoothness = 2f;
    
    // Components
    private Camera cam;
    private float currentFieldOfView;
    private Vector3 currentCameraPosition;
    private Vector3 targetCameraPosition;
    private Vector3 lockedPosition;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        currentFieldOfView = baseFieldOfView;
        
        // Store initial position
        lockedPosition = transform.position;
        if (lockPosition)
        {
            cameraPosition = transform.position;
        }
        
        // Set initial camera position
        currentCameraPosition = cameraPosition;
        targetCameraPosition = cameraPosition;
        
        if (lockPosition)
        {
            transform.position = cameraPosition;
        }
    }
    
    private void Start()
    {
        // Initialize FoV
        if (cam != null)
        {
            cam.fieldOfView = baseFieldOfView;
            currentFieldOfView = baseFieldOfView;
        }
        
        // Lock position if enabled
        if (lockPosition)
        {
            lockedPosition = cameraPosition;
            transform.position = lockedPosition;
        }
    }
    
    private void OnValidate()
    {
        // Ensure max FoV is not less than base FoV
        if (maxFieldOfView < baseFieldOfView)
        {
            maxFieldOfView = baseFieldOfView;
        }
        
        // Update camera position if locked
        if (lockPosition && Application.isPlaying)
        {
            lockedPosition = cameraPosition;
            transform.position = lockedPosition;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        // Only update position if not locked and follow is enabled
        if (!lockPosition && enablePositionFollow)
        {
            UpdateFollow();
        }
        else if (lockPosition)
        {
            // Keep position locked
            transform.position = lockedPosition;
        }
        
        UpdateLookAt();
        
        if (enableDynamicZoom)
        {
            UpdateZoom();
        }
    }
    
    /// <summary>
    /// Updates camera position to smoothly follow character (only if enabled)
    /// </summary>
    private void UpdateFollow()
    {
        if (target == null || !enablePositionFollow)
            return;
        
        // Calculate target position based on character position and follow amount
        Vector3 characterOffset = (target.position - cameraPosition) * followAmount;
        targetCameraPosition = cameraPosition + characterOffset;
        
        // Smoothly interpolate camera position with delay (ease in/out)
        float smoothFactor = Time.deltaTime / Mathf.Max(followDelay, 0.01f);
        currentCameraPosition = Vector3.Lerp(currentCameraPosition, targetCameraPosition, smoothFactor);
        
        // Apply position
        transform.position = currentCameraPosition;
    }
    
    /// <summary>
    /// Updates camera rotation to look at character with angle offsets
    /// </summary>
    private void UpdateLookAt()
    {
        Vector3 lookDirection = target.position - transform.position;
        
        if (lookDirection.magnitude > 0.1f)
        {
            // Calculate base rotation
            Quaternion baseRotation = Quaternion.LookRotation(lookDirection);
            
            // Apply angle offsets
            Vector3 eulerAngles = baseRotation.eulerAngles;
            eulerAngles.x -= upwardAngleOffset; // Subtract because Unity's X rotation is inverted
            eulerAngles.y += horizontalAngleOffset;
            eulerAngles.z += rollAngleOffset;
            Quaternion targetRotation = Quaternion.Euler(eulerAngles);
            
            // Apply rotation
            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
    
    /// <summary>
    /// Updates camera Field of View based on character distance from screen center
    /// </summary>
    private void UpdateZoom()
    {
        if (cam == null || target == null)
            return;
        
        // Get character's current screen position
        Vector3 characterScreenPos = cam.WorldToViewportPoint(target.position);
        
        // Calculate distance from screen center (0.5, 0.5)
        Vector2 centerOffset = new Vector2(
            characterScreenPos.x - 0.5f,
            characterScreenPos.y - 0.5f
        );
        
        float distanceFromCenter = centerOffset.magnitude;
        
        // Normalize distance (0 to maxZoomDistance maps to 0 to 1)
        float normalizedDistance = Mathf.Clamp01(distanceFromCenter / maxZoomDistance);
        
        // Calculate target FoV based on distance
        float targetFoV = Mathf.Lerp(baseFieldOfView, maxFieldOfView, normalizedDistance);
        
        // Smoothly interpolate to target FoV
        currentFieldOfView = Mathf.Lerp(currentFieldOfView, targetFoV, zoomSmoothness * Time.deltaTime);
        
        // Apply to camera
        cam.fieldOfView = currentFieldOfView;
    }
    
    /// <summary>
    /// Sets the camera's position (only works if lockPosition is false)
    /// </summary>
    public void SetCameraPosition(Vector3 position)
    {
        cameraPosition = position;
        targetCameraPosition = position;
        currentCameraPosition = position;
        
        if (!lockPosition)
        {
            transform.position = position;
        }
        else
        {
            lockedPosition = position;
            transform.position = lockedPosition;
        }
    }
    
    /// <summary>
    /// Gets the current camera position
    /// </summary>
    public Vector3 GetCameraPosition()
    {
        return cameraPosition;
    }
    
    /// <summary>
    /// Toggles position lock
    /// </summary>
    public void SetLockPosition(bool locked)
    {
        lockPosition = locked;
        if (locked)
        {
            lockedPosition = transform.position;
            cameraPosition = lockedPosition;
        }
    }
    
    /// <summary>
    /// Sets the target to look at
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
