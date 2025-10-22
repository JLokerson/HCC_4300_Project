using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    [Tooltip("How long the bullet can exist before automatically destroying itself\n(Like if it somehow gets out of bounds it won't fly forever)")]
    public float lifetime = 3f; // How long the bullet exists before being destroyed

    public float damage = 1f;

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
            other.GetComponent<EnemyCore>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        if(other.gameObject.layer.Equals(LayerMask.NameToLayer("Environment")))
        {
            Destroy(gameObject);
        }
    }
}
