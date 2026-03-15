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

        inputHandler.TimeSpeedStepRequested += HandleTimeSpeedStep;
        inputHandler.ResearchMenuToggleRequested += HandleResearchToggle;
        inputHandler.PauseToggleRequested += HandlePauseToggle;
        inputHandler.PauseMenuRequested += HandlePauseMenuRequested;
    }

    private void OnDestroy()
    {
        if (inputHandler == null)
            return;

        inputHandler.TimeSpeedStepRequested -= HandleTimeSpeedStep;
        inputHandler.ResearchMenuToggleRequested -= HandleResearchToggle;
        inputHandler.PauseToggleRequested -= HandlePauseToggle;
        inputHandler.PauseMenuRequested -= HandlePauseMenuRequested;
    }

    private void HandleTimeSpeedStep(float step)
    {
        if (timeManager == null)
            return;

        timeManager.StepGameTimeMultiplier(step);
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
