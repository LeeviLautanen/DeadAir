using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameEndType
{
    None,
    Lose,
    Win
}

public class GameManager : MonoBehaviour
{
    [Header("Building IDs")]
    [SerializeField] private string bunkerBuildingId = "bunker";
    [SerializeField] private string sentinelBuildingId = "sentinel";

    [Header("Cinematic")]
    [SerializeField] private float cameraMoveDuration = 1.2f;
    [SerializeField] private float focusZoom = 8f;
    [SerializeField] private float fadeDuration = 1.8f;
    [SerializeField] private float cinematicGameSpeed = 0.2f;

    [Header("References")]
    private CameraController cameraController;
    private InputHandler inputHandler;
    private TimeManager timeManager;
    private BuildingManager buildingManager;
    private MeteoriteWaveManager waveManager;
    [SerializeField] private CanvasGroup loseFadeGroup;
    [SerializeField] private CanvasGroup winFadeGroup;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button playAgainButton;
    [SerializeField] Canvas difficultySelectionCanvas;
    [SerializeField] Canvas helpMenuCanvas;
    [SerializeField] Button easyButton;
    [SerializeField] Button normalButton;
    [SerializeField] Button hardButton;
    [SerializeField] Button pauseMenuRestartButton;
    [SerializeField] Button pauseMenuResumeButton;
    [SerializeField] Button pauseMenuExitButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Slider volumeSlider;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] AudioMixer audioMixer;

    public GameEndType EndType { get; private set; } = GameEndType.None;
    public bool IsGameEnding => isGameEnding;
    private bool isGameEnding;

    private void Awake()
    {
        cameraController = FindFirstObjectByType<CameraController>();
        inputHandler = FindFirstObjectByType<InputHandler>();
        timeManager = TimeManager.Instance;
        waveManager = FindFirstObjectByType<MeteoriteWaveManager>();
        buildingManager = FindFirstObjectByType<BuildingManager>();

        loseFadeGroup.alpha = 0f;
        loseFadeGroup.blocksRaycasts = false;
        loseFadeGroup.GetComponent<Canvas>().enabled = true;
        winFadeGroup.alpha = 0f;
        winFadeGroup.blocksRaycasts = false;
        winFadeGroup.GetComponent<Canvas>().enabled = true;
    }

    private void Start()
    {
        tryAgainButton.onClick.AddListener(() => { RestartGame(); });
        playAgainButton.onClick.AddListener(() => { RestartGame(); });

        settingsButton.onClick.AddListener(() => { inputHandler.gameObject.GetComponent<GameplayHotkeyController>().HandlePauseMenuToggle(); });
        pauseMenuRestartButton.onClick.AddListener(() => { RestartGame(); });
        pauseMenuResumeButton.onClick.AddListener(() => { inputHandler.gameObject.GetComponent<GameplayHotkeyController>().HandlePauseMenuToggle(); });
        pauseMenuExitButton.onClick.AddListener(() => { Application.Quit(); });

        easyButton.onClick.AddListener(() => SelectDifficulty(0.8f));
        normalButton.onClick.AddListener(() => SelectDifficulty(1f));
        hardButton.onClick.AddListener(() => SelectDifficulty(1.2f));

        volumeSlider.onValueChanged.AddListener((value) => { SetAudioLevel(value); });
        volumeSlider.value = 0.5f;
        SetAudioLevel(volumeSlider.value);

        PauseGame();
        helpMenuCanvas.enabled = false;
        difficultySelectionCanvas.enabled = true;
    }

    private void OnEnable()
    {
        Building.OnLethalDamage += HandleLethalBuildingDamage;
        Building.OnOperational += HandleBuildingOperational;
    }

    private void OnDisable()
    {
        Building.OnLethalDamage -= HandleLethalBuildingDamage;
        Building.OnOperational -= HandleBuildingOperational;
    }

    private void SetAudioLevel(float sliderValue)
    {
        float decibels = Mathf.Max(0.0001f, sliderValue) * 40f - 20f;
        audioMixer.SetFloat("MasterVolume", decibels);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SelectDifficulty(float modifier)
    {
        waveManager.DifficultyModifier = modifier;
        waveManager.UpdateWaveinfo();
        difficultySelectionCanvas.enabled = false;
        helpMenuCanvas.enabled = true;
    }

    private bool HandleLethalBuildingDamage(Building building)
    {
        if (building == null || isGameEnding)
            return false;

        if (!string.Equals(building.Id, bunkerBuildingId, StringComparison.OrdinalIgnoreCase))
            return false;

        StartEndSequence(GameEndType.Lose, building.transform);
        return true;
    }

    private void HandleBuildingOperational(Building building)
    {
        if (building == null || isGameEnding)
            return;

        if (!string.Equals(building.Id, sentinelBuildingId, StringComparison.OrdinalIgnoreCase))
            return;

        StartEndSequence(GameEndType.Win, building.transform);
    }

    private void StartEndSequence(GameEndType endType, Transform focusTarget)
    {
        if (isGameEnding)
            return;

        isGameEnding = true;
        EndType = endType;
        StartCoroutine(PlayEndSequence(focusTarget));
    }

    private IEnumerator PlayEndSequence(Transform focusTarget)
    {
        LockInput();
        PauseGame();

        yield return MoveCameraToTarget(focusTarget);

        // Calculate some bs score
        if (EndType == GameEndType.Win)
        {
            int totalScore = 0;
            Dictionary<string, int> buildingScores = new()
            {
                { "apartment", 15 },
                { "energy_storage", 30 },
                { "interceptor_cannon", 100 },
                { "material_storage", 30 },
                { "power_plant", 50 },
                { "shield", 10 },
                { "refinery", 60 },
                { "laboratory", 80 }
            };

            foreach (var entry in buildingScores)
            {
                totalScore += buildingManager.GetBuildingCount(entry.Key) * entry.Value;
            }

            scoreText.SetText($"Your score: {totalScore}");
        }

        SetGameSpeed(cinematicGameSpeed);
        loseFadeGroup.blocksRaycasts = true;
        winFadeGroup.blocksRaycasts = true;
        yield return FadeToBlack();

        PauseGame();
    }

    private IEnumerator MoveCameraToTarget(Transform focusTarget)
    {
        Camera cam = Camera.main;
        if (cam == null || focusTarget == null)
            yield break;

        Transform cameraTransform = cameraController != null ? cameraController.transform : cam.transform;

        Vector3 startPosition = cameraTransform.position;
        Vector3 targetPosition = new(focusTarget.position.x, focusTarget.position.y, startPosition.z);
        float startZoom = cam.orthographicSize;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, cameraMoveDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(startPosition, targetPosition, eased);
            cam.orthographicSize = Mathf.Lerp(startZoom, focusZoom, eased);

            yield return null;
        }

        cameraTransform.position = targetPosition;
        cam.orthographicSize = focusZoom;
    }

    private IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);
        CanvasGroup targetGroup;

        switch (EndType)
        {
            case GameEndType.Win:
                targetGroup = winFadeGroup;
                break;
            case GameEndType.Lose:
                targetGroup = loseFadeGroup;
                break;
            default:
                yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            SetGroupAlpha(targetGroup, alpha);
            yield return null;
        }

        SetGroupAlpha(targetGroup, 1f);
    }

    private void LockInput()
    {
        if (inputHandler == null)
            inputHandler = FindFirstObjectByType<InputHandler>();

        inputHandler.SetInputContext(InputHandler.InputContext.EndMenu);
    }

    private void PauseGame()
    {
        if (timeManager == null)
            return;

        timeManager.SetPause(true);
    }

    private void SetGameSpeed(float speed)
    {
        if (timeManager == null)
            return;

        timeManager.SetPause(false);

        timeManager.GameTimeMultiplier = Mathf.Max(0f, speed);
    }

    private void SetGroupAlpha(CanvasGroup group, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        if (group != null)
        {
            group.alpha = alpha;
        }
    }
}
