using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.5f;
    TMP_Text fpsText;

    private float timeSinceLastUpdate;
    private int frameCount;

    private void Start()
    {
        fpsText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        frameCount++;

        if (timeSinceLastUpdate >= updateInterval)
        {
            float fps = frameCount / timeSinceLastUpdate;
            fpsText.SetText($"FPS: {fps:F0}");

            timeSinceLastUpdate = 0f;
            frameCount = 0;
        }
    }
}
