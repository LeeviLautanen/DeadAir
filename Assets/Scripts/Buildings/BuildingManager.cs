using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("Available Buildings")]
    public List<BuildingData> availableBuildings;

    private Dictionary<string, BuildingData> buildingDatabase;
    private List<GameObject> instantiatedBuildings;
    private ResourceManager resourceManager;

    private void Start()
    {
        InitializeBuildingDatabase();
        instantiatedBuildings = new List<GameObject>();
        resourceManager = GetComponent<ResourceManager>();

        Building.OnBuildingDestroyed += DestroyBuilding;
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
        newBuilding.TryGetComponent(out Building buildingComponent);
        if (buildingComponent == null)
        {
            Debug.LogError("Building prefab does not have a Building script");
            return null;
        }

        // Initialize the building with its data
        buildingComponent.Initialize(buildingData);

        // Add effects to capacity
        foreach (var effect in buildingData.CapacityEffects)
        {
            resourceManager.ChangeResourceMax(effect.Data.Id, effect.Amount);
        }

        instantiatedBuildings.Add(newBuilding);

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

    public void ProcessBuildingResources()
    {
        foreach (var buildingObj in instantiatedBuildings)
        {
            if (buildingObj.TryGetComponent(out Building building))
            {
                var data = building.Data;
                if (data == null)
                {
                    Debug.LogError("No building data found for production processing.");
                    continue;
                }

                resourceManager.AddResources(data.ProducedResources);
                resourceManager.TryConsumeResources(data.ConsumedResources);
            }
        }

        resourceManager.SmoothResources();

        return;
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
