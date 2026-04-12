using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SaveAction
{
    Save,
    Load,
    Clear
}

public class InputHandler : MonoBehaviour
{
    public enum InputContext
    {
        Gameplay,
        ResearchTree,
        EndMenu
    }

    public Vector2 MouseWorldPosition { get; private set; }
    public InputActionAsset inputActions;
    public InputContext CurrentContext { get; private set; } = InputContext.Gameplay;
    public static readonly Key[] numberKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
    };

    // Consuming events
    public delegate bool ClickHandler(MouseClick click);
    public delegate bool MoveHandler(Vector2 move);
    public delegate bool ScrollHandler(Vector2 scroll);

    // Non-consuming events
    public event Action<Vector2> PointerMove;
    public event Action<Vector2> PointerMoveDelta;
    public event Action<Key> NumberKeyPressed;
    public event Action<SaveAction> SaveActionTriggered;
    public event Action<MouseButton> MouseButtonReleased;
    public event Action GameTimeIncreaseRequested;
    public event Action GameTimeDecreaseRequested;
    public event Action ResearchMenuToggleRequested;
    public event Action PauseToggleRequested;
    public event Action HelpMenuToggled;
    public event Action PauseMenuToggled;

    private readonly List<(int priority, ClickHandler handler)> clickHandlers = new();
    private readonly List<(int priority, MoveHandler handler)> moveHandlers = new();
    private readonly List<(int priority, ScrollHandler handler)> scrollHandlers = new();

    private static readonly Logger log = new(nameof(InputHandler));
    private InputActionMap gameplayActionMap;
    private InputActionMap researchTreeActionMap;
    private InputActionMap globalHotkeysActionMap;
    private InputAction cameraMoveAction;
    private InputAction cameraZoomAction;
    private InputAction pointerPositionAction;
    private InputAction speedUpTimeAction;
    private InputAction slowDownTimeAction;
    private InputAction toggleResearchAction;
    private InputAction togglePauseAction;
    private InputAction toggleHelpMenu;
    private InputAction togglePauseMenuAction;
    private InputAction saveGameAction;
    private InputAction loadGameAction;
    private InputAction clearGameAction;
    private Camera mainCamera;
    private Vector2 screenPosCache = Vector2.zero;
    private readonly MouseClick cachedClick = new();
    private Vector2 cachedMove = new();
    private Vector2 cachedPointerDelta = new();
    private Vector2 cachedScroll = new();

    private void Start()
    {
        mainCamera = Camera.main;

        if (inputActions == null)
        {
            Debug.LogError("NO INPUT ASSET FOR INPUT CONTROLLER DUMBASS");
            enabled = false;
            return;
        }

        gameplayActionMap = inputActions.FindActionMap("Gameplay");
        researchTreeActionMap = inputActions.FindActionMap("ResearchTree");

        if (gameplayActionMap == null || researchTreeActionMap == null)
        {
            Debug.LogError("Input maps Gameplay and ResearchTree are required in InputActions asset.");
            enabled = false;
            return;
        }

        gameplayActionMap.Enable();
        researchTreeActionMap.Disable();

        cameraMoveAction = gameplayActionMap.FindAction("ViewMove");
        cameraZoomAction = gameplayActionMap.FindAction("ViewZoom");
        pointerPositionAction = gameplayActionMap.FindAction("MousePosition");
        speedUpTimeAction = gameplayActionMap.FindAction("SpeedUpTime", false);
        slowDownTimeAction = gameplayActionMap.FindAction("SlowDownTime", false);
        toggleResearchAction = gameplayActionMap.FindAction("ToggleResearchMenu", false);
        togglePauseAction = gameplayActionMap.FindAction("TogglePause", false);
        toggleHelpMenu = gameplayActionMap.FindAction("ToggleHelpMenu", false);
        saveGameAction = gameplayActionMap.FindAction("SaveGame", false);
        loadGameAction = gameplayActionMap.FindAction("LoadGame", false);
        clearGameAction = gameplayActionMap.FindAction("ClearGame", false);

        globalHotkeysActionMap = inputActions.FindActionMap("GlobalHotkeys", false);
        if (globalHotkeysActionMap != null)
        {
            globalHotkeysActionMap.Enable();
            togglePauseMenuAction = globalHotkeysActionMap.FindAction("TogglePauseMenu", false);
        }

        if (cameraMoveAction == null || cameraZoomAction == null || pointerPositionAction == null)
        {
            Debug.LogError("Actions ViewMove, ViewZoom, and MousePosition are required in Gameplay map.");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        // Mouse
        UpdateMouseWorldPosition();
        PointerMove?.Invoke(MouseWorldPosition);
        PointerMoveDelta?.Invoke(cachedPointerDelta);
        HandleMouseClicks();
        HandleScroll();

        // Keyboard
        HandleMove();
        HandleNumberKeys();
        HandleGameplayHotkeys();
        HandleGlobalHotkeys();
    }

    public void SetInputContext(InputContext context)
    {
        if (CurrentContext == context)
            return;

        switch (context)
        {
            case InputContext.ResearchTree:
                researchTreeActionMap.Enable();
                gameplayActionMap.Disable();
                cameraMoveAction = researchTreeActionMap.FindAction("ViewMove");
                cameraZoomAction = researchTreeActionMap.FindAction("ViewZoom");
                pointerPositionAction = researchTreeActionMap.FindAction("MousePosition");
                toggleResearchAction = researchTreeActionMap.FindAction("ToggleResearchMenu");
                break;

            case InputContext.Gameplay:
                gameplayActionMap.Enable();
                researchTreeActionMap.Disable();
                cameraMoveAction = gameplayActionMap.FindAction("ViewMove");
                cameraZoomAction = gameplayActionMap.FindAction("ViewZoom");
                pointerPositionAction = gameplayActionMap.FindAction("MousePosition");
                toggleResearchAction = gameplayActionMap.FindAction("ToggleResearchMenu");
                break;

            case InputContext.EndMenu:
                gameplayActionMap.Disable();
                researchTreeActionMap.Disable();
                break;
        }

        CurrentContext = context;
    }

    public void RegisterMoveHandler(MoveHandler handler, int priority = 0)
    {
        moveHandlers.Add((priority, handler));
        moveHandlers.Sort((a, b) => b.priority.CompareTo(a.priority));
    }

    public void UnregisterMoveHandler(MoveHandler handler)
    {
        moveHandlers.RemoveAll(x => x.handler == handler);
    }

    public void RegisterScrollHandler(ScrollHandler handler, int priority = 0)
    {
        scrollHandlers.Add((priority, handler));
        scrollHandlers.Sort((a, b) => b.priority.CompareTo(a.priority));
    }

    public void UnregisterScrollHandler(ScrollHandler handler)
    {
        scrollHandlers.RemoveAll(x => x.handler == handler);
    }

    public void RegisterClickHandler(ClickHandler handler, int priority = 0)
    {
        clickHandlers.Add((priority, handler));
        clickHandlers.Sort((a, b) => b.priority.CompareTo(a.priority));
    }

    public void UnregisterClickHandler(ClickHandler handler)
    {
        clickHandlers.RemoveAll(x => x.handler == handler);
    }

    private void UpdateMouseWorldPosition()
    {
        var screenPos = pointerPositionAction.ReadValue<Vector2>();
        cachedPointerDelta = screenPos - screenPosCache;
        screenPosCache.x = screenPos.x;
        screenPosCache.y = screenPos.y;
        MouseWorldPosition = mainCamera.ScreenToWorldPoint(screenPosCache);
    }

    private void HandleMouseClicks()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            MouseButton btn = Mouse.current.leftButton.wasPressedThisFrame ? MouseButton.Left : MouseButton.Right;
            cachedClick.Button = btn;
            cachedClick.WorldPosition = MouseWorldPosition;

            foreach (var (priority, handler) in clickHandlers)
            {
                try
                {
                    bool handled = handler(cachedClick);
                    if (handled)
                    {
                        log.Info($"Click handled by {handler.Method.DeclaringType.Name} with priority {priority}");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            MouseButtonReleased?.Invoke(MouseButton.Left);
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            MouseButtonReleased?.Invoke(MouseButton.Right);
        }
    }

    private void HandleMove()
    {
        cachedMove = cameraMoveAction.ReadValue<Vector2>();

        foreach (var (priority, handler) in moveHandlers)
        {
            try
            {
                bool handled = handler(cachedMove);
                if (handled)
                    break;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void HandleScroll()
    {
        cachedScroll = cameraZoomAction.ReadValue<Vector2>();

        foreach (var (priority, handler) in scrollHandlers)
        {
            try
            {
                bool handled = handler(cachedScroll);
                if (handled)
                    break;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void HandleNumberKeys()
    {
        if (Keyboard.current == null) return;

        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Keyboard.current[numberKeys[i]].wasPressedThisFrame)
            {
                NumberKeyPressed?.Invoke(numberKeys[i]);
                break;
            }
        }
    }

    private void HandleGameplayHotkeys()
    {
        if (speedUpTimeAction != null && speedUpTimeAction.WasPressedThisFrame())
        {
            GameTimeIncreaseRequested?.Invoke();
        }

        if (slowDownTimeAction != null && slowDownTimeAction.WasPressedThisFrame())
        {
            GameTimeDecreaseRequested?.Invoke();
        }

        if (toggleResearchAction != null && toggleResearchAction.WasPressedThisFrame())
        {
            ResearchMenuToggleRequested?.Invoke();
        }

        if (togglePauseAction != null && togglePauseAction.WasPressedThisFrame())
        {
            PauseToggleRequested?.Invoke();
        }

        if (toggleHelpMenu != null && toggleHelpMenu.WasPressedThisFrame())
        {
            HelpMenuToggled?.Invoke();
        }

        if (saveGameAction != null && saveGameAction.WasPressedThisFrame())
        {
            SaveActionTriggered?.Invoke(SaveAction.Save);
        }

        if (loadGameAction != null && loadGameAction.WasPressedThisFrame())
        {
            SaveActionTriggered?.Invoke(SaveAction.Load);
        }

        if (clearGameAction != null && clearGameAction.WasPressedThisFrame())
        {
            SaveActionTriggered?.Invoke(SaveAction.Clear);
        }
    }

    private void HandleGlobalHotkeys()
    {
        if (togglePauseMenuAction != null && togglePauseMenuAction.WasPressedThisFrame())
        {
            PauseMenuToggled?.Invoke();
        }
    }

    public enum MouseButton { Left, Right }

    public class MouseClick
    {
        public MouseButton Button;
        public Vector3 WorldPosition;
    }
}
