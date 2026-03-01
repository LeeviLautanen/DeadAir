using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float accelerationSpeed = 50f;
    public Vector3 MouseWorldPosition { get; private set; }

    private Camera mainCamera;
    private BuildingPlacer buildingPlacer;
    private BuildingManager buildingManager;

    private void Start()
    {
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        buildingPlacer = FindFirstObjectByType<BuildingPlacer>();
        buildingManager = FindFirstObjectByType<BuildingManager>();
    }

    private void Update()
    {
        MouseWorldPosition = MousePosInWorld();
        HandleNumberKeys();
        HandleMouseClicks();
        HandleSaveControls();
    }

    private Vector3 MousePosInWorld()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
    }

    private void HandleMouseClicks()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (buildingPlacer.IsPlacing)
            {
                buildingPlacer.TryPlaceGhost();
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (buildingPlacer.IsPlacing)
            {
                buildingPlacer.ClearSelected();
            }
        }
    }

    private void HandleNumberKeys()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            buildingPlacer.SelectBuilding("apartment");
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            buildingPlacer.SelectBuilding("refinery");
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            buildingPlacer.SelectBuilding("shield");
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            buildingPlacer.SelectBuilding("power_plant");
        }
        else if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            buildingPlacer.SelectBuilding("laboratory");
        }
    }

    private void HandleSaveControls()
    {
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.sKey.wasPressedThisFrame)
        {
            buildingManager.SaveBuildings();
        }
        else if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.rKey.wasPressedThisFrame)
        {
            buildingManager.LoadBuildings();
        }
        else if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.cKey.wasPressedThisFrame)
        {
            System.IO.File.Delete("./buildings.json");
        }
    }
}
