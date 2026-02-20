
public enum BuildingState
{
    Inactive,           // Not built or player turned off
    PendingReservation, // Player activated, but waiting for resources to be reserved
    Operational,        // Has full reservations and is producing/consuming
    OutOfResources,     // Has reservations, but currently can't consume (transient)
    Destroyed
}
