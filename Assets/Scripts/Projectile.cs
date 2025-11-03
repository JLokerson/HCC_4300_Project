using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    [Tooltip("How long the bullet can exist before automatically destroying itself\n(Like if it somehow gets out of bounds it won't fly forever)")]
    public float lifetime = 3f; // How long the bullet exists before being destroyed

    public float baseDamage = 1f;
    private float currentDamage;

    [Tooltip("How many enemies/objects the bullet can pierce through before being destroyed\nNote: enemies have a piercing resistance stat")]
    public float basePiercing = 0f;
    private float currentPiercing;

    [Tooltip("How many times bullet can bounce off surfaces after it has no piercing left")]
    public int maxBounces = 0;
    private int currentBounces;

    [Header("Audio")]
    [SerializeField]
    private AudioClip bounceSound = null;
    [SerializeField]
    private AudioClip breakSound = null;
    [SerializeField]
    private AudioClip spawnSound = null;
    [SerializeField]
    private AudioClip pierceSound = null;

    private AudioSource audioSource = null;

    private Vector3 moveDirection = Vector3.zero;
       
    // Reference to the shooter's stat manager (passed in when bullet is created)
    private StatManager shooterStatManager;

    public void Initialize(StatManager statManager)
    {
        shooterStatManager = statManager;
    }

    public void SetTarget(Vector3 position, float spread = 0) //this is called in CharacterCore when the bullet is fired. default spread is 0 if none is specified
    {
        // Calculate direction at the moment of firing 
        Vector3 target = new Vector3(position.x, transform.position.y, position.z);
        moveDirection = (target - transform.position).normalized;

        // Apply spread
        if (spread > 0)
        {
            float xSpread = Random.Range(-spread, spread);
            float zSpread = Random.Range(-spread, spread);
            moveDirection += new Vector3(xSpread, 0, zSpread);
            moveDirection.Normalize();
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the bullet after its lifetime expires
        
        // Get damage from shooter's StatManager if available, otherwise use base damage
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
        
        if (audioSource != null && spawnSound != null)
            audioSource.PlayOneShot(spawnSound);
            
        Debug.Log($"[Projectile] Initialized with Damage: {currentDamage}, Piercing: {currentPiercing}, Speed: {speed}");
    }

    void Update()
    {
        if (moveDirection != Vector3.zero) //move in direction if it has a direction
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("Bullet"))
        {
            // Ignore collisions with the player
            return;
        }
        
        bool hitSomething = false;
        
        if (other.CompareTag("Enemy"))
        {
            EnemyCore enemy = other.GetComponent<EnemyCore>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
                float piercingResistance = enemy.getPiercingResistance();
                currentPiercing -= piercingResistance;
                Debug.Log($"[Projectile] Hit enemy. Piercing: {currentPiercing + piercingResistance} -> {currentPiercing}");
                hitSomething = true;
            }
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            currentPiercing -= 5;
            Debug.Log($"[Projectile] Hit environment. Piercing: {currentPiercing + 5} -> {currentPiercing}");
            hitSomething = true;
        }

        // Only check piercing/bouncing if we actually hit something
        if (hitSomething)
        {
            if (currentPiercing < 0) // Out of piercing
            {
                if (currentBounces < maxBounces)
                {
                    BounceBullet(other);
                }
                else 
                { 
                    if (breakSound != null)
                        AudioSource.PlayClipAtPoint(breakSound, transform.position);
                    Destroy(gameObject);
                }                
            }
            else // Still has piercing
            {
                if (audioSource != null && pierceSound != null)
                    audioSource.PlayOneShot(pierceSound);
            }
        }
    }

    private void BounceBullet(Collider hitCollider)
    {
        // Calculate normal from the collision point
        Vector3 collisionPoint = hitCollider.ClosestPoint(transform.position);
        Vector3 normal = (transform.position - collisionPoint).normalized;
        
        if (normal != Vector3.zero)
        {
            moveDirection = Vector3.Reflect(moveDirection, normal);
            currentBounces++;
            Debug.Log($"[Projectile] Bounced! Count: {currentBounces}/{maxBounces}");
            
            if (audioSource != null && bounceSound != null)
                audioSource.PlayOneShot(bounceSound);
        }
    }
}
