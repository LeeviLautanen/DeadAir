using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BuildingPlacer : MonoBehaviour
{
    public bool IsPlacing { get; private set; }

    private static readonly Logger log = new(true, LogLevel.Warning);
    private BuildingManager buildingManager;
    private InputHandler inputHandler;
    private string currentBuildingId;
    private BuildingData selectedBuildingData;
    private TMP_Text buildingTypeText;
    private GameObject ghostGO;
    private Building ghostBuilding;

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
            if (ghostGO != null)
            {
                Vector3 ghostPos = new(mousePos.x, ghostGO.transform.position.y, -3);
                ghostGO.transform.position = ghostPos;
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

        if (IsPlacing || ghostGO != null) ClearSelected();

        IsPlacing = true;
        currentBuildingId = buildingId;
        buildingTypeText.text = selectedBuildingData.DisplayName;

        GameObject buildingPrefab = buildingManager.availableBuildings.Find(b => b.Id == buildingId).Prefab;
        ghostGO = Instantiate(buildingPrefab);
        foreach (Collider2D collider in ghostGO.GetComponentsInChildren<Collider2D>())
        {
            if (collider.gameObject.layer != LayerMask.NameToLayer("Placement"))
            {
                log.Info("Disabling collider: " + collider.gameObject.name);
                collider.enabled = false;
            }
        }
        ghostBuilding = ghostGO.GetComponent<Building>();
    }

    public void TryPlaceGhost()
    {
        if (buildingManager == null || IsPlacing == false || !ghostBuilding.ValidBuildPlacement) return;

        Vector3 spawnPos = new(ghostGO.transform.position.x, 0, -1);
        buildingManager.CreateBuilding(currentBuildingId, spawnPos);
        ClearSelected();
    }

    public void ClearSelected()
    {
        IsPlacing = false;
        currentBuildingId = null;
        buildingTypeText.text = "";
        if (ghostGO != null)
        {
            Destroy(ghostGO);
        }
    }
}
