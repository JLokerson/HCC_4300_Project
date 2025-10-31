using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    [Tooltip("How long the bullet can exist before automatically destroying itself\n(Like if it somehow gets out of bounds it won't fly forever)")]
    public float lifetime = 3f; // How long the bullet exists before being destroyed

    public float maxDamage = 1f;
    private float currentDamage;

    [Tooltip("How many enemies/objects the bullet can pierce through before being destroyed\nNote: enemies have a piercing resistance stat")]
    public float maxPiercing = 0f;
    private float currentPiercing;

    [Tooltip("How many times bullet can bounce off surfaces after it has no piercing left")]
    public int maxBounces = 0;
    private int currentBounces;

    private Vector3 moveDirection = Vector3.zero;
       

    public void SetTarget(Vector3 position,float spread=0) //this is called in CharacterCore when the bullet is fired. default spread is 0 if none is specified
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
        currentDamage = maxDamage;
        currentPiercing = maxPiercing;
        currentBounces = 0;
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
            other.GetComponent<EnemyCore>()?.TakeDamage(currentDamage);
            currentPiercing=currentPiercing-other.GetComponent<EnemyCore>()!.getPiercingResistance();
            
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
                Destroy(gameObject);
            }
                
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
        }
    }
}
