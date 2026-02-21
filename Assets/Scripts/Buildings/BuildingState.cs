
public enum BuildingState
{
    Inactive,           // Not built or turned off
    Operational,        // Is producing/consuming
    PendingResources,   // Missing reservations or consumed resources
    Destroyed           // Pending destruction, not interactable
}
