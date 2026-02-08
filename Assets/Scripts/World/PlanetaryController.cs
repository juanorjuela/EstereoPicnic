// -----------------------------------------------------------------------------
// SETUP (Unity):
// 1. Planet: Assign a Transform that will be rotated (e.g. GrassSphere or parent).
//    No Rigidbody on planet. Collider on planet for ground.
// 2. Player: Add Rigidbody (mass 1, useGravity off in script), CapsuleCollider,
//    then this script. Assign Planet reference.
// 3. Input: Uses InputSystem_Actions (Player.Move, Player.Jump). Enable in OnEnable.
// 4. Camera: Child of player works by default.
// 5. To use: Disable WorldRotationController, WorldRotationCharacterFollower,
//    WorldRotationCharacterController; remove/disable CharacterController.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Planetary movement: custom gravity toward planet, surface alignment, and orbital
/// movement by rotating the planet (player stays on surface). Uses Rigidbody only.
/// Math: gravity = (planet - player).normalized * strength; up = surface normal;
/// tangent right = Cross(up, refForward); forward = Cross(right, up). Planet rotates
/// around player's right (pitch) and forward (yaw); player position is then updated
/// to the rotated attachment point via MovePosition.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlanetaryController : MonoBehaviour
{
    #region Inspector

    [Header("Planet")]
    [Tooltip("The planet Transform that will be rotated. Required.")]
    [SerializeField] private Transform planet;

    [Header("Gravity")]
    [Tooltip("Strength of gravity toward planet center.")]
    [SerializeField] private float gravityStrength = 30f;

    [Header("Movement (Planet Rotation)")]
    [Tooltip("Planet rotation speed in degrees per second per unit input.")]
    [SerializeField] private float moveSpeed = 45f;

    [Tooltip("Slerp speed for aligning player up to surface normal.")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Movement Polish")]
    [Tooltip("How fast rotation accelerates when input is held.")]
    [SerializeField] private float accelerationRate = 15f;

    [Tooltip("How fast rotation decelerates when input is released.")]
    [SerializeField] private float decelerationRate = 20f;

    [Tooltip("Optional damping on rotation velocity (0 = none).")]
    [SerializeField] private float rotationDamping = 0f;

    [Header("Jump")]
    [Tooltip("Impulse force along surface normal when jumping.")]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("Distance along gravity direction to consider grounded.")]
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Tooltip("Layer mask for ground (planet).")]
    [SerializeField] private LayerMask groundLayerMask = 1;

    [Header("Camera")]
    [Tooltip("Optional: camera as child of player. No logic here; hook for future smoothing.")]
    [SerializeField] private Transform optionalCamera;

    #endregion

    #region Input

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool jumpPressed;

    #endregion

    #region State

    private Rigidbody rb;
    private float currentPitchVelocity; // degrees/sec
    private float currentYawVelocity;
    private Vector3 lastValidForward = Vector3.forward;

    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Jump.performed += OnJump;
        }
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Jump.performed -= OnJump;
            inputActions.Disable();
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        float mag = value.magnitude;
        moveInput = mag < 0.01f ? Vector2.zero : value;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void FixedUpdate()
    {
        if (planet == null) return;

        Vector3 toPlayer = rb.position - planet.position;
        float dist = toPlayer.magnitude;
        if (dist < 0.001f) return;

        Vector3 up = toPlayer / dist; // surface normal (outward from planet)
        Vector3 gravityDir = -up;

        #region Gravity
        rb.AddForce(gravityDir * gravityStrength);
        #endregion

        #region Tangent frame
        // For planet rotation axes and surface alignment
        Vector3 refForward = optionalCamera != null && optionalCamera.gameObject.activeInHierarchy
            ? optionalCamera.forward
            : Vector3.forward;
        Vector3 right = Vector3.Cross(up, refForward).normalized;
        if (right.sqrMagnitude < 0.01f) // pole: up ~ ±refForward
            right = Vector3.Cross(up, Vector3.right).normalized;
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(up, Vector3.up).normalized;
        right.Normalize();
        Vector3 forward = Vector3.Cross(right, up).normalized;
        if (forward.sqrMagnitude > 0.01f)
            lastValidForward = forward;
        #endregion

        #region Orbital movement (planet rotation)
        // Smoothed rotation velocities
        float targetPitch = -moveInput.y * moveSpeed;
        float targetYaw = moveInput.x * moveSpeed;
        float acc = accelerationRate * Time.fixedDeltaTime;
        float dec = decelerationRate * Time.fixedDeltaTime;

        if (Mathf.Abs(moveInput.y) > 0.01f)
            currentPitchVelocity = Mathf.MoveTowards(currentPitchVelocity, targetPitch, acc);
        else
            currentPitchVelocity = Mathf.MoveTowards(currentPitchVelocity, 0f, dec);

        if (Mathf.Abs(moveInput.x) > 0.01f)
            currentYawVelocity = Mathf.MoveTowards(currentYawVelocity, targetYaw, acc);
        else
            currentYawVelocity = Mathf.MoveTowards(currentYawVelocity, 0f, dec);

        if (rotationDamping > 0f)
        {
            float d = 1f - rotationDamping * Time.fixedDeltaTime;
            currentPitchVelocity *= Mathf.Clamp01(d);
            currentYawVelocity *= Mathf.Clamp01(d);
        }

        // --- Rotate planet around its center (player's right = pitch, forward = yaw) ---
        Vector3 center = planet.position;
        if (Mathf.Abs(currentPitchVelocity) > 0.001f)
            planet.RotateAround(center, right, currentPitchVelocity * Time.fixedDeltaTime);
        if (Mathf.Abs(currentYawVelocity) > 0.001f)
            planet.RotateAround(center, lastValidForward, currentYawVelocity * Time.fixedDeltaTime);

        // Keep player on surface: attachment point moved with planet
        float radius = dist;
        Vector3 surfacePoint = planet.position + up * radius;
        Vector3 localAttachment = planet.InverseTransformPoint(surfacePoint);
        Vector3 newWorldPos = planet.TransformPoint(localAttachment);
        rb.MovePosition(newWorldPos);

        // Recompute up after move for alignment
        Vector3 toPlayerNew = newWorldPos - planet.position;
        Vector3 upNew = toPlayerNew.sqrMagnitude > 0.001f ? toPlayerNew.normalized : up;

        // --- Jump (along surface normal) ---
        bool grounded = IsGrounded(gravityDir);
        if (grounded && jumpPressed)
        {
            rb.AddForce(up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
        else if (!grounded)
        {
            jumpPressed = false;
        }
        #endregion

        #region Surface alignment
        Quaternion targetRot = Quaternion.LookRotation(lastValidForward, upNew);
        Quaternion slerped = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(slerped);
        #endregion
    }

    /// <summary>
    /// Ground check along gravity direction (toward planet).
    /// </summary>
    private bool IsGrounded(Vector3 gravityDir)
    {
        float checkDist = groundCheckDistance + 0.1f;
        if (GetComponent<CapsuleCollider>() is CapsuleCollider cap)
        {
            Vector3 center = transform.TransformPoint(cap.center);
            float r = Mathf.Max(cap.radius - 0.02f, 0.01f);
            return Physics.SphereCast(center, r, gravityDir, out _, checkDist + cap.height * 0.5f, groundLayerMask);
        }
        return Physics.Raycast(rb.position, gravityDir, checkDist, groundLayerMask);
    }
}
