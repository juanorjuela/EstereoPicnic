using UnityEngine;
using TMPro;

/// <summary>
/// Updates the top-left friend counter HUD based on FriendManager events.
/// </summary>
public class FriendCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    private void OnEnable()
    {
        if (FriendManager.Instance != null)
        {
            FriendManager.Instance.OnFriendCountChanged.AddListener(HandleFriendCountChanged);
            // Initialize immediately.
            HandleFriendCountChanged(FriendManager.Instance.TotalFriendsCollected);
        }
    }

    private void OnDisable()
    {
        if (FriendManager.Instance != null)
        {
            FriendManager.Instance.OnFriendCountChanged.RemoveListener(HandleFriendCountChanged);
        }
    }

    private void HandleFriendCountChanged(int count)
    {
        if (counterText == null)
            return;

        counterText.text = count.ToString();
    }
}

