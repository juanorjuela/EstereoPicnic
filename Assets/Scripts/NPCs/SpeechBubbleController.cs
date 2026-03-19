using UnityEngine;
using TMPro;

/// <summary>
/// Controls a world-space speech bubble above an NPC.
/// It follows a target anchor and always faces the main camera.
/// </summary>
public class SpeechBubbleController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Canvas bubbleCanvas;

    [Header("Follow Settings")]
    [Tooltip("Offset above the anchor, in world units.")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private Transform targetAnchor;
    private Camera mainCamera;

    public void Initialize(Transform anchor, string message)
    {
        targetAnchor = anchor;
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (bubbleCanvas != null)
        {
            bubbleCanvas.worldCamera = Camera.main;
        }

        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetAnchor == null)
        {
            Destroy(gameObject);
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Follow anchor
        transform.position = targetAnchor.position + worldOffset;

        // Face camera (billboard)
        if (mainCamera != null)
        {
            Vector3 direction = transform.position - mainCamera.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    public void Hide()
    {
        Destroy(gameObject);
    }
}

