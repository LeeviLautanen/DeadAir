using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSelector : MonoBehaviour
{
    private static readonly Logger log = new(nameof(BuildingSelector));
    private InputHandler inputHandler;
    private TechManager techManager;
    private Building current;
    private Canvas infoCanvas;
    private GameObject infoContainer;
    private TMP_Text nameText;
    private TMP_Text statusText;
    private TMP_Text integrityText;
    private TMP_Text startupTimeText;
    private TMP_Text consumedResourcesText;
    private TMP_Text producedResourcesText;
    private TMP_Text capacityText;
    private TMP_Text reservationsText;
    private UnityEngine.UI.Button destroyButton;
    private UnityEngine.UI.Button activateButton;
    private bool isPanelOpen;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();
        techManager = FindFirstObjectByType<TechManager>();

        infoCanvas = gameObject.GetComponent<Canvas>();
        infoContainer = gameObject.transform.Find("InfoContainer").gameObject;

        nameText = infoContainer.transform.Find("NameText").GetComponent<TMP_Text>();
        statusText = infoContainer.transform.Find("StatusText").GetComponent<TMP_Text>();
        integrityText = infoContainer.transform.Find("IntegrityText").GetComponent<TMP_Text>();
        startupTimeText = infoContainer.transform.Find("StartupTimeText").GetComponent<TMP_Text>();
        consumedResourcesText = infoContainer.transform.Find("ConsumedResourcesText").GetComponent<TMP_Text>();
        producedResourcesText = infoContainer.transform.Find("ProducedResourcesText").GetComponent<TMP_Text>();
        capacityText = infoContainer.transform.Find("CapacityEffectsText").GetComponent<TMP_Text>();
        reservationsText = infoContainer.transform.Find("ReservationsText").GetComponent<TMP_Text>();

        destroyButton = infoContainer.transform.Find("Buttons").transform.Find("DestroyButton").GetComponent<UnityEngine.UI.Button>();
        destroyButton.onClick.AddListener(() =>
        {
            if (current != null)
            {
                current.DestroyBuilding();
            }
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

        inputHandler.RegisterClickHandler(HandleMouseClick, 0);

        ClosePanel();
    }

    private bool HandleMouseClick(InputHandler.MouseClick click)
    {
        if (click.Button == InputHandler.MouseButton.Left)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false;

            RaycastHit2D[] hits = Physics2D.RaycastAll(inputHandler.MouseWorldPosition, Vector2.zero);
            foreach (var hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    return true;
                }

                if (hit.collider.transform.TryGetComponent<BuildingCollider>(out var buildingCollider)
                    && buildingCollider.Type == BuildingColliderType.Damage)
                {
                    hit.collider.transform.parent.TryGetComponent<Building>(out var building);
                    OpenPanel(building);
                    return true;
                }
            }
        }
        else if (click.Button == InputHandler.MouseButton.Right && isPanelOpen)
        {
            ClosePanel();
            return true;
        }
        return false;
    }

    private void OpenPanel(Building b)
    {
        if (isPanelOpen)
        {
            ClosePanel();
        }

        log.Info("Panel opened for building " + b.name);
        isPanelOpen = true;
        current = b;
        infoCanvas.enabled = true;
        UpdateUI();
    }

    private void ClosePanel()
    {
        isPanelOpen = false;
        current = null;
        infoCanvas.enabled = false;
    }

    private void UpdateUI()
    {
        if (current == null)
            return;

        nameText.text = current.Data.DisplayName;

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

        GameObject startupTextGO = startupTimeText.gameObject;
        bool isStartupTextActive = startupTextGO.activeSelf;
        if (current.Data.StartupTime > 0)
        {
            if (!isStartupTextActive)
            {
                startupTextGO.SetActive(true);
            }

            if (Mathf.Approximately(current.Data.StartupTime, 1))
            {
                startupTimeText.SetText($"Startup time: 1 hr");
            }
            else
            {
                startupTimeText.SetText($"Startup time: {current.Data.StartupTime} hrs");
            }
        }
        else if (isStartupTextActive)
        {
            startupTextGO.SetActive(false);
        }

        UpdateInfoText(consumedResourcesText, "Consumes", current.Data.ConsumedResources,
                r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.ConsumptionRate, current.Data.Id)}/s");

        UpdateInfoText(producedResourcesText, "Produces", current.Data.ProducedResources,
                r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.ProductionRate, current.Data.Id)}/s");

        UpdateInfoText(capacityText, "Storage", current.Data.CapacityEffects,
                r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.Capacity, current.Data.Id)}");

        UpdateInfoText(reservationsText, "Operational requirements", current.Data.RequiredReservations,
                r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.Reservation, current.Data.Id)}");
    }

    private void UpdateInfoText<T>(TMP_Text text, string header, IEnumerable<T> items, Func<T, string> formatter)
    {
        if (!items.Any())
        {
            text.gameObject.SetActive(false);
            return;
        }

        if (text.gameObject.activeSelf == false)
        {
            text.gameObject.SetActive(true);
        }

        text.SetText(header + ":\n" + string.Join("\n", items.Select(formatter)));
    }
}
