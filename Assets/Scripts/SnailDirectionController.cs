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
    
    [Header("Damage Visual Effects")]
    [Tooltip("Color tint when snail takes damage")]
    public Color damageColor = Color.red;
    [Tooltip("How fast the damage color flashes (higher = faster)")]
    public float flashSpeed = 10f;
    
    [Header("Shell Settings")]
    private Sprite originalSprite;
    private bool isInShellMode = false;
    private Animator animator;
    
    private Vector3 lastPosition;
    private bool facingRight = true; // Assumes sprite sheet has snail facing right by default
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
        
        // Get animator component (also on child "Renderer" object)
        animator = GetComponentInChildren<Animator>();
        
        if (navMeshAgent == null)
        {
            Debug.LogError("SnailDirectionController: No NavMeshAgent found on " + gameObject.name);
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("SnailDirectionController: No SpriteRenderer found in children of " + gameObject.name);
        }
        else
        {
            // Store original color for damage effects
            originalColor = spriteRenderer.color;
        }
        
        if (animator == null)
        {
            Debug.LogError("SnailDirectionController: No Animator found in children of " + gameObject.name);
        }
        
        if (enemyCore == null)
        {
            Debug.LogWarning("SnailDirectionController: No EnemyCore component found on " + gameObject.name);
        }
        
        lastPosition = transform.position;
        
        // Store original sprite and initialize to face right (no flip)
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
            spriteRenderer.flipX = false;
            facingRight = true;
            Debug.Log($"[{gameObject.name}] Original sprite stored: {(originalSprite != null ? originalSprite.name : "NULL")}");
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
        
        // Don't handle direction changes if in shell mode
        if (isInShellMode)
        {
            return;
        }
        
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
    
    // Method to switch between normal and shell mode
    public void SetShellMode(bool enterShellMode, Sprite shellSprite)
    {
        Debug.Log($"[{gameObject.name}] SetShellMode called - Enter: {enterShellMode}, Sprite: {(shellSprite != null ? shellSprite.name : "NULL")}");
        isInShellMode = enterShellMode;
        
        if (enterShellMode)
        {
            // Disable animator and set shell sprite
            if (animator != null)
            {
                Debug.Log($"[{gameObject.name}] Disabling animator (was enabled: {animator.enabled})");
                animator.enabled = false;
                Debug.Log($"[{gameObject.name}] Animator disabled for shell mode (now enabled: {animator.enabled})");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Animator is null, cannot disable for shell mode");
            }
            
            if (spriteRenderer != null && shellSprite != null)
            {
                Sprite previousSprite = spriteRenderer.sprite;
                spriteRenderer.sprite = shellSprite;
                Debug.Log($"[{gameObject.name}] Shell sprite successfully changed from '{(previousSprite != null ? previousSprite.name : "NULL")}' to '{shellSprite.name}' (Expected: snail_shell_v1)");
                
                // Verify the change actually took effect
                if (spriteRenderer.sprite == shellSprite)
                {
                    Debug.Log($"[{gameObject.name}] ✓ Shell sprite change CONFIRMED - sprite is now: {spriteRenderer.sprite.name}");
                }
                else
                {
                    Debug.LogError($"[{gameObject.name}] ✗ Shell sprite change FAILED - sprite is still: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
                }
                // Don't flip the shell sprite - keep it facing the same direction
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Cannot set shell sprite - SpriteRenderer: {(spriteRenderer != null ? "OK" : "NULL")}, ShellSprite: {(shellSprite != null ? shellSprite.name : "NULL")}");
            }
        }
        else
        {
            // Re-enable animator and restore original sprite
            if (animator != null)
            {
                animator.enabled = true;
                Debug.Log($"[{gameObject.name}] Animator re-enabled after shell mode");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Animator is null, cannot re-enable after shell mode");
            }
            
            if (spriteRenderer != null && originalSprite != null)
            {
                spriteRenderer.sprite = originalSprite;
                Debug.Log($"[{gameObject.name}] Original sprite restored: {originalSprite.name}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Cannot restore original sprite - SpriteRenderer: {(spriteRenderer != null ? "OK" : "NULL")}, OriginalSprite: {(originalSprite != null ? "OK" : "NULL")}");
            }
        }
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
    
    // Debug method to test shell mode directly
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugForceShellSprite()
    {
        EnemyCore enemyCore = GetComponent<EnemyCore>();
        if (enemyCore != null)
        {
            var shellSpriteField = typeof(EnemyCore).GetField("shellSprite");
            if (shellSpriteField != null)
            {
                Sprite shellSprite = (Sprite)shellSpriteField.GetValue(enemyCore);
                if (shellSprite != null)
                {
                    Debug.Log($"[{gameObject.name}] Debug: Forcing shell sprite directly - {shellSprite.name}");
                    SetShellMode(true, shellSprite);
                }
                else
                {
                    Debug.LogError($"[{gameObject.name}] Debug: Shell sprite is null!");
                }
            }
        }
    }

}