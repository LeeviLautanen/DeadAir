using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Properties")]
    public string Id;
    public string DisplayName;
    public List<ResourceStack> ConstructionCost;
    public int MaxHealth = 100;

    [Header("Visual")]
    public GameObject Prefab;

    [Header("Production")]
    public List<ResourceStack> ProducedResources;

    [Header("Consumption")]
    public List<ResourceStack> ConsumedResources;

    [Header("Capacity effects")]
    public List<ResourceStack> CapacityEffects;
}
