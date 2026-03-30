using UnityEngine;
using System.Collections.Generic;

public class AudioPoolManager : MonoBehaviour
{
    public static AudioPoolManager Instance { get; private set; }
    public AudioSource prefabSource;
    public int poolSize = 50;

    private static readonly Logger log = new(nameof(AudioPoolManager));
    private CameraController cameraController;
    private List<AudioSource> pool = new();
    private float lastCameraZoom;
    private float baseMinDistance = 15f;
    private float baseMaxDistance = 500f;
    private float baseVolume = 0.1f;
    private float baseOrthoSize = 20f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            var s = Instantiate(prefabSource, transform);
            s.playOnAwake = false;
            pool.Add(s);
        }
    }

    private void Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
    }

    private void Update()
    {
        if (cameraController == null)
            return;

        if (lastCameraZoom != cameraController.CamOrthoSize)
        {
            float zoom = cameraController.CamOrthoSize / baseOrthoSize;

            foreach (var s in pool)
            {
                s.minDistance = baseMinDistance * zoom;
                s.maxDistance = baseMaxDistance * zoom;
                s.volume = Mathf.Clamp01(baseVolume / (zoom * zoom));
            }
            lastCameraZoom = cameraController.CamOrthoSize;
        }
    }

    public void PlayAt(AudioClip clip, Vector3 pos, float delay = 0f, float pitch = 1f)
    {
        AudioSource s = pool.Find(x => !x.isPlaying);
        if (s == null)
        {
            log.Warning("No available audio sources in pool");
            return;
        }

        if (delay < 0)
        {
            log.Warning($"Impact sound delay was negative ({delay}s), timing might be off.");
            delay = 0;
        }

        s.transform.position = pos;
        s.clip = clip;
        s.pitch = pitch;
        s.PlayDelayed(delay);
        log.Info($"Scheduled audio with delay {delay}");
    }

    public void PauseAll()
    {
        foreach (var s in pool)
        {
            if (s.isPlaying)
                s.Pause();
        }
    }

    public void UnpauseAll()
    {
        foreach (var s in pool)
        {
            if (s.clip != null && s.time > 0f)
                s.UnPause();
        }
    }
}
