using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager that tracks collected friend NPCs,
/// assigns formation slots, and exposes events for UI.
/// </summary>
public class FriendManager : MonoBehaviour
{
    public static FriendManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PLAYER_CONTROLLER playerController;

    [Header("Formation Settings")]
    [Tooltip("Base distance behind the player where the first row forms.")]
    [SerializeField] private float baseFollowDistance = 1.5f;

    [Tooltip("Extra distance added per row behind the player.")]
    [SerializeField] private float rowSpacing = 1.0f;

    [Tooltip("Side-to-side spacing between friends in the same row.")]
    [SerializeField] private float lateralSpacing = 1.0f;

    [Tooltip("How many friends per row in the formation.")]
    [SerializeField] private int friendsPerRow = 5;

    [Tooltip("Random position noise added to each friend for a soft swarm look.")]
    [SerializeField] private float positionNoiseRadius = 0.4f;

    [Header("Limits")]
    [Tooltip("Maximum number of friend NPCs visible at once.")]
    [SerializeField] private int maxVisibleFriends = 30;

    [Tooltip("Seed for random position noise. Use 0 for fully random each play.")]
    [SerializeField] private int noiseSeed = 12345;

    [Header("Events")]
    public UnityEvent<int> OnFriendCountChanged;
    public UnityEvent<FriendNPC> OnFriendCollected;

    // Total number of friends collected over time (for UI display).
    public int TotalFriendsCollected { get; private set; }

    // Friends that are currently spawned and following (maxVisibleFriends).
    private readonly List<FriendNPC> activeFollowers = new List<FriendNPC>();

    // Internal noise offsets per friend index to keep positions stable.
    private readonly Dictionary<FriendNPC, Vector3> noiseOffsets = new Dictionary<FriendNPC, Vector3>();

    private System.Random noiseRandom;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (noiseSeed != 0)
        {
            noiseRandom = new System.Random(noiseSeed);
        }
        else
        {
            noiseRandom = new System.Random();
        }
    }

    private void Start()
    {
        if (playerTransform == null || playerController == null)
        {
            var player = FindObjectOfType<PLAYER_CONTROLLER>();
            if (player != null)
            {
                playerController = player;
                playerTransform = player.transform;
            }
        }

        OnFriendCountChanged ??= new UnityEvent<int>();
        OnFriendCollected ??= new UnityEvent<FriendNPC>();

        // Initialize UI with zero friends.
        OnFriendCountChanged.Invoke(TotalFriendsCollected);
    }

    /// <summary>
    /// Called by FriendNPC when it is collected.
    /// Handles UI count and visible follower list (with maxVisibleFriends).
    /// </summary>
    public void RegisterCollectedFriend(FriendNPC friend)
    {
        if (friend == null)
            return;

        TotalFriendsCollected++;
        OnFriendCountChanged.Invoke(TotalFriendsCollected);
        OnFriendCollected.Invoke(friend);

        // Manage visible followers (maxVisibleFriends limit).
        if (!activeFollowers.Contains(friend))
        {
            if (activeFollowers.Count >= maxVisibleFriends)
            {
                // Remove the oldest follower visually.
                FriendNPC toRemove = activeFollowers[0];
                activeFollowers.RemoveAt(0);
                if (toRemove != null)
                {
                    toRemove.DespawnFollower();
                    noiseOffsets.Remove(toRemove);
                }
            }

            activeFollowers.Add(friend);
            EnsureNoiseOffset(friend);
        }
    }

    /// <summary>
    /// Gets the desired world position for a given friend.
    /// </summary>
    public Vector3 GetTargetPosition(FriendNPC friend)
    {
        if (playerTransform == null || friend == null)
            return friend.transform.position;

        int index = activeFollowers.IndexOf(friend);
        if (index < 0)
        {
            return friend.transform.position;
        }

        int row = index / Mathf.Max(1, friendsPerRow);
        int col = index % Mathf.Max(1, friendsPerRow);

        // Center friends horizontally around the player.
        float middle = (friendsPerRow - 1) / 2f;

        Vector3 back = -playerTransform.forward;
        back.y = 0f;
        back.Normalize();

        Vector3 right = playerTransform.right;
        right.y = 0f;
        right.Normalize();

        float followDistance = baseFollowDistance + row * rowSpacing;
        Vector3 basePos = playerTransform.position + back * followDistance;
        Vector3 lateralOffset = right * ((col - middle) * lateralSpacing);

        Vector3 target = basePos + lateralOffset;

        // Add soft noise for swarm look.
        EnsureNoiseOffset(friend);
        target += noiseOffsets[friend];

        return target;
    }

    private void EnsureNoiseOffset(FriendNPC friend)
    {
        if (friend == null)
            return;

        if (!noiseOffsets.ContainsKey(friend))
        {
            float x = (float)(noiseRandom.NextDouble() * 2.0 - 1.0);
            float z = (float)(noiseRandom.NextDouble() * 2.0 - 1.0);
            Vector3 offset = new Vector3(x, 0f, z).normalized * positionNoiseRadius;
            noiseOffsets[friend] = offset;
        }
    }
}

