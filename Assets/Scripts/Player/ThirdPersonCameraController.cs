using UnityEngine;

/// <summary>
/// Simple third-person camera controller that follows behind the player character.
/// Camera maintains a fixed distance, height, and downward angle.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The target character to follow")]
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [Tooltip("Distance behind player")]
    [SerializeField] private float cameraDistance = 5f;
    
    [Tooltip("Height above player")]
    [SerializeField] private float heightOffset = 2f;
    
    [Tooltip("Downward angle (degrees, positive = looking down)")]
    [SerializeField] private float verticalAngle = 25f;

    [Header("Camera Movement")]
    [Tooltip("How smoothly the camera follows")]
    [SerializeField] private float followSmoothness = 5f;
    
    [Tooltip("How fast the camera rotates to look at player")]
    [SerializeField] private float rotationSmoothness = 10f;

    [Header("Collision")]
    [Tooltip("Layer mask for collision detection")]
    [SerializeField] private LayerMask collisionLayerMask = -1;
    
    [Tooltip("Radius of collision sphere")]
    [SerializeField] private float collisionRadius = 0.3f;

    private Camera cam;
    private float currentCameraDistance;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        currentCameraDistance = cameraDistance;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateCameraPosition();
    }

    /// <summary>
    /// Updates camera position to follow behind player
    /// </summary>
    private void UpdateCameraPosition()
    {
        // Calculate position behind player
        Vector3 playerPosition = target.position;
        Vector3 playerBack = -target.forward;
        playerBack.y = 0f;
        playerBack.Normalize();

        // Calculate camera direction with downward angle
        Quaternion verticalRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
        Vector3 cameraDirection = verticalRotation * playerBack;

        // Calculate desired position
        Vector3 targetPosition = playerPosition + Vector3.up * heightOffset;
        Vector3 desiredPosition = targetPosition - cameraDirection * cameraDistance;

        // Check for collision
        Vector3 directionToCamera = desiredPosition - targetPosition;
        float desiredDistance = directionToCamera.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(
            targetPosition,
            collisionRadius,
            directionToCamera.normalized,
            out hit,
            desiredDistance,
            collisionLayerMask))
        {
            // Collision detected, move camera closer
            currentCameraDistance = Mathf.Max(hit.distance - collisionRadius, 0.5f);
        }
        else
        {
            // No collision, smoothly return to desired distance
            currentCameraDistance = Mathf.Lerp(currentCameraDistance, cameraDistance, Time.deltaTime * followSmoothness);
        }

        // Clamp to reasonable range
        currentCameraDistance = Mathf.Clamp(currentCameraDistance, 0.5f, cameraDistance);

        // Calculate final position with adjusted distance
        desiredPosition = targetPosition - cameraDirection * currentCameraDistance;

        // Smoothly move camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSmoothness);

        // Look at player
        Vector3 lookDirection = targetPosition - transform.position;
        if (lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (target == null)
            return;

        Vector3 playerPosition = target.position + Vector3.up * heightOffset;
        Vector3 playerBack = -target.forward;
        playerBack.y = 0f;
        playerBack.Normalize();

        Quaternion verticalRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
        Vector3 cameraDirection = verticalRotation * playerBack;
        Vector3 cameraPos = playerPosition - cameraDirection * cameraDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerPosition, cameraPos);
        Gizmos.DrawWireSphere(cameraPos, collisionRadius);
    }
}
