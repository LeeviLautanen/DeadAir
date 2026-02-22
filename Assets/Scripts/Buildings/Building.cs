using System;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData Data => buildingData;
    public int Priority => buildingData.ResourcePriority;
    public BuildingState CurrentState => currentState;
    public static event Action<Building> OnBuildingDestroyed;
    public static event Action<Building> OnBuildingCreated;

    protected static readonly Logger log = new();
    protected ResourceManager resourceManager;
    protected BuildingData buildingData;
    [SerializeField] protected BuildingState currentState = BuildingState.Inactive;
    protected float currentHealth;
    protected float startupTimer;

    internal void Initialize(BuildingData data)
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        buildingData = data;
        currentHealth = buildingData.MaxHealth;
    }

    private void Start()
    {
        if (buildingData == null)
        {
            log.Error("Building initialized without data!");
            return;
        }
        Activate();
        OnBuildingCreated?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Meteorite meteorite))
        {
            if (meteorite == null)
            {
                Debug.LogError("Shield collided with an object that doesnt have the meteorite script");
                return;
            }
            Damage(meteorite.Damage);
        }
    }

    public void Activate()
    {
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
                log.Log($"Building {buildingData.DisplayName} is now inactive.");
                resourceManager.ReleaseReservations(Data.RequiredReservations, this);
                resourceManager.UnregisterResourceUser(this);
                resourceManager.RemoveCapacityEffects(Data.CapacityEffects);
                break;

            case BuildingState.Startup:
                log.Log($"Building {buildingData.DisplayName} is starting up.");
                startupTimer = buildingData.StartupTime;
                break;

            case BuildingState.PendingResources:
                log.Log($"Building {buildingData.DisplayName} is pending reservation.");
                break;

            case BuildingState.Operational:
                log.Log($"Building {buildingData.DisplayName} is now operational.");
                resourceManager.RegisterResourceUser(this);
                resourceManager.ApplyCapacityEffects(Data.CapacityEffects);
                break;

            case BuildingState.Destroyed:
                log.Log($"Building {buildingData.DisplayName} destroyed.");
                resourceManager.ReleaseReservations(Data.RequiredReservations, this);
                resourceManager.UnregisterResourceUser(this);
                resourceManager.RemoveCapacityEffects(Data.CapacityEffects);
                OnBuildingDestroyed?.Invoke(this);
                Destroy(gameObject);
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
