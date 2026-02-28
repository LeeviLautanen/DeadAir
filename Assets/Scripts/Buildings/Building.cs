using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public int ResourcePriority => resourcePriority;
    public BuildingState CurrentState => currentState;
    public string Id => data.Id;
    public string DisplayName => data.DisplayName;
    public List<ResourceAmount> ProducedResources => producedResources;
    public List<ResourceAmount> ConsumedResources => consumedResources;
    public List<ResourceAmount> CapacityEffects => capacityEffects;
    public List<ResourceAmount> RequiredReservations => requiredReservations;
    public bool ValidBuildPlacement => placementOverlapCount == 0;
    public static Action<Building> OnCreated;
    public static Action<Building> OnDestroyed;

    protected static readonly Logger log = new(true, LogLevel.Info);
    protected ResourceManager resourceManager;
    protected BuildingManager buildingManager;
    protected TechManager techManager;
    [SerializeField] protected BuildingData data;
    [SerializeField] protected BuildingState currentState = BuildingState.Inactive;

    private List<ResourceAmount> producedResources;
    private List<ResourceAmount> consumedResources;
    private List<ResourceAmount> capacityEffects;
    private List<ResourceAmount> requiredReservations;
    private float startupTime;
    private int resourcePriority;
    private float currentHealth;
    [SerializeField] private float maxHealth;
    private float startupTimer;
    private float placementOverlapCount = 0;

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError("BuildingData SO not assigned on " + gameObject.name);
            return;
        }

        resourceManager = FindFirstObjectByType<ResourceManager>();
        buildingManager = FindFirstObjectByType<BuildingManager>();
        techManager = FindFirstObjectByType<TechManager>();
        TechManager.OnResearchCompleted += UpdateStats;

        maxHealth = data.MaxHealth;
        resourcePriority = data.ResourcePriority;
        startupTime = data.StartupTime;
        currentHealth = data.MaxHealth;

        // Make deep copies of the SO lists
        producedResources = data.ProducedResources.ConvertAll(resource => new ResourceAmount(resource.Data, resource.Amount));
        consumedResources = data.ConsumedResources.ConvertAll(resource => new ResourceAmount(resource.Data, resource.Amount));
        capacityEffects = data.CapacityEffects.ConvertAll(resource => new ResourceAmount(resource.Data, resource.Amount));
        requiredReservations = data.RequiredReservations.ConvertAll(resource => new ResourceAmount(resource.Data, resource.Amount));

        OnCreated?.Invoke(this);
        UpdateStats();
    }

    public virtual void ColliderEnter(BuildingColliderType colliderType, Collider2D other)
    {
        switch (colliderType)
        {
            case BuildingColliderType.Damage:
                if (other.gameObject.TryGetComponent(out Meteorite meteorite))
                {
                    if (meteorite == null)
                    {
                        Debug.LogError("Shield collided with an object that doesnt have the meteorite script");
                        return;
                    }
                    if (meteorite.HasCollided) return; // Prevent multiple collisions from the same meteorite
                    meteorite.HasCollided = true;
                    Damage(meteorite.Damage);
                }
                break;

            case BuildingColliderType.Placement:
                if (other.gameObject.layer != LayerMask.NameToLayer("Placement")) break;
                placementOverlapCount++;
                log.Info(placementOverlapCount);
                break;
        }
    }

    public virtual void ColliderExit(BuildingColliderType colliderType, Collider2D other)
    {
        switch (colliderType)
        {
            case BuildingColliderType.Placement:
                if (other.gameObject.layer != LayerMask.NameToLayer("Placement")) break;
                placementOverlapCount = Mathf.Max(placementOverlapCount - 1, 0);
                log.Info(placementOverlapCount);
                break;
        }
    }

    public void Activate()
    {
        resourceManager.RegisterResourceUser(this);
        if (currentState == BuildingState.Inactive)
            TransitionTo(BuildingState.PendingResources);
    }

    public void Deactivate()
    {
        if (currentState != BuildingState.Inactive)
            TransitionTo(BuildingState.Inactive);
    }

    public void DestroyBuilding()
    {
        TransitionTo(BuildingState.Destroyed);
    }

    public virtual void UpdateState(float deltaTime)
    {
        switch (currentState)
        {
            case BuildingState.Inactive:
                // Do nothing until activated
                break;

            case BuildingState.PendingResources:
                if (resourceManager.HasEnoughResources(consumedResources, true) &&
                    resourceManager.TryReserveResources(requiredReservations, this))
                {
                    TransitionTo(BuildingState.Startup);
                }
                break;

            case BuildingState.Startup:
                if (!resourceManager.HasEnoughResources(requiredReservations, false) ||
                    !resourceManager.HasEnoughResources(consumedResources, true))
                {
                    TransitionTo(BuildingState.PendingResources);
                }

                startupTimer -= deltaTime;
                if (startupTimer <= 0)
                {
                    TransitionTo(BuildingState.Operational);
                }
                break;

            case BuildingState.Operational:
                if (!resourceManager.HasReservation(this) ||
                    !resourceManager.TryConsumeResources(consumedResources, true))
                {
                    TransitionTo(BuildingState.PendingResources);
                }
                break;
        }
    }

    protected virtual void EnterState(BuildingState state)
    {
        switch (state)
        {
            case BuildingState.Inactive:
                log.Info($"Building {data.DisplayName} is now inactive.");
                resourceManager.ReleaseReservations(requiredReservations, this);
                resourceManager.RemoveCapacityEffects(capacityEffects);
                resourceManager.UnregisterResourceUser(this);
                break;

            case BuildingState.PendingResources:
                log.Info($"Building {data.DisplayName} is pending reservation.");
                break;

            case BuildingState.Startup:
                log.Info($"Building {data.DisplayName} is starting up.");
                startupTimer = startupTime;
                break;

            case BuildingState.Operational:
                log.Info($"Building {data.DisplayName} is now operational.");
                resourceManager.ApplyCapacityEffects(capacityEffects);
                break;

            case BuildingState.Destroyed:
                log.Info($"Building {data.DisplayName} destroyed.");
                resourceManager.ReleaseReservations(requiredReservations, this);
                resourceManager.UnregisterResourceUser(this);
                resourceManager.RemoveCapacityEffects(capacityEffects);
                OnDestroyed?.Invoke(this);
                break;
        }
    }

    protected void UpdateStats()
    {
        bool hasResources = currentState == BuildingState.Operational || currentState == BuildingState.Startup;

        // Max health
        maxHealth = techManager.GetModifiedValue(data.MaxHealth, ModifierType.MaxHealth, data.Id);
        currentHealth = maxHealth;

        // Production
        for (int i = 0; i < data.ProducedResources.Count; i++)
        {
            float newAmount = techManager.GetModifiedValue(data.ProducedResources[i].Amount, ModifierType.ProductionRate, data.Id);
            producedResources[i].Amount = newAmount;
        }

        // Consumption
        for (int i = 0; i < data.ConsumedResources.Count; i++)
        {
            float newAmount = techManager.GetModifiedValue(data.ConsumedResources[i].Amount, ModifierType.ConsumptionRate, data.Id);
            consumedResources[i].Amount = newAmount;
        }

        // Capacity
        if (hasResources) resourceManager.RemoveCapacityEffects(capacityEffects);
        for (int i = 0; i < data.CapacityEffects.Count; i++)
        {
            float newAmount = techManager.GetModifiedValue(data.CapacityEffects[i].Amount, ModifierType.Capacity, data.Id);
            capacityEffects[i].Amount = newAmount;
        }
        if (hasResources) resourceManager.ApplyCapacityEffects(capacityEffects);

        // Reservations
        if (hasResources) resourceManager.ReleaseReservations(requiredReservations, this);
        for (int i = 0; i < data.RequiredReservations.Count; i++)
        {
            float newAmount = techManager.GetModifiedValue(data.RequiredReservations[i].Amount, ModifierType.Reservation, data.Id);
            requiredReservations[i].Amount = newAmount;
        }
        if (hasResources) resourceManager.TryReserveResources(requiredReservations, this);
    }

    private void TransitionTo(BuildingState newState)
    {
        if (newState == currentState || currentState == BuildingState.Destroyed) return;

        currentState = newState;
        EnterState(newState);
    }

    private void Damage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }
}
