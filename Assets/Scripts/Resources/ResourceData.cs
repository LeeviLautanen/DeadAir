using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
public class ResourceData : ScriptableObject
{
    [SerializeField] private string resourceId;
    [SerializeField] private string displayName;

    public string Id => resourceId;
    public string DisplayName => displayName;
}
