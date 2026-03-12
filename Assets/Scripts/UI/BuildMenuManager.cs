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

    private static readonly Logger log = new(true, LogLevel.Info);
    private InputHandler inputHandler;
    [SerializeField] private List<BuildMenuElement> allElements = new();
    private Canvas buildMenuCanvas;
    private BuildMenuElement selectedElement;
    private bool isVisible;

    private void Start()
    {
        buildMenuCanvas = GetComponent<Canvas>();
        SetVisible(true);

        // Get all nodes
        GetComponentsInChildren(allElements);
        BuildMenuElement.OnElementClicked += HandleElementClicked;

        // Input handling
        inputHandler = FindFirstObjectByType<InputHandler>();
        inputHandler.RegisterClickHandler(HandleMouseClick, 0);
        inputHandler.NumberKeyPressed += HandleNumberKey;
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

        if (click.Button == InputHandler.MouseButton.Right && selectedElement != null)
        {
            selectedElement.IsSelected = false;
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

    private void HandleElementClicked(BuildMenuElement clickedElement)
    {
        if (selectedElement != null)
        {
            selectedElement.IsSelected = false;
        }

        selectedElement = clickedElement;
        selectedElement.IsSelected = true;
        log.Info($"Element clicked: {clickedElement.BuildingId}");
    }
}
