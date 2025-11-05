using UnityEngine;
using UnityEngine.AI;

public class SlimeDirectionController : MonoBehaviour
{
    [Header("Components")]
    private NavMeshAgent navMeshAgent;
    private SpriteRenderer spriteRenderer;
    
    [Header("Direction Settings")]
    [Tooltip("Minimum velocity magnitude to trigger direction change")]
    public float velocityThreshold = 0.1f;
    
    [Header("Damage Visual Effects")]
    [Tooltip("Color tint when slime takes damage")]
    public Color damageColor = Color.red;
    [Tooltip("How fast the damage color flashes (higher = faster)")]
    public float flashSpeed = 10f;
    
    private Vector3 lastPosition;
    private bool facingRight = true; // Assumes sprite sheet has slime facing right by default
    private Color originalColor;
    private EnemyCore enemyCore;
    private bool isInvulnerable = false;
    private Animator animator;
    
    void Start()
    {
        // Get required components - script is on parent object
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyCore = GetComponent<EnemyCore>();
        
        // SpriteRenderer is on child object named "Visual"
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // Get animator component (also on child object)
        animator = GetComponentInChildren<Animator>();
        
        if (navMeshAgent == null)
        {
            Debug.LogError("SlimeDirectionController: No NavMeshAgent found on " + gameObject.name);
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("SlimeDirectionController: No SpriteRenderer found in children of " + gameObject.name);
        }
        else
        {
            // Store original color for damage effects
            originalColor = spriteRenderer.color;
        }
        
        if (enemyCore == null)
        {
            Debug.LogWarning("SlimeDirectionController: No EnemyCore component found on " + gameObject.name);
        }
        
        lastPosition = transform.position;
        
        // Initialize to face right (no flip)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            facingRight = true;
            Debug.Log($"[{gameObject.name}] SlimeDirectionController initialized, facing right");
        }
    }
    
    void Update()
    {
        if (navMeshAgent == null || spriteRenderer == null) 
        {
            return;
        }
        
        // Lock rotation to prevent NavMeshAgent from rotating the slime
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
        
        // Handle damage visual effects
        HandleDamageEffects();
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
    
    // Handle damage visual effects
    private void HandleDamageEffects()
    {
        if (spriteRenderer == null || enemyCore == null) return;
        
        // Check if enemy is flashing from recent damage
        bool flashingFromDamage = enemyCore.IsFlashingFromDamage();
        
        // Update invulnerable state
        bool shouldBeInvulnerable = flashingFromDamage;
        if (shouldBeInvulnerable != isInvulnerable)
        {
            isInvulnerable = shouldBeInvulnerable;
        }
        
        if (isInvulnerable)
        {
            // Flash between red and original color
            float flash = Mathf.Sin(Time.time * flashSpeed) * 0.5f + 0.5f;
            spriteRenderer.color = Color.Lerp(originalColor, damageColor, flash);
        }
        else
        {
            // Restore original color
            spriteRenderer.color = originalColor;
        }
    }
}
