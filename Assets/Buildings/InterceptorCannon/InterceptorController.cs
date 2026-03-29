public class InterceptorController : Building
{
    private MeteoriteWaveManager meteoriteWaveManager;

    protected override void Awake()
    {
        base.Awake();
        meteoriteWaveManager = FindFirstObjectByType<MeteoriteWaveManager>();
    }

    protected override void EnterState(BuildingState state)
    {
        switch (state)
        {
            case BuildingState.Inactive:
                meteoriteWaveManager.SetInterceptorState(this, false);
                break;

            case BuildingState.PendingResources:
                meteoriteWaveManager.SetInterceptorState(this, false);
                break;

            case BuildingState.Operational:
                meteoriteWaveManager.SetInterceptorState(this, true);
                break;

            case BuildingState.Destroyed:
                meteoriteWaveManager.SetInterceptorState(this, false);
                break;
        }

        base.EnterState(state);
    }
}
