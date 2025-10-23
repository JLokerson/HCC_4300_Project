using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    [Tooltip("How long the bullet can exist before automatically destroying itself\n(Like if it somehow gets out of bounds it won't fly forever)")]
    public float lifetime = 3f; // How long the bullet exists before being destroyed

    public float maxDamage = 1f;
    private float currentDamage;

    [Tooltip("How many enemies/objects the bullet can pierce through before being destroyed\nNote: enemies have a piercing resistance stat")]
    public float maxPiercing = 1f;
    private float currentPiercing;

    private Vector3 moveDirection = Vector3.zero;
       

    public void SetTarget(Vector3 position) //this is called in CharacterCore when the bullet is fired
    {
        // Calculate direction at the moment of firing
        Vector3 target = new Vector3(position.x, transform.position.y, position.z);
        moveDirection = (target - transform.position).normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the bullet after its lifetime expires
        currentDamage = maxDamage;
        currentPiercing = maxPiercing;
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
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyCore>()?.TakeDamage(currentDamage);
            currentPiercing=currentPiercing-other.GetComponent<EnemyCore>()!.getPiercingResistance();
            
        }
        if(other.gameObject.layer.Equals(LayerMask.NameToLayer("Environment")))
        {
            currentPiercing = currentPiercing - 5;
            
        }

        if (currentPiercing <= 0)//if the bullet has gone through all that it can, destroy it
        {
            Destroy(gameObject);
        }
    }
}
