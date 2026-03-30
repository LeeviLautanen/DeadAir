using UnityEngine;

public class SentinelController : Building
{
    private static readonly new Logger log = new(nameof(SentinelController));

    protected override void Start()
    {
        if (PlacementMode)
        {
            base.Start();
            return;
        }

        base.Start();
    }

    protected override void EnterState(BuildingState state)
    {
        base.EnterState(state);

        switch (state)
        {
            case BuildingState.Operational:
                break;
        }
    }
}
