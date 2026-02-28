using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public InputActionAsset inputActions;

    private InputAction cameraMoveAction;
    private InputAction cameraZoomAction;
    private TechManager techManager;
    private Camera mainCamera;
    private float zoomMultiplier;

    private void Awake()
    {
        techManager = FindFirstObjectByType<TechManager>();

        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        zoomMultiplier = mainCamera.orthographicSize;

        if (inputActions == null)
        {
            Debug.LogError("NO INPUT ASSET FOR CAMERA CONTROLLER DUMBASS");
            enabled = false;
            return;
        }

        InputActionMap actionMap = inputActions.FindActionMap("CameraMovement");
        cameraMoveAction = actionMap.FindAction("CameraMove");
        cameraZoomAction = actionMap.FindAction("CameraZoom");
    }

    private void Update()
    {
        if (techManager.IsVisible)
            return;

        Vector2 move = cameraMoveAction.ReadValue<Vector2>();
        transform.position += moveSpeed * zoomMultiplier * Time.deltaTime * (Vector3)move;

        Vector2 scroll = cameraZoomAction.ReadValue<Vector2>();
        if (scroll.y != 0f)
        {
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll.y * zoomSpeed, minZoom, maxZoom);
            zoomMultiplier = mainCamera.orthographicSize;
        }
    }
}
