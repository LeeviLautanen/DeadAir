using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 7f;
    public float maxZoom = 30f;

    private TechManager techManager;
    private InputHandler inputHandler;
    private Camera mainCamera;
    private float zoomMultiplier;

    private void Start()
    {
        techManager = FindFirstObjectByType<TechManager>();
        inputHandler = FindFirstObjectByType<InputHandler>();

        inputHandler.RegisterMoveHandler(HandleMoveInput, priority: 0);
        inputHandler.RegisterScrollHandler(HandleScrollInput, priority: 0);

        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        zoomMultiplier = mainCamera.orthographicSize;
    }

    private bool HandleMoveInput(Vector2 move)
    {
        transform.position += moveSpeed * zoomMultiplier * Time.deltaTime * (Vector3)move;
        return true;
    }

    private bool HandleScrollInput(Vector2 scroll)
    {
        if (scroll.y == 0f)
        {
            return false;
        }

        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll.y * zoomSpeed, minZoom, maxZoom);
        zoomMultiplier = mainCamera.orthographicSize;
        return true;
    }
}
