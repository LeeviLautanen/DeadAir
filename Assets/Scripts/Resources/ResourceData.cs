using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/Resource Data")]
public class ResourceData : ScriptableObject
{
    [SerializeField] private string resourceId; // Unique identifier
    [SerializeField] private string displayName; // User-friendly name
    [SerializeField] private int defaultMaxAmount = 100; // Default maximum amount

    // Properties with getters (read-only outside the class)
    public string Id => resourceId;
    public string DisplayName => displayName;
    public int DefaultMaxAmount => defaultMaxAmount;
}
