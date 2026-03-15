using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple third-person follow camera. Stays behind the target (no mouse orbit).
/// Uses target's forward to place camera; optional smoothing and collision pull-in.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PLAYER_CAMERA_CONTROLLER : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float heightOffset = 2f;
    [SerializeField] private float pitchAngle = 25f;

    [Header("Smoothing")]
    [SerializeField] private float followSmoothTime = 5f;
    [SerializeField] private float rotationSmoothTime = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionLayerMask = -1;
    [SerializeField] private float collisionRadius = 0.3f;

    [Header("Orbit")]
    [SerializeField] private bool useMouseOrbit = true;
    [SerializeField] private float mouseXSensitivity = 120f;
    [SerializeField] private float mouseYSensitivity = 120f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;

    private float _currentDistance;
    private float _yaw;
    private float _pitch;

    private void Awake()
    {
        _currentDistance = distance;
        if (target != null)
        {
            // Initialize yaw so camera starts behind the target.
            Vector3 flatForward = target.forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude > 0.001f)
            {
                _yaw = Quaternion.LookRotation(flatForward).eulerAngles.y;
            }
        }
        _pitch = pitchAngle;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Update orbit from mouse/trackball input if enabled.
        if (useMouseOrbit && Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            float dt = Time.deltaTime;
            _yaw += delta.x * mouseXSensitivity * dt;
            _pitch -= delta.y * mouseYSensitivity * dt;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        Quaternion orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 targetPos = target.position + Vector3.up * heightOffset;
        Vector3 cameraDir = orbitRotation * Vector3.forward;

        Vector3 desiredPos = targetPos - cameraDir * distance;
        Vector3 toCamera = desiredPos - targetPos;
        float desiredDist = toCamera.magnitude;

        if (Physics.SphereCast(targetPos, collisionRadius, toCamera.normalized, out RaycastHit hit, desiredDist, collisionLayerMask))
            _currentDistance = Mathf.Max(hit.distance - collisionRadius, 0.5f);
        else
            _currentDistance = Mathf.Lerp(_currentDistance, distance, followSmoothTime * Time.deltaTime);

        _currentDistance = Mathf.Clamp(_currentDistance, 0.5f, distance);
        Vector3 finalPos = targetPos - cameraDir * _currentDistance;

        transform.position = Vector3.Lerp(transform.position, finalPos, followSmoothTime * Time.deltaTime);

        Vector3 lookDir = targetPos - transform.position;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothTime * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Vector3 targetPos = target.position + Vector3.up * heightOffset;
        Vector3 back = -target.forward;
        back.y = 0f;
        back.Normalize();
        Vector3 cameraDir = Quaternion.Euler(pitchAngle, 0f, 0f) * back;
        Vector3 camPos = targetPos - cameraDir * distance;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(targetPos, camPos);
        Gizmos.DrawWireSphere(camPos, collisionRadius);
    }
}
