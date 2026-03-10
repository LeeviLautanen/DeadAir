using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Properties")]
    public string Id;
    public string DisplayName;
    public List<ReadonlyResourceAmount> ConstructionCost;
    public int MaxHealth = 100;
    [Range(0, 10)]
    public int ResourcePriority = 0;
    public float StartupTime = 1f;

    [Header("Visual")]
    public GameObject Prefab;

    [Header("Production")]
    public List<ReadonlyResourceAmount> ProducedResources;

    [Header("Consumption")]
    public List<ReadonlyResourceAmount> ConsumedResources;

    [Header("Capacity effects")]
    public List<ReadonlyResourceAmount> CapacityEffects;

    [Header("Resources reserved when active")]
    public List<ReadonlyResourceAmount> RequiredReservations;

    // Limit the consumption priority to fit allowed range
    private void OnValidate()
    {
        ResourcePriority = Mathf.Clamp(ResourcePriority, 0, 10);
    }
}
