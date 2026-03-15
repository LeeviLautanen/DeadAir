using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildMenuInfo : MonoBehaviour
{
    private static readonly Logger log = new(nameof(BuildMenuInfo));
    private InputHandler inputHandler;
    private TechManager techManager;
    private TimeManager timeManager;
    private BuildingData current;
    private Canvas infoCanvas;
    private GameObject infoContainer;
    private TMP_Text nameText;
    private TMP_Text descriptionText;
    private TMP_Text constructionCostsText;
    private TMP_Text consumedResourcesText;
    private TMP_Text producedResourcesText;
    private TMP_Text capacityText;
    private TMP_Text reservationsText;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();
        techManager = FindFirstObjectByType<TechManager>();
        timeManager = TimeManager.Instance;

        infoCanvas = gameObject.GetComponent<Canvas>();
        infoContainer = gameObject.transform.Find("BuildMenuInfoContainer").gameObject;

        nameText = infoContainer.transform.Find("NameText").GetComponent<TMP_Text>();
        descriptionText = infoContainer.transform.Find("DescriptionText").GetComponent<TMP_Text>();
        constructionCostsText = infoContainer.transform.Find("ConstructionCostsText").GetComponent<TMP_Text>();
        consumedResourcesText = infoContainer.transform.Find("ConsumedResourcesText").GetComponent<TMP_Text>();
        producedResourcesText = infoContainer.transform.Find("ProducedResourcesText").GetComponent<TMP_Text>();
        capacityText = infoContainer.transform.Find("CapacityEffectsText").GetComponent<TMP_Text>();
        reservationsText = infoContainer.transform.Find("ReservationsText").GetComponent<TMP_Text>();

        ClosePanel();
    }

    public void OpenPanel(BuildingData b)
    {
        if (current == b)
            return;

        log.Info("Panel opened for building " + b.DisplayName);
        current = b;
        infoCanvas.enabled = true;
        UpdateUI();
    }

    public void ClosePanel()
    {
        current = null;
        infoCanvas.enabled = false;
    }

    private void UpdateUI()
    {
        if (current == null)
            return;

        float secondsPerGameHour = timeManager.DayLengthSeconds / 24f;

        nameText.text = current.DisplayName;

        descriptionText.text = current.Description;

        UpdateInfoText(constructionCostsText, "Construction costs", current.ConstructionCost,
            r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.ConstructionCost, current.Id)}");

        UpdateInfoText(consumedResourcesText, "Consumes", current.ConsumedResources,
            r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.ConsumptionRate, current.Id) * secondsPerGameHour}/hr");

        UpdateInfoText(producedResourcesText, "Produces", current.ProducedResources,
            r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.ProductionRate, current.Id) * secondsPerGameHour}/hr");

        UpdateInfoText(capacityText, "Storage", current.CapacityEffects,
            r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.Capacity, current.Id)}");

        UpdateInfoText(reservationsText, "Operational requirements", current.RequiredReservations,
            r => $"{r.Data.DisplayName} {techManager.GetModifiedValue(r.Amount, ModifierType.Reservation, current.Id)}");
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
