using System;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData Data => buildingData;
    public int Priority => buildingData.ResourcePriority;
    public BuildingState CurrentState => currentState;
    public bool ValidBuildPlacement => placementOverlapCount == 0;

    protected static readonly Logger log = new(true, LogLevel.Info);
    protected ResourceManager resourceManager;
    protected BuildingManager buildingManager;
    protected BuildingData buildingData;
    [SerializeField] protected BuildingState currentState = BuildingState.Inactive;
    protected float currentHealth;
    protected float startupTimer;
    private float placementOverlapCount = 0;

    internal void Initialize(BuildingData data)
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        buildingManager = FindFirstObjectByType<BuildingManager>();
        buildingData = data;
        currentHealth = buildingData.MaxHealth;
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
                if (resourceManager.HasEnoughResources(Data.ConsumedResources, true) &&
                    resourceManager.TryReserveResources(Data.RequiredReservations, this))
                {
                    TransitionTo(BuildingState.Startup);
                }
                break;

            case BuildingState.Operational:
                if (!resourceManager.HasReservation(this) ||
                    !resourceManager.TryConsumeResources(Data.ConsumedResources, true))
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
                resourceManager.ReleaseReservations(Data.RequiredReservations, this);
                resourceManager.RemoveCapacityEffects(Data.CapacityEffects);
                resourceManager.UnregisterResourceUser(this);
                break;

            case BuildingState.Startup:
                log.Info($"Building {buildingData.DisplayName} is starting up.");
                startupTimer = buildingData.StartupTime;
                break;

            case BuildingState.PendingResources:
                log.Info($"Building {buildingData.DisplayName} is pending reservation.");
                break;

            case BuildingState.Operational:
                log.Info($"Building {buildingData.DisplayName} is now operational.");
                resourceManager.RegisterResourceUser(this);
                resourceManager.ApplyCapacityEffects(Data.CapacityEffects);
                break;

            case BuildingState.Destroyed:
                log.Info($"Building {buildingData.DisplayName} destroyed.");
                resourceManager.ReleaseReservations(Data.RequiredReservations, this);
                resourceManager.UnregisterResourceUser(this);
                resourceManager.RemoveCapacityEffects(Data.CapacityEffects);
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
