using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("Available Buildings")]
    public List<BuildingData> availableBuildings;

    private Dictionary<string, BuildingData> buildingDatabase;
    private List<GameObject> instantiatedBuildings;
    private ResourceManager resourceManager;
    private SortedDictionary<int, List<Building>> resourceEntityLists = new();

    private void Start()
    {
        InitializeBuildingDatabase();
        instantiatedBuildings = new List<GameObject>();
        resourceManager = GetComponent<ResourceManager>();

        Building.OnBuildingDestroyed += DestroyBuilding;
    }

    private void Update()
    {
        foreach (var entityList in resourceEntityLists)
        {
            foreach (var entity in entityList.Value)
            {
                if (!entity.IsActive) continue;
                resourceManager.AddResourceRates(entity.Data.ProducedResources);
            }
        }

        foreach (var entityList in resourceEntityLists)
        {
            foreach (var entity in entityList.Value)
            {
                if (resourceManager.TryConsumeResourceRates(entity.Data.ConsumedResources))
                {
                    if (!entity.IsActive) entity.Activate();
                }
                else
                {
                    if (entity.IsActive) entity.Deactivate();
                }
            }
        }

        resourceManager.SmoothResources();
    }

    public GameObject CreateBuilding(string buildingId, Vector3 position, Quaternion rotation = default)
    {
        if (!buildingDatabase.ContainsKey(buildingId))
        {
            Debug.LogError($"Building with ID '{buildingId}' not found!");
            return null;
        }

        var buildingData = buildingDatabase[buildingId];

        if (!resourceManager.TryConsumeResources(buildingData.ConstructionCost))
        {
            Debug.LogWarning("Not enough resources to build " + buildingData.DisplayName);
            return null;
        }

        GameObject newBuilding = Instantiate(buildingData.Prefab, position, rotation);

        // Get or add building component and initialize with data
        newBuilding.TryGetComponent(out Building buildingScript);
        if (buildingScript == null)
        {
            Debug.LogError("Building prefab does not have a Building script");
            return null;
        }

        // Initialize the building with its data
        buildingScript.Initialize(buildingData);

        // Add effects to capacity
        foreach (var effect in buildingData.CapacityEffects)
        {
            resourceManager.ChangeResourceMax(effect.Data.Id, effect.Amount);
        }

        instantiatedBuildings.Add(newBuilding);

        if (!resourceEntityLists.ContainsKey(buildingData.ConsumptionPriority))
        {
            resourceEntityLists[buildingData.ConsumptionPriority] = new List<Building>();
        }
        resourceEntityLists[buildingData.ConsumptionPriority].Add(buildingScript);

        //Debug.Log($"Created {buildingData.DisplayName} at {position}");
        return newBuilding;
    }

    public BuildingData GetBuildingData(string buildingId)
    {
        return buildingDatabase.ContainsKey(buildingId) ? buildingDatabase[buildingId] : null;
    }

    public void DestroyBuilding(GameObject building)
    {
        if (instantiatedBuildings.Contains(building))
        {
            instantiatedBuildings.Remove(building);
            Destroy(building);
        }
    }

    private void InitializeBuildingDatabase()
    {
        buildingDatabase = new Dictionary<string, BuildingData>();
        foreach (var building in availableBuildings)
        {
            buildingDatabase[building.Id] = building;
        }
    }
}
