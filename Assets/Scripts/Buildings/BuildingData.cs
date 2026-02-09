using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Properties")]
    public string Id;
    public string DisplayName;
    public List<ResourceAmount> ConstructionCost;
    public int MaxHealth = 100;

    [Header("Visual")]
    public GameObject Prefab;

    [Header("Production")]
    public List<ResourceAmount> ProducedResources;

    [Header("Consumption")]
    public List<ResourceAmount> ConsumedResources;

    [Header("Capacity effects")]
    public List<ResourceAmount> CapacityEffects;
}
