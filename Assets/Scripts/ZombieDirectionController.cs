using UnityEngine;
using UnityEngine.AI;

public class ZombieDirectionController : MonoBehaviour
{
    [Header("Components")]
    private NavMeshAgent navMeshAgent;
    private SpriteRenderer spriteRenderer;
    
    [Header("Direction Settings")]
    [Tooltip("Minimum velocity magnitude to trigger direction change")]
    public float velocityThreshold = 0.1f;
    
    [Header("Damage Visual Effects")]
    [Tooltip("Color tint when enemy takes damage")]
    public Color damageColor = Color.red;
    [Tooltip("How fast the damage color flashes (higher = faster)")]
    public float flashSpeed = 10f;
    
    [Header("Debug")]
    public bool debugMode = false;
    
    private Vector3 lastPosition;
    private bool facingRight = true; // Assumes sprite sheet has zombie facing right by default
    private Color originalColor;
    private EnemyCore enemyCore;
    private bool isInvulnerable = false;
    
    void Start()
    {
        // Get required components - script is on parent object
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyCore = GetComponent<EnemyCore>();
        
        // SpriteRenderer is on child object named "Renderer"
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (navMeshAgent == null)
        {
            Debug.LogError("ZombieDirectionController: No NavMeshAgent found on " + gameObject.name);
        }
        else
        {
            // Disable NavMeshAgent rotation to prevent spinning
            navMeshAgent.updateRotation = false;
            if (debugMode) Debug.Log("ZombieDirectionController: Disabled NavMeshAgent rotation for " + gameObject.name);
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("ZombieDirectionController: No SpriteRenderer found in children of " + gameObject.name);
        }
        else
        {
            // Store original color for damage effects
            originalColor = spriteRenderer.color;
        }
        
        if (enemyCore == null)
        {
            Debug.LogWarning("ZombieDirectionController: No EnemyCore component found on " + gameObject.name);
        }
        
        lastPosition = transform.position;
        
        // Initialize sprite to face right (no flip)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            facingRight = true;
        }
        
        // Lock initial rotation
        transform.rotation = Quaternion.identity;
    }
    
    void Update()
    {
        if (navMeshAgent == null || spriteRenderer == null) 
        {
            return;
        }
        
        // Lock rotation to prevent any spinning
        transform.rotation = Quaternion.identity;
        
        // Ensure child renderer object maintains correct top-down rotation (90 degrees on X-axis)
        Transform rendererTransform = spriteRenderer.transform;
        if (rendererTransform != null)
        {
            rendererTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        
        // Check movement direction based on velocity
        Vector3 velocity = navMeshAgent.velocity;
        
        if (debugMode && Time.frameCount % 60 == 0) // Only log once per second to avoid spam
        {
            Debug.Log($"Zombie velocity: {velocity}, magnitude: {velocity.magnitude}, facingRight: {facingRight}, transform.rotation: {transform.rotation}");
        }
        
        // Only change direction if moving fast enough
        if (velocity.magnitude > velocityThreshold)
        {
            // Check X-axis movement (left/right in world space)
            if (velocity.x > 0.01f && !facingRight)
            {
                // Moving in positive X direction
                spriteRenderer.flipX = false;
                facingRight = true;
                if (debugMode) Debug.Log("Zombie now facing RIGHT");
            }
            else if (velocity.x < -0.01f && facingRight)
            {
                // Moving in negative X direction
                spriteRenderer.flipX = true;
                facingRight = false;
                if (debugMode) Debug.Log("Zombie now facing LEFT");
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
        
        // Check if enemy is currently stunned (equivalent to player's invulnerability)
        bool currentlyStunned = enemyCore.getCurrentHealth() <= 0 && enemyCore.immortal;
        
        // Check if enemy is flashing from recent damage
        bool flashingFromDamage = enemyCore.IsFlashingFromDamage();
        
        // Update invulnerable state
        bool shouldBeInvulnerable = currentlyStunned || flashingFromDamage;
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