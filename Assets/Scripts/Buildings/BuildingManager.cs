using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("Available Buildings")]
    public List<BuildingData> availableBuildings;

    private Dictionary<string, BuildingData> buildingDatabase;
    private List<GameObject> instantiatedBuildings;
    private ResourceManager resourceManager;
    private List<Building>[] resourceEntityLists = new List<Building>[10];
    private List<Building>[] reservationBuildingList = new List<Building>[10];

    private void Start()
    {
        InitializeBuildingDatabase();
        instantiatedBuildings = new List<GameObject>();
        resourceManager = GetComponent<ResourceManager>();

        Building.OnBuildingDestroyed += DestroyBuilding;
        Building.OnBuildingActivated += HandleBuildingActivation;
        Building.OnBuildingDeactivated += HandleBuildingActivation;
    }

    private void Update()
    {
        foreach (var entityList in resourceEntityLists)
        {
            foreach (var entity in entityList)
            {
                if (!entity.IsActive) continue;
                resourceManager.AddResourceRates(entity.Data.ProducedResources);
            }
        }

        foreach (var entityList in resourceEntityLists)
        {
            foreach (var entity in entityList)
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
        instantiatedBuildings.Add(newBuilding);

        // Add building to the resource entity list based on its consumption priority
        resourceEntityLists[buildingData.ConsumptionPriority].Add(buildingScript);

        // Add building to the reservation list based on its consumption priority
        reservationBuildingList[buildingData.ConsumptionPriority].Add(buildingScript);

        // Activate the new building
        buildingScript.Activate();

        //Debug.Log($"Created {buildingData.DisplayName} at {position}");
        return newBuilding;
    }

    public BuildingData GetBuildingData(string buildingId)
    {
        return buildingDatabase.ContainsKey(buildingId) ? buildingDatabase[buildingId] : null;
    }

    private void DestroyBuilding(Building building)
    {
        if (instantiatedBuildings.Contains(building.gameObject))
        {
            resourceEntityLists[building.Data.ConsumptionPriority].Remove(building);
            instantiatedBuildings.Remove(building.gameObject);
            Destroy(building.gameObject);
        }
    }

    private void HandleBuildingActivation(Building building)
    {
        // Reserve resources (badly implemented)
        foreach (var reservation in building.Data.ReservationEffects)
        {
            if (!resourceManager.TryReserveResource(reservation))
            {
                return;
            }
        }

        // Add effects to capacity
        foreach (var effect in building.Data.CapacityEffects)
        {
            resourceManager.ChangeResourceMax(effect.Data.Id, effect.Amount);
        }

    }

    private void HandleBuildingDeactivation(Building building)
    {
        // Remove capacity effects
        foreach (var effect in building.Data.CapacityEffects)
        {
            resourceManager.ChangeResourceMax(effect.Data.Id, -effect.Amount);
        }

        // Release reserved resources
        foreach (var reservation in building.Data.ReservationEffects)
        {
            resourceManager.ReleaseReservation(reservation);
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
