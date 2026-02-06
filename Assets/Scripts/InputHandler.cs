using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float accelerationSpeed = 50f;
    public Vector3 MouseWorldPosition { get; private set; }

    private Camera mainCamera;
    private BuildingPlacer buildingPlacer;

    private void Start()
    {
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        buildingPlacer = FindFirstObjectByType<BuildingPlacer>();
    }

    private void Update()
    {
        MouseWorldPosition = MousePosInWorld();
        HandleNumberKeys();
        HandleMouseClicks();
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
    }
}
