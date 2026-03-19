using UnityEngine;

/// <summary>
/// NPC that can show an eco-tip speech bubble and be collected
/// to follow the player using simple scripted movement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FriendNPC : MonoBehaviour
{
    public enum FriendState
    {
        IdleStationary,
        CollectedFollowing
    }

    [Header("References")]
    [SerializeField] private Transform speechBubbleAnchor;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem collectParticles;

    [Header("Data")]
    [SerializeField] private EcoTipsDatabase ecoTipsDatabase;

    [Header("Speech Bubble")]
    [SerializeField] private SpeechBubbleController speechBubblePrefab;

    [Header("Movement Settings")]
    [Tooltip("Walking speed when following the player.")]
    [SerializeField] private float followSpeed = 3f;

    [Tooltip("Minimum distance to its target slot before stopping.")]
    [SerializeField] private float stopDistance = 0.3f;

    [Tooltip("Rotation speed when turning to face movement direction.")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Popup")]
    [Tooltip("Floating text prefab used for '+1 Friend' feedback.")]
    [SerializeField] private FloatingTextPopup popupPrefab;

    [Tooltip("World offset for the '+1 Friend' popup.")]
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 2f, 0f);

    private CharacterController characterController;
    private SpeechBubbleController activeBubble;
    private FriendState state = FriendState.IdleStationary;
    private bool hasBeenCollected;

    private static readonly int AnimatorSpeed = Animator.StringToHash("Speed");

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        if (speechBubbleAnchor == null)
        {
            speechBubbleAnchor = transform;
        }
    }

    private void Update()
    {
        if (state == FriendState.CollectedFollowing)
        {
            HandleFollowing();
        }
    }

    private void HandleFollowing()
    {
        if (FriendManager.Instance == null)
            return;

        Vector3 targetPos = FriendManager.Instance.GetTargetPosition(this);
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        Vector3 velocity = Vector3.zero;

        if (distance > stopDistance)
        {
            Vector3 direction = toTarget.normalized;
            velocity = direction * followSpeed;

            // Move using CharacterController for basic collision handling.
            Vector3 movement = velocity * Time.deltaTime;
            movement.y = Physics.gravity.y * Time.deltaTime;
            characterController.Move(movement);

            // Rotate towards movement direction.
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // Update simple walk/idle animation based on horizontal speed.
        if (animator != null)
        {
            float speedValue = velocity.magnitude;
            animator.SetFloat(AnimatorSpeed, speedValue);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!hasBeenCollected)
        {
            ShowSpeechBubble();
            Collect();
        }
        else
        {
            // Already a follower: do nothing special.
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Close bubble when player leaves range.
        if (activeBubble != null)
        {
            activeBubble.Hide();
            activeBubble = null;
        }
    }

    private void ShowSpeechBubble()
    {
        if (speechBubblePrefab == null || ecoTipsDatabase == null)
            return;

        // Destroy old bubble if still active.
        if (activeBubble != null)
        {
            activeBubble.Hide();
        }

        string tip = ecoTipsDatabase.GetRandomTip();
        if (string.IsNullOrEmpty(tip))
            return;

        SpeechBubbleController bubbleInstance = Instantiate(
            speechBubblePrefab,
            speechBubbleAnchor.position,
            Quaternion.identity
        );

        bubbleInstance.Initialize(speechBubbleAnchor, tip);
        activeBubble = bubbleInstance;
    }

    private void Collect()
    {
        if (hasBeenCollected)
            return;

        hasBeenCollected = true;
        state = FriendState.CollectedFollowing;

        // Particle burst.
        if (collectParticles != null)
        {
            collectParticles.Play();
        }

        // +1 Friend popup.
        if (popupPrefab != null)
        {
            Vector3 spawnPos = transform.position + popupOffset;
            Instantiate(popupPrefab, spawnPos, Quaternion.identity);
        }

        // Notify manager (handles UI and visible followers).
        if (FriendManager.Instance != null)
        {
            FriendManager.Instance.RegisterCollectedFriend(this);
        }

        // We keep the speech bubble open until player exits trigger.
        // After that, OnTriggerExit will close it.
    }

    /// <summary>
    /// Called by FriendManager when this friend should no longer be
    /// an active visible follower (due to maxVisibleFriends).
    /// The friend stops following and is destroyed for now.
    /// </summary>
    public void DespawnFollower()
    {
        Destroy(gameObject);
    }
}

