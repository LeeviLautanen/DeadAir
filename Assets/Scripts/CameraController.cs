using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 7f;
    public float maxZoom = 30f;
    public float CamOrthoSize => mainCamera.orthographicSize;
    public int targetFrameRate = 60;
    public Vector2 xBounds = new Vector2(-10f, 10f);
    public Vector2 yBounds = new Vector2(-10f, 10f);

    private InputHandler inputHandler;
    private Camera mainCamera;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();

        inputHandler.RegisterMoveHandler(HandleMoveInput, priority: 0);
        inputHandler.RegisterScrollHandler(HandleScrollInput, priority: 0);

        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.UnregisterMoveHandler(HandleMoveInput);
            inputHandler.UnregisterScrollHandler(HandleScrollInput);
        }
    }

    private float GetMaxZoomThatFitsBounds()
    {
        float worldWidth = Mathf.Abs(xBounds.y - xBounds.x);
        float worldHeight = Mathf.Abs(yBounds.y - yBounds.x);
        return Mathf.Min(maxZoom, worldHeight * 0.5f, worldWidth * 0.5f / mainCamera.aspect);
    }

    private void ClampCameraToBounds()
    {
        float worldXMin = Mathf.Min(xBounds.x, xBounds.y);
        float worldXMax = Mathf.Max(xBounds.x, xBounds.y);
        float worldYMin = Mathf.Min(yBounds.x, yBounds.y);
        float worldYMax = Mathf.Max(yBounds.x, yBounds.y);

        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;

        float x = transform.position.x;
        float y = transform.position.y;

        if (worldXMax - worldXMin >= 2f * halfWidth)
            x = Mathf.Clamp(x, worldXMin + halfWidth, worldXMax - halfWidth);
        else
            x = (worldXMin + worldXMax) * 0.5f;

        if (worldYMax - worldYMin >= 2f * halfHeight)
            y = Mathf.Clamp(y, worldYMin + halfHeight, worldYMax - halfHeight);
        else
            y = (worldYMin + worldYMax) * 0.5f;

        transform.position = new Vector3(x, y, transform.position.z);
    }

    private bool HandleMoveInput(Vector2 move)
    {
        transform.position += moveSpeed * mainCamera.orthographicSize * Time.deltaTime * (Vector3)move;
        ClampCameraToBounds();
        return true;
    }

    private bool HandleScrollInput(Vector2 scroll)
    {
        if (scroll.y == 0f)
            return false;

        float targetSize = mainCamera.orthographicSize - scroll.y * zoomSpeed;
        float maxZoomAllowedByBounds = GetMaxZoomThatFitsBounds();

        mainCamera.orthographicSize = maxZoomAllowedByBounds < minZoom
            ? maxZoomAllowedByBounds
            : Mathf.Clamp(targetSize, minZoom, maxZoomAllowedByBounds);

        ClampCameraToBounds();
        return false;
    }
}
