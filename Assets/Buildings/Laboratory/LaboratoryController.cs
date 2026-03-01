public class LaboratoryController : Building
{
    private LaboratoryManager laboratoryManager;

    protected override void Start()
    {
        laboratoryManager = FindFirstObjectByType<LaboratoryManager>();

        base.Start();
    }

    protected override void EnterState(BuildingState state)
    {
        base.EnterState(state);

        switch (state)
        {
            case BuildingState.Inactive:
                laboratoryManager.SetLaboratoryState(this, false);
                break;

            case BuildingState.PendingResources:
                laboratoryManager.SetLaboratoryState(this, false);
                break;

            case BuildingState.Operational:
                laboratoryManager.SetLaboratoryState(this, true);
                break;

            case BuildingState.Destroyed:
                laboratoryManager.SetLaboratoryState(this, false);
                break;
        }
    }
}
