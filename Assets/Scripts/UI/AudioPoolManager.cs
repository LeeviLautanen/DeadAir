using UnityEngine;
using System.Collections.Generic;

public class AudioPoolManager : MonoBehaviour
{
    public static AudioPoolManager Instance { get; private set; }
    public AudioSource prefabSource;
    public int poolSize = 50;

    private static readonly Logger log = new(nameof(AudioPoolManager));
    private List<AudioSource> pool = new();

    void Awake()
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
