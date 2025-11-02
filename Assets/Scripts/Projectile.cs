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
    public float maxPiercing = 0f;
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
        }
        else
        {
            currentDamage = baseDamage;
            Debug.LogWarning("Projectile has no StatManager reference. Using base damage.");
        }

        currentPiercing = shooterStatManager != null ? shooterStatManager.GetStatValue(StatType.BulletPiercing) : currentPiercing;
        currentBounces = 0;
        speed = shooterStatManager != null ? shooterStatManager.GetStatValue(StatType.BulletSpeed) : speed;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null && spawnSound != null)
            audioSource.PlayOneShot(spawnSound);
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
        if(other.CompareTag("Player")||other.CompareTag("Bullet"))
        {
            // Ignore collisions with the player
            return;
        }
        if (other.CompareTag("Enemy"))
        {
            EnemyCore enemy = other.GetComponent<EnemyCore>();                        
            enemy?.TakeDamage(currentDamage);
            currentPiercing = currentPiercing - enemy.getPiercingResistance();

        }
        if(other.gameObject.layer.Equals(LayerMask.NameToLayer("Environment")))
        {
            currentPiercing = currentPiercing - 5;
            
        }

        if (currentPiercing < 0)//if the bullet has gone through all that it can, it bounces if it can and if not, destroy it
        {
            if (currentBounces < maxBounces)
            {
                BounceBullet();
            }
            else 
            { 
                AudioSource.PlayClipAtPoint(breakSound, transform.position); //play the sound where the bullet was.
                                                                             //PlayOneShot would start it and it would instantly cut because the audio source was destroyed along with the bullet
                Destroy(gameObject);
            }                
        }
        else if(other.CompareTag("Enemy")||other.gameObject.layer.Equals("Environment"))
        {
            audioSource.PlayOneShot(pierceSound);
        }
    }

    private void BounceBullet()
    {
        // Reflect the moveDirection vector off the surface normal
        RaycastHit hit;
        if (Physics.Raycast(transform.position, moveDirection, out hit))
        {
            moveDirection = Vector3.Reflect(moveDirection, hit.normal);
            currentBounces++;
            audioSource.PlayOneShot(bounceSound);
        }
    }
}
