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
    public Vector3 MouseWorldPosition { get; private set; }
    public delegate bool ClickHandler(MouseClick click);
    public event Action<Vector3> PointerMove;
    public event Action<Key> NumberKeyPressed;
    public event Action<SaveAction> SaveActionTriggered;

    private readonly List<(int priority, ClickHandler handler)> clickHandlers = new();
    private Camera mainCamera;
    private Vector3 screenPosCache = Vector3.zero;
    private readonly MouseClick cachedClick = new();

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateMouseWorldPosition();
        PointerMove?.Invoke(MouseWorldPosition);
        HandleMouseClicks();
        HandleNumberKeys();
        HandleSaveControls();
    }

    private void UpdateMouseWorldPosition()
    {
        var screenPos = Mouse.current.position.ReadValue();
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
                        break;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }

    private void HandleNumberKeys()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) NumberKeyPressed?.Invoke(Key.Digit1);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) NumberKeyPressed?.Invoke(Key.Digit2);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) NumberKeyPressed?.Invoke(Key.Digit3);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) NumberKeyPressed?.Invoke(Key.Digit4);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) NumberKeyPressed?.Invoke(Key.Digit5);
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
