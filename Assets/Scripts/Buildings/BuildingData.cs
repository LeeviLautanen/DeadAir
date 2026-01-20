using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Properties")]
    public string Id;
    public string DisplayName;
    public List<Dictionary<string, int>> ConstructionCost;
    public int MaxHealth = 100;

    [Header("Visual")]
    public GameObject Prefab;

    [Header("Production")]
    public List<Dictionary<string, int>> ProducedResources;

    [Header("Consumption")]
    public List<Dictionary<string, int>> ConsumedResources;
}
