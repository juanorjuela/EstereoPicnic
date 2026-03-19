using UnityEngine;
using TMPro;

/// <summary>
/// Simple floating text popup used for feedback like "+1 Friend".
/// </summary>
public class FloatingTextPopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float riseSpeed = 1.0f;

    private float timer;

    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshPro>();
        }

        if (textMesh != null && string.IsNullOrEmpty(textMesh.text))
        {
            textMesh.text = "+1 Friend";
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

