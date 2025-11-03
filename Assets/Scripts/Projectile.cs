using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;

    public float baseDamage = 1f;
    private float currentDamage;

    public float basePiercing = 0f;
    private float currentPiercing;

    public int maxBounces = 0;
    private int currentBounces;

    [Header("Audio")]
    [SerializeField] private AudioClip bounceSound = null;
    [SerializeField] private AudioClip breakSound = null;
    [SerializeField] private AudioClip spawnSound = null;
    [SerializeField] private AudioClip pierceSound = null;
    private AudioSource audioSource = null;

    // NEW: assign this to the visual child (SpriteRenderer/Mesh/Quad)
    [Header("Visual")]
    [Tooltip("Child transform that renders the bullet sprite/mesh.")]
    public Transform Graphic;

    // If your sprite points along +X in its source image, leave this true.
    // If it points along +Z, set this to false and we’ll use LookRotation instead.
    [SerializeField] private bool spriteFacesPositiveX = true;

    private Vector3 moveDirection = Vector3.zero;

    private StatManager shooterStatManager;

    public void Initialize(StatManager statManager) { shooterStatManager = statManager; }

    public void SetTarget(Vector3 position, float spread = 0)
    {
        // Direction on XZ plane
        Vector3 target = new Vector3(position.x, transform.position.y, position.z);
        moveDirection = (target - transform.position).normalized;

        // Apply optional spread
        if (spread > 0f)
        {
            moveDirection += new Vector3(Random.Range(-spread, spread), 0f, Random.Range(-spread, spread));
            moveDirection.Normalize();
        }

        OrientGraphic();   // NEW: face where we’ll travel
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (shooterStatManager != null)
        {
            currentDamage = shooterStatManager.GetStatValue(StatType.BulletDamage);
            currentPiercing = shooterStatManager.GetStatValue(StatType.BulletPiercing);
            speed = shooterStatManager.GetStatValue(StatType.BulletSpeed);
        }
        else
        {
            currentDamage = baseDamage;
            currentPiercing = basePiercing;
            Debug.LogWarning("Projectile has no StatManager reference. Using base values.");
        }

        currentBounces = 0;
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && spawnSound != null) audioSource.PlayOneShot(spawnSound);

        // If fired by spawning with a preset moveDirection, make sure we’re oriented.
        OrientGraphic();
    }

    void Update()
    {
        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
            // If moveDirection can change during flight (e.g., homing), keep the visual aligned:
            // OrientGraphic();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bullet")) return;

        bool hitSomething = false;

        if (other.CompareTag("Enemy"))
        {
            EnemyCore enemy = other.GetComponent<EnemyCore>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
                float pr = enemy.getPiercingResistance();
                currentPiercing -= pr;
                hitSomething = true;
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            currentPiercing -= 5;
            hitSomething = true;
        }

        if (hitSomething)
        {
            if (currentPiercing < 0)
            {
                if (currentBounces < maxBounces)
                {
                    BounceBullet(other);
                }
                else
                {
                    if (breakSound != null) AudioSource.PlayClipAtPoint(breakSound, transform.position);
                    Destroy(gameObject);
                }
            }
            else
            {
                if (audioSource != null && pierceSound != null) audioSource.PlayOneShot(pierceSound);
            }
        }
    }

    private void BounceBullet(Collider hitCollider)
    {
        Vector3 collisionPoint = hitCollider.ClosestPoint(transform.position);
        Vector3 normal = (transform.position - collisionPoint).normalized;

        if (normal != Vector3.zero)
        {
            moveDirection = Vector3.Reflect(moveDirection, normal);
            currentBounces++;

            // NEW: re-orient visual after changing direction
            OrientGraphic();

            if (audioSource != null && bounceSound != null) audioSource.PlayOneShot(bounceSound);
        }
    }

    // NEW: rotate the visual so its “right” (or “forward”) matches moveDirection on XZ.
    private void OrientGraphic()
    {
        if (Graphic == null || moveDirection == Vector3.zero) return;

        Vector3 flatDir = new Vector3(moveDirection.x, 0f, moveDirection.z);
        if (flatDir.sqrMagnitude < 0.0001f) return;

        if (spriteFacesPositiveX)
        {
            // Make the sprite’s +X (its nose) point at our direction
            Graphic.rotation = Quaternion.FromToRotation(Vector3.right, flatDir);
        }
        else
        {
            // If the model faces +Z by default, use LookRotation
            Graphic.rotation = Quaternion.LookRotation(flatDir, Vector3.up);
        }
    }
}
