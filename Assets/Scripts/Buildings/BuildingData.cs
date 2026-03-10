using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Properties")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private int maxHealth = 100;
    [Range(0, 10)]
    [SerializeField] private int resourcePriority = 0;
    [SerializeField] private float startupTime = 1f;

    [Header("Visual")]
    [SerializeField] private GameObject prefab;

    [Header("Construction")]
    public List<ReadonlyResourceAmount> ConstructionCost;

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
        resourcePriority = Mathf.Clamp(resourcePriority, 0, 10);
    }

    // Getters
    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public int MaxHealth => maxHealth;
    public int ResourcePriority => resourcePriority;
    public float StartupTime => startupTime;
    public GameObject Prefab => prefab;
}
