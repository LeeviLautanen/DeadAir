using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

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
    private SpriteRenderer ghostSpriteRenderer;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();
        buildingManager = FindFirstObjectByType<BuildingManager>();
        buildingTypeText = GameObject.Find("BuildingTypeText").GetComponent<TMP_Text>();

        inputHandler.NumberKeyPressed += HandleNumberKey;
        inputHandler.RegisterClickHandler(HandleMouseClick, priority: 100);
    }

    private void HandleNumberKey(Key key)
    {
        switch (key)
        {
            case Key.Digit1:
                SelectBuilding("apartment");
                break;
            case Key.Digit2:
                SelectBuilding("refinery");
                break;
            case Key.Digit3:
                SelectBuilding("shield");
                break;
            case Key.Digit4:
                SelectBuilding("power_plant");
                break;
            case Key.Digit5:
                SelectBuilding("laboratory");
                break;
        }
    }

    private bool HandleMouseClick(InputHandler.MouseClick click)
    {
        // Ignore mouse if we arent placing a building
        if (!IsPlacing)
            return false;

        if (click.Button == InputHandler.MouseButton.Left)
        {
            TryPlaceGhost();
            return true;
        }
        else if (click.Button == InputHandler.MouseButton.Right)
        {
            ClearSelected();
            return true;
        }

        return false;
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

            if (!ghostBuilding.ValidBuildPlacement && ghostSpriteRenderer.color.a == 1.0f)
            {
                Color oldColor = ghostSpriteRenderer.color;
                oldColor.a = 0.5f;
                ghostSpriteRenderer.color = oldColor;
            }
            else if (ghostBuilding.ValidBuildPlacement && ghostSpriteRenderer.color.a == 0.5f)
            {
                Color oldColor = ghostSpriteRenderer.color;
                oldColor.a = 1.0f;
                ghostSpriteRenderer.color = oldColor;
            }
        }
    }

    private void SelectBuilding(string buildingId)
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
        ghostSpriteRenderer = ghostGO.GetComponentInChildren<SpriteRenderer>();
        ghostBuilding.PlacementMode = true;
    }

    private void TryPlaceGhost()
    {
        if (buildingManager == null || IsPlacing == false || !ghostBuilding.ValidBuildPlacement) return;

        Vector3 spawnPos = new(ghostGO.transform.position.x, 0, -1);
        if (buildingManager.CreateBuilding(currentBuildingId, spawnPos))
        {
            ClearSelected();
        }
    }

    private void ClearSelected()
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
