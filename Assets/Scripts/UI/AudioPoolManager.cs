using UnityEngine;
using System.Collections.Generic;

public class AudioPoolManager : MonoBehaviour
{
    public static AudioPoolManager Instance { get; private set; }
    public AudioSource prefabSource;
    public int poolSize = 50;
    List<AudioSource> pool = new();

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

    public void PlayAt(AudioClip clip, Vector3 pos, float volume = 1f, float pitch = 1f)
    {
        AudioSource s = pool.Find(x => !x.isPlaying);
        if (s == null) s = pool[0];
        s.transform.position = pos;
        s.clip = clip;
        s.volume = volume;
        s.pitch = pitch;
        s.spatialBlend = 1f;
        s.minDistance = 5f;
        s.maxDistance = 80f;
        s.rolloffMode = AudioRolloffMode.Logarithmic;
        s.Play();
    }
}
