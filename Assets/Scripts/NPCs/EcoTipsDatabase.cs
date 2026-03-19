using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple ScriptableObject that stores a list of eco-tips
/// and can return a random one.
/// </summary>
[CreateAssetMenu(fileName = "EcoTipsDatabase", menuName = "Game/Eco Tips Database")]
public class EcoTipsDatabase : ScriptableObject
{
    [TextArea(2, 5)]
    [SerializeField] private List<string> tips = new List<string>();

    /// <summary>
    /// Returns a random eco-tip from the list.
    /// If the list is empty, returns an empty string.
    /// </summary>
    public string GetRandomTip()
    {
        if (tips == null || tips.Count == 0)
        {
            return string.Empty;
        }

        int index = Random.Range(0, tips.Count);
        return tips[index];
    }
}

