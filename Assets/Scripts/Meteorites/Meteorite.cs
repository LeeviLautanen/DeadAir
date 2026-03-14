using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float Damage = 50f;
    public float Speed = 30f;
    public float Lifetime = 10f;
    public float ImpactAudioDelay = 0.5f;
    public Vector2 ImpactAudioPitchRange = new(0.7f, 1.2f);
    public bool HasCollided = false;

    private static readonly Logger log = new(nameof(Meteorite));
    private MeteoriteParticleSystem meteoriteParticleSystem;
    private AudioPoolManager audioPoolManager;
    private TimeManager timeManager;
    private Rigidbody2D rb;
    private float lifeTimer = 0f;
    private Vector2 moveDirection;
    public AudioClip ImpactClip;

    private void Start()
    {
        meteoriteParticleSystem = MeteoriteParticleSystem.Instance;
        audioPoolManager = AudioPoolManager.Instance;
        timeManager = TimeManager.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveDirection = -rb.transform.up;

        ScheduleImpactAudio();
    }

    private void ScheduleImpactAudio()
    {
        int layerMask = LayerMask.GetMask("Ground", "BuildingDamage") & ~(1 << gameObject.layer);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, Mathf.Infinity, layerMask);

        if (hit.collider != GetComponent<Collider2D>())
        {
            float distance = Vector2.Distance(transform.position, hit.point);
            float travelTime = distance / (Speed * timeManager.GameTimeMultiplier);
            float randomPitch = Random.Range(ImpactAudioPitchRange.x, ImpactAudioPitchRange.y);
            float timeToImpact = travelTime + ImpactAudioDelay / randomPitch;
            audioPoolManager.PlayAt(ImpactClip, hit.point, timeToImpact, randomPitch);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Trigger particles only once
        if (HasCollided)
        {
            return;
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (meteoriteParticleSystem != null)
            {
                Vector2 particleAngle = moveDirection;
                particleAngle.y = Mathf.Abs(particleAngle.y);
                meteoriteParticleSystem.Play(new(transform.position.x, -1f), particleAngle);
            }
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("BuildingDamage"))
        {
            if (meteoriteParticleSystem != null)
            {
                Vector2 particleAngle = moveDirection;
                particleAngle.y = Mathf.Abs(particleAngle.y);
                var collisionPoint = collider.ClosestPoint(transform.position);
                var collisionNormal = ((Vector2)transform.position - collisionPoint).normalized;

                collisionPoint -= collisionNormal * 0.5f; // Move slightly towards the building

                meteoriteParticleSystem.Play(collisionPoint, collisionNormal);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + Speed * Time.fixedDeltaTime * timeManager.GameTimeMultiplier * moveDirection);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
