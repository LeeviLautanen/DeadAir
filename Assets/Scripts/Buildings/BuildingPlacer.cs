using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BuildingPlacer : MonoBehaviour
{
    public bool IsPlacing { get; private set; }

    private BuildingManager buildingManager;
    private InputHandler inputHandler;
    private string currentBuildingId;
    private BuildingData selectedBuildingData;
    private TMP_Text buildingTypeText;
    private GameObject ghostGO;

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
                Vector3 ghostPos = new(Mathf.Round(mousePos.x), ghostGO.transform.position.y, -1);
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

        ghostGO = new("BuildingGhost");
        SpriteRenderer buildingGhost = ghostGO.AddComponent<SpriteRenderer>();

        GameObject buildingPrefab = buildingManager.availableBuildings.Find(b => b.Id == buildingId).Prefab;
        GameObject spriteGO = buildingPrefab.GetComponentInChildren<SpriteRenderer>().gameObject;
        ghostGO.transform.localScale = spriteGO.transform.localScale;
        ghostGO.transform.position = spriteGO.transform.position;
        buildingGhost.sprite = buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        buildingGhost.sortingOrder = 1000;
    }

    public void TryPlaceGhost()
    {
        if (buildingManager == null || IsPlacing == false) return;

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
