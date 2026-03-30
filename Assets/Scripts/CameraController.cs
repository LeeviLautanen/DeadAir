using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 7f;
    public float maxZoom = 30f;
    public float CamOrthoSize => mainCamera.orthographicSize;
    public int targetFrameRate = 60;

    private InputHandler inputHandler;
    private Camera mainCamera;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();

        inputHandler.RegisterMoveHandler(HandleMoveInput, priority: 0);
        inputHandler.RegisterScrollHandler(HandleScrollInput, priority: 0);

        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();

        //Application.targetFrameRate = targetFrameRate;
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.UnregisterMoveHandler(HandleMoveInput);
            inputHandler.UnregisterScrollHandler(HandleScrollInput);
        }
    }

    private bool HandleMoveInput(Vector2 move)
    {
        transform.position += moveSpeed * mainCamera.orthographicSize * Time.deltaTime * (Vector3)move;
        return true;
    }

    private bool HandleScrollInput(Vector2 scroll)
    {
        if (scroll.y == 0f)
        {
            return false;
        }

        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll.y * zoomSpeed, minZoom, maxZoom);
        return false; // Why would i consume a scroll event?
    }
}
