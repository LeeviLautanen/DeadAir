using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMenuManager : MonoBehaviour
{
    public bool IsVisible
    {
        get { return isVisible; }
        set
        {
            SetVisible(value);
        }
    }

    private static readonly Logger log = new(nameof(BuildMenuManager));
    private InputHandler inputHandler;
    private BuildMenuInfo buildMenuInfo;
    private BuildingPlacer buildingPlacer;
    [SerializeField] private List<BuildMenuElement> allElements = new();
    private Canvas buildMenuCanvas;
    private BuildMenuElement selectedElement;
    private bool isVisible;

    private void Start()
    {
        buildMenuInfo = FindFirstObjectByType<BuildMenuInfo>();
        buildingPlacer = FindFirstObjectByType<BuildingPlacer>();
        buildMenuCanvas = GetComponent<Canvas>();

        // Get all nodes
        GetComponentsInChildren(allElements);
        BuildMenuElement.OnElementClicked += HandleElementClicked;

        // Input handling
        inputHandler = FindFirstObjectByType<InputHandler>();
        inputHandler.RegisterClickHandler(HandleMouseClick, 1);
        inputHandler.NumberKeyPressed += HandleNumberKey;

        SetVisible(true);
    }

    private void OnDestroy()
    {
        BuildMenuElement.OnElementClicked -= HandleElementClicked;

        if (inputHandler != null)
        {
            inputHandler.UnregisterClickHandler(HandleMouseClick);
            inputHandler.NumberKeyPressed -= HandleNumberKey;
        }
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;
        buildMenuCanvas.enabled = visible;
    }

    private bool HandleMouseClick(InputHandler.MouseClick click)
    {
        if (!IsVisible)
            return false;

        if (click.Button == InputHandler.MouseButton.Left)
        {
            if (buildingPlacer.TryPlaceGhost())
            {
                ClearSelected();
            }
            return true;
        }
        else if (click.Button == InputHandler.MouseButton.Right)
        {
            ClearSelected();
            return true;
        }

        return false;
    }

    private void HandleNumberKey(Key key)
    {
        if (!IsVisible)
            return;

        int index = System.Array.IndexOf(InputHandler.numberKeys, key);
        if (index >= 0 && index < allElements.Count)
        {
            BuildMenuElement elementToSelect = allElements[index];
            HandleElementClicked(elementToSelect);
        }
    }

    private void ClearSelected()
    {
        if (selectedElement != null)
        {
            selectedElement.IsSelected = false;
            selectedElement = null;
            buildMenuInfo.ClosePanel();
            buildingPlacer.ClearSelected();
        }
    }

    private void HandleElementClicked(BuildMenuElement clickedElement)
    {
        if (selectedElement != null)
        {
            selectedElement.IsSelected = false;
        }

        selectedElement = clickedElement;
        selectedElement.IsSelected = true;

        buildMenuInfo.OpenPanel(clickedElement.Data);
        buildingPlacer.SelectBuilding(clickedElement.Data.Id);

        log.Info($"Element clicked: {clickedElement.BuildingId}");
    }
}
