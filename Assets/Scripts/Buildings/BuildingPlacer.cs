using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class BuildingPlacer : MonoBehaviour
{
    public bool IsPlacing => isPlacing;

    private static readonly Logger log = new(nameof(BuildingPlacer));
    private BuildingManager buildingManager;
    private InputHandler inputHandler;
    private string currentBuildingId;
    private BuildingData selectedBuildingData;
    private GameObject ghostGO;
    private Building ghostBuilding;
    private Material ghostNormalMat;
    private Material ghostInvalidPlacementMat;
    private SpriteRenderer ghostSpriteRenderer;
    private bool isPlacing;
    private bool isInvalidVisual;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();
        buildingManager = FindFirstObjectByType<BuildingManager>();

        ghostNormalMat = new(Shader.Find("Sprites/Default"));
        ghostInvalidPlacementMat = new(Shader.Find("Custom/InvalidPlacementShader"));
    }

    private void Update()
    {
        if (IsPlacing && ghostGO != null)
        {
            Vector2 mousePos = inputHandler.MouseWorldPosition;
            Vector3 ghostPos = new(mousePos.x, ghostGO.transform.position.y, -3);
            ghostGO.transform.position = ghostPos;

            bool isValidPlacement = ghostBuilding.IsValidPlacement();
            if (!isValidPlacement && !isInvalidVisual)
            {
                ghostSpriteRenderer.material = ghostInvalidPlacementMat;
                isInvalidVisual = true;
            }
            else if (isValidPlacement && isInvalidVisual)
            {
                ghostSpriteRenderer.material = ghostNormalMat;
                isInvalidVisual = false;
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

        if (isPlacing || ghostGO != null) ClearSelected();

        isPlacing = true;
        currentBuildingId = buildingId;

        GameObject buildingPrefab = buildingManager.GetBuildingData(buildingId).Prefab;
        ghostGO = Instantiate(buildingPrefab);
        foreach (Collider2D collider in ghostGO.GetComponentsInChildren<Collider2D>())
        {
            collider.TryGetComponent<BuildingCollider>(out var buildingCollider);
            if (buildingCollider == null || buildingCollider.Type != BuildingColliderType.Placement)
            {
                log.Info("Disabling collider: " + collider.gameObject.name);
                collider.enabled = false;
            }
        }
        ghostSpriteRenderer = ghostGO.GetComponentInChildren<SpriteRenderer>();
        ghostBuilding = ghostGO.GetComponent<Building>();
        ghostBuilding.PlacementMode = true;
    }

    public bool TryPlaceGhost()
    {
        if (buildingManager == null || IsPlacing == false || !ghostBuilding.IsValidPlacement())
        {
            return false;
        }

        Vector3 spawnPos = new(ghostGO.transform.position.x, 0, -2);
        if (buildingManager.CreateBuilding(currentBuildingId, spawnPos))
        {
            return true;
        }
        return false;
    }

    public void ClearSelected()
    {
        isPlacing = false;
        currentBuildingId = null;
        isInvalidVisual = false;
        if (ghostGO != null)
        {
            Destroy(ghostGO);
        }
    }
}
