using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public int ResourcePriority => resourcePriority;
    public BuildingState CurrentState => currentState;
    public BuildingData Data => data;
    public string Id => data.Id;
    public string DisplayName => data.DisplayName;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public List<ResourceAmount> ProducedResources => producedResources;
    public List<ResourceAmount> ConsumedResources => consumedResources;
    public List<ResourceAmount> CapacityEffects => capacityEffects;
    public List<ResourceAmount> RequiredReservations => requiredReservations;
    public bool PlacementMode = false;
    public static Action<Building> OnCreated;
    public static Action<Building> OnDestroyed;

    protected static readonly Logger log = new(nameof(Building));
    protected ResourceManager resourceManager;
    protected BuildingManager buildingManager;
    protected TechManager techManager;
    protected TimeManager timeManager;
    [SerializeField] protected BuildingData data;
    [SerializeField] protected BuildingState currentState = BuildingState.Inactive;

    private List<ResourceAmount> producedResources = new();
    private List<ResourceAmount> consumedResources = new();
    private List<ResourceAmount> capacityEffects = new();
    private List<ResourceAmount> requiredReservations = new();
    private float startupTime;
    private int resourcePriority;
    private float currentHealth;
    [SerializeField] private float maxHealth;
    private float startupTimer;
    private float placementOverlapCount = 0;
    private Material buildingMat;
    private Shader activeShader;
    private Shader inactiveShader;

    protected virtual void Awake()
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        buildingManager = FindFirstObjectByType<BuildingManager>();
        techManager = FindFirstObjectByType<TechManager>();
        timeManager = TimeManager.Instance;
    }

    protected virtual void Start()
    {
        if (data == null)
        {
            Debug.LogError("BuildingData SO not assigned on " + gameObject.name);
            return;
        }

        if (PlacementMode) return;

        buildingMat = GetComponentInChildren<SpriteRenderer>().material;
        activeShader = buildingMat.shader;
        inactiveShader = Shader.Find("Custom/BuildingInactiveShader");
        buildingMat.shader = inactiveShader;

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

        log.Info($"Creating building {data.DisplayName}");
        OnCreated?.Invoke(this);
        UpdateStats();
    }

    public bool IsValidPlacement()
    {
        bool hasSpace = placementOverlapCount == 0;
        bool hasResources = resourceManager.HasEnoughResources(requiredReservations, false) &&
                            resourceManager.HasEnoughResources(data.ConstructionCost, false);

        return hasSpace && hasResources;
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
                        Debug.LogError("Building collided with an object that doesnt have the meteorite script");
                        return;
                    }
                    if (meteorite.HasCollided) return; // Prevent multiple collisions from the same meteorite
                    meteorite.HasCollided = true;
                    Damage(meteorite.Damage);
                }
                break;

            case BuildingColliderType.Placement:
                other.TryGetComponent<BuildingCollider>(out var buildingCollider);
                if (buildingCollider == null || buildingCollider.Type != BuildingColliderType.Placement)
                {
                    return;
                }

                placementOverlapCount++;
                log.Info($"Placement overlap count: {placementOverlapCount}");
                break;
        }
    }

    public virtual void ColliderExit(BuildingColliderType colliderType, Collider2D other)
    {
        switch (colliderType)
        {
            case BuildingColliderType.Placement:
                other.TryGetComponent<BuildingCollider>(out var buildingCollider);
                if (buildingCollider == null || buildingCollider.Type != BuildingColliderType.Placement)
                {
                    return;
                }

                placementOverlapCount = Mathf.Max(placementOverlapCount - 1, 0);
                log.Info($"Placement overlap count: {placementOverlapCount}");
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
                {
                    // Do nothing until activated
                    break;
                }

            case BuildingState.PendingResources:
                {
                    bool canConsume = resourceManager.HasEnoughResources(consumedResources, true);
                    bool hasReservation = resourceManager.HasReservation(this);
                    bool canReserve = false;
                    if (!hasReservation)
                    {
                        canReserve = resourceManager.HasEnoughResources(requiredReservations, false);
                    }

                    if (canConsume && (hasReservation || canReserve))
                    {
                        TransitionTo(BuildingState.Startup);
                        break;
                    }
                    break;
                }

            case BuildingState.Startup:
                {
                    bool canConsume = resourceManager.HasEnoughResources(consumedResources, true);
                    bool hasReservation = resourceManager.HasReservation(this);
                    bool canReserve = false;
                    if (!hasReservation)
                    {
                        canReserve = resourceManager.HasEnoughResources(requiredReservations, false);
                    }

                    if (!canConsume && (!hasReservation || !canReserve))
                    {
                        TransitionTo(BuildingState.PendingResources);
                        break;
                    }

                    startupTimer -= deltaTime;
                    if (startupTimer <= 0)
                    {
                        TransitionTo(BuildingState.Operational);
                        break;
                    }
                    break;
                }

            case BuildingState.Operational:
                {
                    // Try to reserve resources
                    bool hasReservation = resourceManager.HasReservation(this);
                    if (!hasReservation)
                    {
                        bool newReservation = resourceManager.TryReserveResources(requiredReservations, this);
                        if (!newReservation)
                        {
                            TransitionTo(BuildingState.PendingResources);
                            break;
                        }
                    }

                    // Try to consume resources
                    bool consumptionSuccessful = resourceManager.TryConsumeResources(consumedResources, true);
                    if (!consumptionSuccessful)
                    {
                        TransitionTo(BuildingState.PendingResources);
                        break;
                    }

                    // Regen 0.5 health per second
                    if (currentHealth < maxHealth)
                    {
                        currentHealth = Mathf.Min(currentHealth + deltaTime * 0.5f, maxHealth);
                    }
                    break;
                }
        }
    }

    protected virtual void EnterState(BuildingState state)
    {
        switch (state)
        {
            case BuildingState.Inactive:
                resourceManager.ReleaseReservations(requiredReservations, this);
                resourceManager.RemoveCapacityEffects(capacityEffects, this);
                resourceManager.UnregisterResourceUser(this);
                buildingMat.shader = inactiveShader;
                log.Info($"Building {data.DisplayName} is now inactive.");
                break;

            case BuildingState.PendingResources:
                buildingMat.shader = inactiveShader;
                log.Info($"Building {data.DisplayName} is pending reservation.");
                break;

            case BuildingState.Startup:
                startupTimer = startupTime;
                log.Info($"Building {data.DisplayName} is starting up.");
                break;

            case BuildingState.Operational:
                buildingMat.shader = activeShader;
                resourceManager.ApplyCapacityEffects(capacityEffects, this);
                log.Info($"Building {data.DisplayName} is now operational.");
                break;

            case BuildingState.Destroyed:
                if (resourceManager.HasReservation(this))
                {
                    resourceManager.ReleaseReservations(requiredReservations, this);
                }
                resourceManager.UnregisterResourceUser(this);
                resourceManager.RemoveCapacityEffects(capacityEffects, this);
                OnDestroyed?.Invoke(this);
                log.Info($"Building {data.DisplayName} destroyed.");
                Destroy(gameObject);
                break;
        }
    }

    protected virtual void UpdateStats()
    {
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
        for (int i = 0; i < data.CapacityEffects.Count; i++)
        {
            float newAmount = techManager.GetModifiedValue(data.CapacityEffects[i].Amount, ModifierType.Capacity, data.Id);
            float delta = newAmount - capacityEffects[i].Amount;
            capacityEffects[i].Amount = newAmount;

            if (resourceManager.HasCapacityEffects(this))
                resourceManager.ChangeCapacity(data.CapacityEffects[i].Data.Id, delta);
        }

        // Reservations
        for (int i = 0; i < data.RequiredReservations.Count; i++)
        {
            float newAmount = techManager.GetModifiedValue(data.RequiredReservations[i].Amount, ModifierType.Reservation, data.Id);
            float delta = newAmount - requiredReservations[i].Amount;
            requiredReservations[i].Amount = newAmount;

            if (resourceManager.HasReservation(this))
                resourceManager.ChangeReservation(data.RequiredReservations[i].Data.Id, delta);
        }
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

        log.Info($"{data.DisplayName} took {damage} damage, current health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }
}
