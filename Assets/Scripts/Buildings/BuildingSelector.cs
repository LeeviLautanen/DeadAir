using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSelector : MonoBehaviour
{
    private static readonly Logger log = new(true, LogLevel.Info);
    private InputHandler inputHandler;
    private Building current;
    private Canvas infoCanvas;
    private GameObject infoContainer;
    private TMP_Text nameText;
    private TMP_Text statusText;
    private TMP_Text integrityText;
    private TMP_Text consumedResourcesText;
    private TMP_Text producedResourcesText;
    private TMP_Text capacityText;
    private TMP_Text reservationsText;
    private UnityEngine.UI.Button destroyButton;
    private UnityEngine.UI.Button activateButton;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();

        infoCanvas = gameObject.GetComponent<Canvas>();
        infoContainer = gameObject.transform.Find("InfoContainer").gameObject;

        nameText = infoContainer.transform.Find("NameText").GetComponent<TMP_Text>();
        statusText = infoContainer.transform.Find("StatusText").GetComponent<TMP_Text>();
        integrityText = infoContainer.transform.Find("IntegrityText").GetComponent<TMP_Text>();
        consumedResourcesText = infoContainer.transform.Find("ConsumedResourcesText").GetComponent<TMP_Text>();
        producedResourcesText = infoContainer.transform.Find("ProducedResourcesText").GetComponent<TMP_Text>();
        capacityText = infoContainer.transform.Find("CapacityEffectsText").GetComponent<TMP_Text>();
        reservationsText = infoContainer.transform.Find("ReservationsText").GetComponent<TMP_Text>();

        destroyButton = infoContainer.transform.Find("Buttons").transform.Find("DestroyButton").GetComponent<UnityEngine.UI.Button>();
        destroyButton.onClick.AddListener(() =>
        {
            if (current != null)
                current.DestroyBuilding();
        });

        activateButton = infoContainer.transform.Find("Buttons").transform.Find("ActivateButton").GetComponent<UnityEngine.UI.Button>();
        activateButton.onClick.AddListener(() =>
        {
            if (current != null)
            {
                if (current.CurrentState == BuildingState.Inactive)
                {
                    current.Activate();
                }
                else
                {
                    current.Deactivate();
                }
                UpdateUI();
            }
        });

        Building.OnDestroyed += (b) =>
        {
            if (current == b)
                ClosePanel();
        };

        inputHandler.RegisterClickHandler(HandleMouseClick, 50);

        ClosePanel();
    }

    private bool HandleMouseClick(InputHandler.MouseClick click)
    {
        if (click.Button == InputHandler.MouseButton.Left)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false;

            RaycastHit2D hit = Physics2D.Raycast(inputHandler.MouseWorldPosition, Vector2.zero);
            if (hit.collider != null && hit.collider.transform.parent.TryGetComponent<Building>(out var b))
            {
                OpenPanel(b);
            }
            else
            {
                ClosePanel();
            }
            return true;
        }
        else if (click.Button == InputHandler.MouseButton.Right)
        {
            ClosePanel();
            return true;
        }
        return false;
    }

    private void OpenPanel(Building b)
    {
        log.Info("Panel opened for building " + b.name);
        current = b;
        infoCanvas.enabled = true;
        UpdateUI();
    }

    private void ClosePanel()
    {
        current = null;
        infoCanvas.enabled = false;
    }

    private void UpdateUI()
    {
        if (current == null)
            return;

        nameText.text = current.DisplayName;

        // Activate button text
        if (current.CurrentState == BuildingState.Inactive)
            activateButton.GetComponentInChildren<TMP_Text>().SetText("Activate");
        else
            activateButton.GetComponentInChildren<TMP_Text>().SetText("Deactivate");

        // Status text
        if (current.CurrentState == BuildingState.Operational)
        {
            statusText.SetText("Status: Operational");
        }
        else if (current.CurrentState == BuildingState.Startup)
        {
            statusText.SetText("Status: Starting up");
        }
        else if (current.CurrentState == BuildingState.PendingResources)
        {
            statusText.SetText("Status: Waiting for resources");
        }
        else if (current.CurrentState == BuildingState.Inactive)
        {
            statusText.SetText("Status: Inactive");
        }

        int integrityPercent = Mathf.FloorToInt(100 * current.CurrentHealth / current.MaxHealth);
        if (integrityPercent > 0 && integrityPercent <= 100)
            integrityText.SetText($"Integrity: {integrityPercent}%");

        if (current.ConsumedResources.Count == 0)
            consumedResourcesText.SetText("");
        else
            consumedResourcesText.SetText("Consumes:\n" + string.Join("\n", current.ConsumedResources.Select(r => $"{r.Data.DisplayName} {r.Amount}/s")));

        if (current.ProducedResources.Count == 0)
            producedResourcesText.SetText("");
        else
            producedResourcesText.SetText("Produces:\n" + string.Join("\n", current.ProducedResources.Select(r => $"{r.Data.DisplayName} {r.Amount}/s")));

        if (current.CapacityEffects.Count == 0)
            capacityText.SetText("");
        else
            capacityText.SetText("Storage:\n" + string.Join("\n", current.CapacityEffects.Select(r => $"{r.Data.DisplayName} {r.Amount}")));

        if (current.RequiredReservations.Count == 0)
            reservationsText.SetText("");
        else
            reservationsText.SetText("Operational requirements:\n" + string.Join("\n", current.RequiredReservations.Select(r => $"{r.Data.DisplayName} {r.Amount}")));
    }
}
