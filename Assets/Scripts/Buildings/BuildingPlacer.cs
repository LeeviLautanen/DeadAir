using UnityEngine;
using TMPro;

public class BuildingPlacer : MonoBehaviour
{
    public bool IsPlacing { get; private set; }

    private BuildingManager buildingManager;
    private InputHandler inputHandler;
    private string currentBuildingId;
    private BuildingData selectedBuildingData;
    private TMP_Text buildingTypeText;
    private GameObject buildingGhost;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();
        buildingManager = FindFirstObjectByType<BuildingManager>();
        buildingTypeText = GameObject.Find("BuildingTypeText").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (IsPlacing)
        {
            Vector3 mousePos = inputHandler.MouseWorldPosition;
            if (buildingGhost != null)
            {
                buildingGhost.transform.position = new Vector3(Mathf.Round(mousePos.x), 0, -1);
            }
        }
    }

    public void SelectBuilding(string buildingId)
    {
        if (buildingManager == null || buildingId == currentBuildingId) return;

        selectedBuildingData = buildingManager.GetBuildingData(buildingId);


        if (selectedBuildingData == null)
        {
            Debug.LogError($"No building data found for id: {buildingId}");
            return;
        }

        if (IsPlacing || buildingGhost != null) ClearSelected();

        IsPlacing = true;
        currentBuildingId = buildingId;
        buildingTypeText.text = selectedBuildingData.DisplayName;
        buildingGhost = Instantiate(selectedBuildingData.Prefab);

        // Not needed, just to shut up errors
        buildingGhost.GetComponent<Building>().Initialize(selectedBuildingData);

    }

    public void TryPlaceBuilding(int x)
    {
        if (buildingManager == null || IsPlacing == false) return;

        Vector3 spawnPos = new(x, 0, -1);
        buildingManager.CreateBuilding(currentBuildingId, spawnPos);
        ClearSelected();
    }

    public void ClearSelected()
    {
        IsPlacing = false;
        currentBuildingId = null;
        buildingTypeText.text = "";
        if (buildingGhost != null)
        {
            Destroy(buildingGhost);
        }
    }
}
