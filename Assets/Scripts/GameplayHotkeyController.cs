using UnityEngine;

public class GameplayHotkeyController : MonoBehaviour
{
    private static readonly Logger log = new(nameof(GameplayHotkeyController));
    private InputHandler inputHandler;
    private TimeManager timeManager;
    private TechManager techManager;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();
        timeManager = FindFirstObjectByType<TimeManager>();
        techManager = FindFirstObjectByType<TechManager>();

        if (inputHandler == null)
        {
            Debug.LogError("InputHandler not found. Gameplay hotkeys are disabled.");
            enabled = false;
            return;
        }

        inputHandler.GameTimeIncreaseRequested += HandleGameTimeIncrease;
        inputHandler.GameTimeDecreaseRequested += HandleGameTimeDecrease;
        inputHandler.ResearchMenuToggleRequested += HandleResearchToggle;
        inputHandler.PauseToggleRequested += HandlePauseToggle;
        inputHandler.PauseMenuRequested += HandlePauseMenuRequested;
    }

    private void OnDestroy()
    {
        if (inputHandler == null)
            return;

        inputHandler.GameTimeIncreaseRequested -= HandleGameTimeIncrease;
        inputHandler.GameTimeDecreaseRequested -= HandleGameTimeDecrease;
        inputHandler.ResearchMenuToggleRequested -= HandleResearchToggle;
        inputHandler.PauseToggleRequested -= HandlePauseToggle;
        inputHandler.PauseMenuRequested -= HandlePauseMenuRequested;
    }

    private void HandleGameTimeIncrease()
    {
        if (timeManager == null)
            return;

        switch (timeManager.GameTimeMultiplier)
        {
            case <= 0.25f:
                timeManager.StepGameTimeMultiplier(0.25f);
                break;
            case <= 0.5f:
                timeManager.StepGameTimeMultiplier(0.5f);
                break;
            case >= 1f:
                timeManager.StepGameTimeMultiplier(1f);
                break;
            default:
                break;
        }
    }

    private void HandleGameTimeDecrease()
    {
        if (timeManager == null)
            return;

        switch (timeManager.GameTimeMultiplier)
        {
            case <= 0.25f:
                break;
            case <= 0.5f:
                timeManager.StepGameTimeMultiplier(-0.25f);
                break;
            case <= 1f:
                timeManager.StepGameTimeMultiplier(-0.5f);
                break;
            case >= 2f:
                timeManager.StepGameTimeMultiplier(-1f);
                break;
            default:
                break;
        }
    }

    private void HandleResearchToggle()
    {
        if (techManager == null)
            return;

        techManager.SetVisible(!techManager.IsVisible);
    }

    private void HandlePauseToggle()
    {
        if (timeManager == null)
            return;

        timeManager.TogglePause();
    }

    private void HandlePauseMenuRequested()
    {
        log.Info("Pause menu requested (ESC). Hook this to the pause menu UI.");
    }
}
