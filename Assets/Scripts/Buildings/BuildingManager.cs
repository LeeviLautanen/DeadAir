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
        newBuilding.TryGetComponent(out Building buildingScript);
        if (buildingScript == null)
        {
            Debug.LogError("Building prefab does not have a Building script");
            return null;
        }

        // Initialize the building with its data
        buildingScript.Initialize(buildingData);
        instantiatedBuildings.Add(newBuilding);

        // Activate the new building
        buildingScript.Activate();

        //Debug.Log($"Created {buildingData.DisplayName} at {position}");
        return newBuilding;
    }

    public BuildingData GetBuildingData(string buildingId)
    {
        return buildingDatabase.ContainsKey(buildingId) ? buildingDatabase[buildingId] : null;
    }

    public void SaveBuildings()
    {
        BuildingSaveDataList saveDataList = new();
        foreach (var buildingObj in instantiatedBuildings)
        {
            if (buildingObj.TryGetComponent(out Building buildingScript))
            {
                BuildingData data = buildingScript.Data;
                Vector3 position = buildingObj.transform.position;
                saveDataList.Buildings.Add(new BuildingSaveData(data.Id, position));
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
            Debug.LogWarning("No save file found for buildings.");
            return;
        }

        string json = System.IO.File.ReadAllText(path);
        BuildingSaveDataList saveDataList = JsonUtility.FromJson<BuildingSaveDataList>(json);

        // Clear existing buildings
        foreach (var buildingObj in instantiatedBuildings)
        {
            buildingObj.TryGetComponent(out Building buildingScript);
            buildingScript.DestroyBuilding();
        }
        instantiatedBuildings.Clear();

        // Instantiate buildings from save data
        foreach (var saveData in saveDataList.Buildings)
        {
            CreateBuilding(saveData.BuildingId, saveData.Position);
        }
    }

    private void DestroyBuilding(Building building)
    {
        if (instantiatedBuildings.Contains(building.gameObject))
        {
            instantiatedBuildings.Remove(building.gameObject);
            Destroy(building.gameObject);
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