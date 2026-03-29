using UnityEngine;
using System.Collections.Generic;

public class MeteoriteParticleSystem : MonoBehaviour
{
    public static MeteoriteParticleSystem Instance { get; private set; }

    private static readonly Logger log = new(nameof(MeteoriteParticleSystem));
    private TimeManager timeManager;
    [SerializeField] private ParticleSystem prefab;
    [SerializeField] private int poolSize = 50;
    private readonly Queue<ParticleSystem> pool = new();
    private readonly List<ParticleSystem> active = new();

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
            var ps = Instantiate(prefab, transform);
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }

    private void Start()
    {
        timeManager = TimeManager.Instance;
    }

    private void Update()
    {
        float dt = timeManager.DeltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var ps = active[i];

            ps.Simulate(dt, true, false, false);

            if (ps.particleCount == 0)
            {
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
                active.RemoveAt(i);
            }
        }
    }

    public void Play(Vector2 position, Vector2 direction)
    {
        if (pool.Count == 0)
        {
            log.Warning("Particle system pool ran out");
            return;
        }

        var ps = pool.Dequeue();

        // Set position
        ps.transform.position = position;

        // Set rotation to shoot particles in the param direction
        float halfArc = ps.shape.arc * 0.5f;
        float dirAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ps.transform.rotation = Quaternion.Euler(0f, 0f, dirAngle - halfArc);

        ps.gameObject.SetActive(true);
        ps.Play(true);
        ps.Pause();

        active.Add(ps);
    }
}
