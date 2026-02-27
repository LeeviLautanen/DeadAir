using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public int ResourcePriority => resourcePriority;
    public BuildingState CurrentState => currentState;
    public string Id => buildingData.Id;
    public string DisplayName => buildingData.DisplayName;
    public List<ResourceAmount> ProducedResources => producedResources;
    public List<ResourceAmount> ConsumedResources => consumedResources;
    public List<ResourceAmount> CapacityEffects => capacityEffects;
    public List<ResourceAmount> RequiredReservations => requiredReservations;
    public bool ValidBuildPlacement => placementOverlapCount == 0;

    protected static readonly Logger log = new(true, LogLevel.Info);
    protected ResourceManager resourceManager;
    protected BuildingManager buildingManager;
    protected BuildingData buildingData;
    [SerializeField] protected BuildingState currentState = BuildingState.Inactive;

    private List<ResourceAmount> producedResources;
    private List<ResourceAmount> consumedResources;
    private List<ResourceAmount> capacityEffects;
    private List<ResourceAmount> requiredReservations;
    private float startupTime;
    private int resourcePriority;
    private float currentHealth;
    private float maxHealth;
    private float startupTimer;
    private float placementOverlapCount = 0;

    internal void Initialize(BuildingData data)
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        buildingManager = FindFirstObjectByType<BuildingManager>();

        buildingData = data;
        maxHealth = buildingData.MaxHealth;
        resourcePriority = buildingData.ResourcePriority;
        startupTime = buildingData.StartupTime;
        currentHealth = buildingData.MaxHealth;

        producedResources = buildingData.ProducedResources;
        consumedResources = buildingData.ConsumedResources;
        capacityEffects = buildingData.CapacityEffects;
        requiredReservations = buildingData.RequiredReservations;
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
        if (currentState != BuildingState.Inactive && currentState != BuildingState.Destroyed)
            TransitionTo(BuildingState.Inactive);
    }

    public void TransitionTo(BuildingState newState)
    {
        if (newState == currentState) return;

        currentState = newState;
        EnterState(newState);
    }

    public virtual void DestroyBuilding()
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

            case BuildingState.Startup:
                startupTimer -= deltaTime;
                if (startupTimer <= 0)
                {
                    TransitionTo(BuildingState.Operational);
                }
                break;

            case BuildingState.PendingResources:
                if (resourceManager.HasEnoughResources(consumedResources, true) &&
                    resourceManager.TryReserveResources(requiredReservations, this))
                {
                    TransitionTo(BuildingState.Startup);
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
                log.Info($"Building {buildingData.DisplayName} is now inactive.");
                resourceManager.ReleaseReservations(requiredReservations, this);
                resourceManager.RemoveCapacityEffects(capacityEffects);
                resourceManager.UnregisterResourceUser(this);
                break;

            case BuildingState.Startup:
                log.Info($"Building {buildingData.DisplayName} is starting up.");
                startupTimer = startupTime;
                break;

            case BuildingState.PendingResources:
                log.Info($"Building {buildingData.DisplayName} is pending reservation.");
                break;

            case BuildingState.Operational:
                log.Info($"Building {buildingData.DisplayName} is now operational.");
                resourceManager.RegisterResourceUser(this);
                resourceManager.ApplyCapacityEffects(capacityEffects);
                break;

            case BuildingState.Destroyed:
                log.Info($"Building {buildingData.DisplayName} destroyed.");
                resourceManager.ReleaseReservations(requiredReservations, this);
                resourceManager.UnregisterResourceUser(this);
                resourceManager.RemoveCapacityEffects(capacityEffects);
                buildingManager.DestroyBuilding(this);
                break;
        }
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
