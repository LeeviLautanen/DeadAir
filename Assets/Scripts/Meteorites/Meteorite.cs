using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float Damage = 50f;
    public float Speed = 30f;
    public float RotationSpeed = 0f;
    public float Lifetime = 10f;
    public float ImpactAudioDelay = 0.5f;
    public Vector2 ImpactAudioPitchRange = new(0.7f, 1.2f);
    public bool HasCollided = false;
    public AudioClip WhooshClip;
    public AudioClip GroundImpactClip;
    public AudioClip BuildingImpactClip;
    public AudioClip ShieldImpactClip;

    private static readonly Logger log = new(nameof(Meteorite));
    private MeteoriteParticleSystem meteoriteParticleSystem;
    private AudioPoolManager audioPoolManager;
    private TimeManager timeManager;
    private Rigidbody2D rb;
    private float lifeTimer = 0f;
    private Vector2 moveDirection;
    private GameObject trailGO;
    private float randomPitch;
    private bool impactVisualized = false;

    private void Awake()
    {
        meteoriteParticleSystem = MeteoriteParticleSystem.Instance;
        audioPoolManager = AudioPoolManager.Instance;
        timeManager = TimeManager.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveDirection = -rb.transform.up;
        trailGO = transform.Find("Trail").gameObject;

        randomPitch = Random.Range(ImpactAudioPitchRange.x, ImpactAudioPitchRange.y);
    }

    public void ScheduleWhooshAudio(float impactPosX)
    {
        Vector2 impactPos = new(impactPosX, 0);
        float distance = Vector2.Distance(transform.position, impactPos);
        float travelTime = distance / (Speed * timeManager.GameTimeMultiplier);
        float timeToImpact = travelTime + ImpactAudioDelay / randomPitch;
        audioPoolManager.PlayAt(WhooshClip, impactPos, timeToImpact, randomPitch);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (impactVisualized) return;

        var layerName = collider.gameObject.layer;

        if (layerName == LayerMask.NameToLayer("MeteoriteDestroyer"))
        {
            Destroy(gameObject);
            return;
        }

        if (layerName == LayerMask.NameToLayer("Ground"))
        {
            if (meteoriteParticleSystem != null)
            {
                Vector2 particleAngle = moveDirection;
                particleAngle.y = Mathf.Abs(particleAngle.y);
                meteoriteParticleSystem.Play(new(transform.position.x, -1f), particleAngle);
                audioPoolManager.PlayAt(GroundImpactClip, transform.position, 0f, randomPitch);
                log.Info($"Ground impact visualized");
            }
        }

        if (!collider.transform.parent.TryGetComponent(out Building building) || building.PlacementMode)
        {
            return;
        }

        if (!collider.TryGetComponent(out BuildingCollider buildingCollider))
        {
            return;
        }
        else if (buildingCollider.Type == BuildingColliderType.Damage || buildingCollider.Type == BuildingColliderType.Placement)
        {
            if (meteoriteParticleSystem != null)
            {
                Vector2 particleAngle = moveDirection;
                particleAngle.y = Mathf.Abs(particleAngle.y);
                // Perimeter point on the collider
                var collisionPoint = collider.ClosestPoint(transform.position);
                // Vector from the building center to the collision point
                var collisionNormal = (collisionPoint - (Vector2)collider.transform.position).normalized;

                collisionPoint -= collisionNormal * 0.5f; // Move slightly towards the building

                meteoriteParticleSystem.Play(collisionPoint, collisionNormal);
                audioPoolManager.PlayAt(BuildingImpactClip, collisionPoint, 0f, randomPitch);
                log.Info($"Building impact visualized");
            }
        }
        else if (buildingCollider.Type == BuildingColliderType.Shield)
        {
            audioPoolManager.PlayAt(ShieldImpactClip, transform.position, 0f, randomPitch);
            log.Info($"Shield impact visualized");
        }

        impactVisualized = true;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + Speed * Time.fixedDeltaTime * timeManager.GameTimeMultiplier * moveDirection);
        rb.MoveRotation(rb.rotation + RotationSpeed * Time.fixedDeltaTime * timeManager.GameTimeMultiplier);
        trailGO.transform.rotation = Quaternion.LookRotation(Vector3.forward, -moveDirection);

        lifeTimer += timeManager.DeltaTime;
        if (lifeTimer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
