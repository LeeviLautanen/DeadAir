using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    [Header("Available Buildings")]
    public List<BuildingData> availableBuildings;

    private static readonly Logger log = new(nameof(BuildingManager));
    private ResourceManager resourceManager;
    private InputHandler inputHandler;
    private TechManager techManager;
    private Dictionary<string, BuildingData> buildingDatabase = new();
    [SerializeField] private List<Building> allBuildings = new();
    private readonly string saveFilePath = "./buildings.json";
    [SerializeField] private bool constructionCostsDisabled = false;
    private GameObject buildingContainer;

    private void Start()
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        inputHandler = FindFirstObjectByType<InputHandler>();
        techManager = FindFirstObjectByType<TechManager>();
        buildingContainer = GameObject.Find("BuildingContainer");

        foreach (var buildingData in availableBuildings)
        {
            buildingDatabase.Add(buildingData.Id, buildingData);
        }

        Building.OnCreated += OnBuildingCreated;
        Building.OnDestroyed += OnBuildingDestroyed;

        inputHandler.SaveActionTriggered += HandleSave;
    }

    public GameObject CreateBuilding(string buildingId, Vector3 position, Quaternion rotation = default)
    {
        if (!buildingDatabase.ContainsKey(buildingId))
        {
            log.Error($"Building with ID '{buildingId}' doesnt exist in building database");
            return null;
        }

        var buildingData = buildingDatabase[buildingId];
        if (!constructionCostsDisabled)
        {
            var constructionCosts = buildingData.ConstructionCost.ConvertAll(resource => new ResourceAmount(resource.Data, resource.Amount));
            foreach (var cost in constructionCosts)
            {
                cost.Amount = techManager.GetModifiedValue(cost.Amount, ModifierType.ConstructionCost, buildingData.Id);
            }
            if (!resourceManager.TryConsumeResources(constructionCosts))
            {
                log.Warning("Not enough resources to build " + buildingData.DisplayName);
                return null;
            }
        }

        GameObject newBuilding = Instantiate(buildingData.Prefab, position, rotation);
        newBuilding.transform.SetParent(buildingContainer.transform);

        log.Info($"Created {buildingData.DisplayName} at {position}");
        return newBuilding;
    }

    public BuildingData GetBuildingData(string buildingId)
    {
        return buildingDatabase.ContainsKey(buildingId) ? buildingDatabase[buildingId] : null;
    }

    public int GetBuildingCount(string buildingId)
    {
        int counter = 0;
        foreach (var building in allBuildings)
        {
            if (building.Data.Id == buildingId)
            {
                counter++;
            }
        }
        return counter;
    }

    private void HandleSave(SaveAction action)
    {
        switch (action)
        {
            case SaveAction.Save:
                SaveBuildings();
                break;

            case SaveAction.Load:
                LoadBuildings();
                break;

            case SaveAction.Clear:
                System.IO.File.Delete(saveFilePath);
                break;
        }
    }

    private void SaveBuildings()
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
        System.IO.File.WriteAllText(saveFilePath, json);
    }

    private void LoadBuildings()
    {
        if (!System.IO.File.Exists(saveFilePath))
        {
            log.Warning("No save file found for buildings.");
            return;
        }

        string json = System.IO.File.ReadAllText(saveFilePath);
        BuildingSaveDataList saveDataList = JsonUtility.FromJson<BuildingSaveDataList>(json);

        // Clear existing buildings
        List<Building> buildingsToDestroy = allBuildings.ToList();
        foreach (var buildingObj in buildingsToDestroy)
        {
            if (buildingObj.TryGetComponent(out Building buildingScript))
            {
                buildingScript.DestroyBuilding();
            }
        }
        allBuildings.Clear();

        // Instantiate buildings from save data
        bool previousState = constructionCostsDisabled;
        constructionCostsDisabled = true;
        foreach (var saveData in saveDataList.Buildings)
        {
            CreateBuilding(saveData.BuildingId, saveData.Position);
        }
        constructionCostsDisabled = previousState;
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
