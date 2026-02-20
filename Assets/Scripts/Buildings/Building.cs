using System;
using UnityEngine;

public class Building : MonoBehaviour, IResourceUser
{
    public BuildingData Data => buildingData;
    public int Priority => buildingData.ResourcePriority;
    public BuildingState CurrentState => currentState;
    public static event Action<Building> OnBuildingDestroyed;

    // IResourceUser callbacks
    public void OnReservationsAcquired() => TransitionTo(BuildingState.Operational);
    public void OnReservationsLost() => TransitionTo(BuildingState.PendingReservation);
    public void OnOutOfResources() => TransitionTo(BuildingState.OutOfResources);
    public void OnResourcesRecovered() => TransitionTo(BuildingState.Operational);

    private static readonly Logger log = new();
    private ResourceManager resourceManager;
    private BuildingData buildingData;
    [SerializeField] private BuildingState currentState = BuildingState.Inactive;
    private float currentHealth;

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
            TransitionTo(BuildingState.PendingReservation);
    }

    public void Deactivate()
    {
        if (currentState != BuildingState.Inactive && currentState != BuildingState.Destroyed)
            TransitionTo(BuildingState.Inactive);
    }

    public virtual void DestroyBuilding()
    {
        Deactivate();
        currentState = BuildingState.Destroyed;
        OnBuildingDestroyed?.Invoke(this);
    }

    protected virtual void EnterState(BuildingState state)
    {
        switch (state)
        {
            case BuildingState.PendingReservation:
                log.Log($"Building {buildingData.DisplayName} is pending reservation.");
                if (resourceManager.TryReserveResources(Data.RequiredReservations))
                    TransitionTo(BuildingState.Operational);
                break;

            case BuildingState.Operational:
                log.Log($"Building {buildingData.DisplayName} is now operational.");
                resourceManager.RegisterResourceUser(this);
                resourceManager.ApplyCapacityEffects(Data.CapacityEffects);
                break;

            case BuildingState.OutOfResources:
                log.Log($"Building {buildingData.DisplayName} is out of resources.");
                break;

            case BuildingState.Inactive:
                log.Log($"Building {buildingData.DisplayName} is now inactive.");
                resourceManager.ReleaseReservations(Data.RequiredReservations);
                resourceManager.UnregisterResourceUser(this);
                resourceManager.RemoveCapacityEffects(Data.CapacityEffects);
                break;
        }
    }

    private void TransitionTo(BuildingState newState)
    {
        if (newState == currentState) return;

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
