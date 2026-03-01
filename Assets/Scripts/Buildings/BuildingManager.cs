using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("Available Buildings")]
    public List<BuildingData> availableBuildings;

    private static readonly Logger log = new(true, LogLevel.Warning);
    private Dictionary<string, BuildingData> buildingDatabase;
    [SerializeField] private List<Building> allBuildings;
    private ResourceManager resourceManager;

    private void Start()
    {
        InitializeBuildingDatabase();
        allBuildings = new();
        resourceManager = GetComponent<ResourceManager>();

        Building.OnCreated += OnBuildingCreated;
        Building.OnDestroyed += OnBuildingDestroyed;
    }

    public GameObject CreateBuilding(string buildingId, Vector3 position, Quaternion rotation = default)
    {
        if (!buildingDatabase.ContainsKey(buildingId))
        {
            log.Error($"Building with ID '{buildingId}' doesnt exist in building database");
            return null;
        }

        var buildingData = buildingDatabase[buildingId];
        if (!resourceManager.TryConsumeResources(buildingData.ConstructionCost))
        {
            log.Warning("Not enough resources to build " + buildingData.DisplayName);
            return null;
        }

        GameObject newBuilding = Instantiate(buildingData.Prefab, position, rotation);

        log.Info($"Created {buildingData.DisplayName} at {position}");
        return newBuilding;
    }

    public BuildingData GetBuildingData(string buildingId)
    {
        return buildingDatabase.ContainsKey(buildingId) ? buildingDatabase[buildingId] : null;
    }

    public void SaveBuildings()
    {
        BuildingSaveDataList saveDataList = new();
        foreach (var buildingObj in allBuildings)
        {
            if (buildingObj.TryGetComponent(out Building buildingScript))
            {
                Vector3 position = buildingObj.transform.position;
                saveDataList.Buildings.Add(new BuildingSaveData(buildingScript.Id, position));
            }
        }
        string json = JsonUtility.ToJson(saveDataList);
        System.IO.File.WriteAllText("./buildings.json", json);
    }

    public void LoadBuildings()
    {
        string path = "./buildings.json";
        if (!System.IO.File.Exists(path))
        {
            log.Warning("No save file found for buildings.");
            return;
        }

        string json = System.IO.File.ReadAllText(path);
        BuildingSaveDataList saveDataList = JsonUtility.FromJson<BuildingSaveDataList>(json);

        // Clear existing buildings
        foreach (var buildingObj in allBuildings)
        {
            buildingObj.TryGetComponent(out Building buildingScript);
            buildingScript.DestroyBuilding();
        }
        allBuildings.Clear();

        // Instantiate buildings from save data
        foreach (var saveData in saveDataList.Buildings)
        {
            CreateBuilding(saveData.BuildingId, saveData.Position);
        }
    }

    public List<Building> GetBuildingsByIds(List<string> buildingIds)
    {
        List<Building> matchingBuildings = new();
        foreach (var building in allBuildings)
        {
            log.Info(building.Id);
            if (buildingIds.Contains(building.Id))
            {
                matchingBuildings.Add(building);
            }
        }
        return matchingBuildings;
    }

    private void OnBuildingCreated(Building building)
    {
        allBuildings.Add(building);
        building.Activate(); // Activate building by default
    }

    private void OnBuildingDestroyed(Building building)
    {
        allBuildings.Remove(building);
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

[System.Serializable]
public class BuildingSaveDataList
{
    public List<BuildingSaveData> Buildings = new();
}

[System.Serializable]
public class BuildingSaveData
{
    public string BuildingId;
    public Vector3 Position;

    public BuildingSaveData(string buildingId, Vector3 position)
    {
        BuildingId = buildingId;
        Position = position;
    }
}
