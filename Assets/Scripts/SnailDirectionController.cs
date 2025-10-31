using UnityEngine;
using UnityEngine.AI;

public class SnailDirectionController : MonoBehaviour
{
    [Header("Components")]
    private NavMeshAgent navMeshAgent;
    private SpriteRenderer spriteRenderer;
    
    [Header("Direction Settings")]
    [Tooltip("Minimum velocity magnitude to trigger direction change")]
    public float velocityThreshold = 0.1f;
    

    
    private Vector3 lastPosition;
    private bool facingRight = true; // Assumes sprite sheet has snail facing right by default
    
    void Start()
    {
        // Get required components - script is on parent object
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        // SpriteRenderer is on child object named "Renderer"
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (navMeshAgent == null)
        {
            Debug.LogError("SnailDirectionController: No NavMeshAgent found on " + gameObject.name);
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("SnailDirectionController: No SpriteRenderer found in children of " + gameObject.name);
        }
        
        lastPosition = transform.position;
        
        // Initialize sprite to face right (no flip)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            facingRight = true;
        }
    }
    
    void Update()
    {
        if (navMeshAgent == null || spriteRenderer == null) 
        {
            return;
        }
        
        // Lock rotation to prevent NavMeshAgent from rotating the snail
        transform.rotation = Quaternion.identity;
        
        // Check movement direction based on velocity
        Vector3 velocity = navMeshAgent.velocity;
        
        // Only change direction if moving fast enough
        if (velocity.magnitude > velocityThreshold)
        {
            // Check X-axis movement (left/right in world space)
            if (velocity.x > 0.01f && !facingRight)
            {
                // Moving in positive X direction
                spriteRenderer.flipX = false;
                facingRight = true;
            }
            else if (velocity.x < -0.01f && facingRight)
            {
                // Moving in negative X direction
                spriteRenderer.flipX = true;
                facingRight = false;
            }
        }
    }
    
    private void FlipSprite(bool flip)
    {
        spriteRenderer.flipX = flip;
    }
    
    // Public method to manually set direction (useful for other scripts)
    public void SetFacingDirection(bool shouldFaceRight)
    {
        if (shouldFaceRight != facingRight)
        {
            FlipSprite(!shouldFaceRight);
            facingRight = shouldFaceRight;
        }
    }
    
    // Get current facing direction
    public bool IsFacingRight()
    {
        return facingRight;
    }
    

}