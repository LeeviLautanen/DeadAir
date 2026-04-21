using UnityEngine;
using System.Reflection;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 1f;
    public float minZoom = 7f;
    public float maxZoom = 30f;
    public float CamOrthoSize => mainCamera != null ? mainCamera.orthographicSize : 0f;
    public Vector2 xBounds = new Vector2(-10f, 10f);
    public Vector2 yBounds = new Vector2(-10f, 10f);

    private InputHandler inputHandler;
    private Camera mainCamera;
    private Component pixelPerfectCamera;
    private PropertyInfo assetsPPUProperty;
    private FieldInfo assetsPPUField;
    private PropertyInfo refResolutionYProperty;
    private FieldInfo refResolutionYField;
    private int cachedAssetsPPU = 1;
    private int cachedRefResolutionY = 1080;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();

        if (inputHandler == null)
        {
            Debug.LogError("InputHandler not found for CameraController.", this);
            enabled = false;
            return;
        }

        inputHandler.RegisterMoveHandler(HandleMoveInput, priority: 0);
        inputHandler.RegisterScrollHandler(HandleScrollInput, priority: 0);

        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("CameraController requires a Camera component.", this);
            enabled = false;
            return;
        }

        pixelPerfectCamera = FindPixelPerfectCamera(mainCamera);

        if (pixelPerfectCamera != null)
        {
            CachePixelPerfectMembers();
            RefreshPixelPerfectCache();
            ClampZoomToLimits();
        }

        ClampCameraToBounds();
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

    private int GetMinAllowedAssetsPPU()
    {
        float maxOrthoAllowed = Mathf.Max(0.0001f, GetMaxZoomThatFitsBounds());
        int minAssetsPPU = Mathf.CeilToInt(GetPixelPerfectRefResolutionY() / (2f * maxOrthoAllowed));
        return Mathf.Max(1, minAssetsPPU);
    }

    private int GetMaxAllowedAssetsPPU()
    {
        float minOrthoAllowed = Mathf.Max(0.0001f, minZoom);
        int maxAssetsPPU = Mathf.FloorToInt(GetPixelPerfectRefResolutionY() / (2f * minOrthoAllowed));
        return Mathf.Max(1, maxAssetsPPU);
    }

    private void ClampZoomToLimits()
    {
        int minAllowedAssetsPPU = GetMinAllowedAssetsPPU();
        int maxAllowedAssetsPPU = GetMaxAllowedAssetsPPU();

        if (maxAllowedAssetsPPU < minAllowedAssetsPPU)
            maxAllowedAssetsPPU = minAllowedAssetsPPU;

        SetPixelPerfectAssetsPPU(Mathf.Clamp(GetPixelPerfectAssetsPPU(), minAllowedAssetsPPU, maxAllowedAssetsPPU));
    }

    private Component FindPixelPerfectCamera(Camera camera)
    {
        Component[] components = camera.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null && components[i].GetType().Name == "PixelPerfectCamera")
                return components[i];
        }

        return null;
    }

    private void CachePixelPerfectMembers()
    {
        if (pixelPerfectCamera == null)
            return;

        System.Type type = pixelPerfectCamera.GetType();
        assetsPPUProperty = type.GetProperty("assetsPPU");
        assetsPPUField = type.GetField("assetsPPU");
        refResolutionYProperty = type.GetProperty("refResolutionY");
        refResolutionYField = type.GetField("refResolutionY");
    }

    private void RefreshPixelPerfectCache()
    {
        cachedAssetsPPU = ReadPixelPerfectInt(assetsPPUProperty, assetsPPUField, 1);
        cachedRefResolutionY = ReadPixelPerfectInt(refResolutionYProperty, refResolutionYField, 1080);
    }

    private int ReadPixelPerfectInt(PropertyInfo property, FieldInfo field, int fallbackValue)
    {
        if (pixelPerfectCamera == null)
            return fallbackValue;

        if (property != null && property.PropertyType == typeof(int) && property.CanRead)
            return (int)property.GetValue(pixelPerfectCamera);

        if (field != null && field.FieldType == typeof(int))
            return (int)field.GetValue(pixelPerfectCamera);

        return fallbackValue;
    }

    private void WritePixelPerfectInt(PropertyInfo property, FieldInfo field, int value)
    {
        if (pixelPerfectCamera == null)
            return;

        if (property != null && property.PropertyType == typeof(int) && property.CanWrite)
        {
            property.SetValue(pixelPerfectCamera, value);
            return;
        }

        if (field != null && field.FieldType == typeof(int))
            field.SetValue(pixelPerfectCamera, value);
    }

    private int GetPixelPerfectAssetsPPU()
    {
        return Mathf.Max(1, cachedAssetsPPU);
    }

    private void SetPixelPerfectAssetsPPU(int value)
    {
        int clamped = Mathf.Max(1, value);
        cachedAssetsPPU = clamped;
        WritePixelPerfectInt(assetsPPUProperty, assetsPPUField, clamped);
    }

    private int GetPixelPerfectRefResolutionY()
    {
        return Mathf.Max(1, cachedRefResolutionY);
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
        Vector2 clampedMove = move;
        if (clampedMove.sqrMagnitude > 1f)
            clampedMove.Normalize();

        transform.position += moveSpeed * mainCamera.orthographicSize * Time.deltaTime * (Vector3)clampedMove;
        ClampCameraToBounds();

        return true;
    }

    private bool HandleScrollInput(Vector2 scroll)
    {
        if (scroll.y == 0f)
            return false;

        if (pixelPerfectCamera != null)
        {
            int direction = scroll.y > 0f ? 1 : -1;
            int zoomStep = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(zoomSpeed)));
            SetPixelPerfectAssetsPPU(GetPixelPerfectAssetsPPU() + direction * zoomStep);
            ClampZoomToLimits();
        }
        else
        {
            float targetSize = mainCamera.orthographicSize - scroll.y * zoomSpeed;
            float maxZoomAllowedByBounds = GetMaxZoomThatFitsBounds();

            mainCamera.orthographicSize = maxZoomAllowedByBounds < minZoom
                ? maxZoomAllowedByBounds
                : Mathf.Clamp(targetSize, minZoom, maxZoomAllowedByBounds);
        }

        ClampCameraToBounds();

        return false;
    }
}
