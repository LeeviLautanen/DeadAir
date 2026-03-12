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
    public Vector2 MouseWorldPosition { get; private set; }
    public InputActionAsset inputActions;
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

    private readonly List<(int priority, ClickHandler handler)> clickHandlers = new();
    private readonly List<(int priority, MoveHandler handler)> moveHandlers = new();
    private readonly List<(int priority, ScrollHandler handler)> scrollHandlers = new();

    private static readonly Logger log = new(true, LogLevel.Info);
    private InputAction cameraMoveAction;
    private InputAction cameraZoomAction;
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

        InputActionMap actionMap = inputActions.FindActionMap("CameraMovement");
        cameraMoveAction = actionMap.FindAction("CameraMove");
        cameraZoomAction = actionMap.FindAction("CameraZoom");
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
        HandleSaveControls();
    }

    private void UpdateMouseWorldPosition()
    {
        var screenPos = Mouse.current.position.ReadValue();
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

    private void HandleSaveControls()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed)
        {
            if (Keyboard.current.sKey.wasPressedThisFrame) SaveActionTriggered?.Invoke(SaveAction.Save);
            else if (Keyboard.current.rKey.wasPressedThisFrame) SaveActionTriggered?.Invoke(SaveAction.Load);
            else if (Keyboard.current.cKey.wasPressedThisFrame) SaveActionTriggered?.Invoke(SaveAction.Clear);
        }
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

    public enum MouseButton { Left, Right }

    public class MouseClick
    {
        public MouseButton Button;
        public Vector3 WorldPosition;
    }
}
