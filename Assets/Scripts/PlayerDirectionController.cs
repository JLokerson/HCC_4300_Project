using UnityEngine;

public class PlayerDirectionController : MonoBehaviour
{
    [Header("Components")]
    private CharacterController characterController;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    [Header("Direction Settings")]
    [Tooltip("Minimum velocity magnitude to trigger direction change")]
    public float velocityThreshold = 0.1f;
    
    [Header("Sprite Settings")]
    [Tooltip("Scale factor for the sprite (1.0 = original size, 0.5 = half size)")]
    public float spriteScale = 0.2f;
    [Header("Damage Visual Effects")]
    [Tooltip("Color tint when player takes damage/is invulnerable")]
    public Color damageColor = Color.red;
    [Tooltip("How fast the damage color flashes (higher = faster)")]
    public float flashSpeed = 10f;
    
    private Vector3 lastPosition;
    private bool facingRight = true; // Assumes sprite sheet has player facing right by default
    private bool isMoving = false;
    private bool isShooting = false;
    private bool isInvulnerable = false;
    private Color originalColor;
    private Health healthComponent;
    
    void Start()
    {
        // Get required components
        characterController = GetComponent<CharacterController>();
        
        // SpriteRenderer and Animator are on child object named "Renderer"
        Transform rendererChild = transform.Find("Renderer");
        if (rendererChild != null)
        {
            spriteRenderer = rendererChild.GetComponent<SpriteRenderer>();
            animator = rendererChild.GetComponent<Animator>();
        }
        
        if (characterController == null)
        {
            Debug.LogError("PlayerDirectionController: No CharacterController found on " + gameObject.name);
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerDirectionController: No SpriteRenderer found in Renderer child of " + gameObject.name);
        }
        
        if (animator == null)
        {
            Debug.LogError("PlayerDirectionController: No Animator found in Renderer child of " + gameObject.name);
        }
        
        // Get Health component for damage effects
        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            Debug.LogWarning("PlayerDirectionController: No Health component found on " + gameObject.name);
        }
        
        lastPosition = transform.position;
        
        // Initialize sprite to face right (no flip)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            facingRight = true;
            
            // Store original color
            originalColor = spriteRenderer.color;
        }
    }
    
    void OnEnable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDamaged += OnPlayerDamaged;
        }
    }
    
    void OnDisable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDamaged -= OnPlayerDamaged;
        }
    }
    
    void Update()
    {
        if (characterController == null || spriteRenderer == null || animator == null) 
        {
            return;
        }
        
        // Calculate movement based on position change (since CharacterController doesn't have velocity like NavMeshAgent)
        Vector3 currentPosition = transform.position;
        Vector3 deltaPosition = currentPosition - lastPosition;
        Vector3 velocity = deltaPosition / Time.deltaTime;
        
        // Check if player is moving
        bool wasMoving = isMoving;
        isMoving = velocity.magnitude > velocityThreshold;
        
        // Handle direction flipping based on horizontal movement
        if (isMoving)
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
        
        // Update animator parameters
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsShooting", isShooting);
        
        // Handle damage visual effects
        HandleDamageEffects();
        
        lastPosition = currentPosition;
    }
    
    // Public method to set shooting state (call this from CharacterCore when shooting)
    public void SetShooting(bool shooting)
    {
        isShooting = shooting;
        if (animator != null)
        {
            animator.SetBool("IsShooting", isShooting);
        }
    }
    
    // Public method to manually set direction (useful for other scripts)
    public void SetFacingDirection(bool shouldFaceRight)
    {
        if (shouldFaceRight != facingRight && spriteRenderer != null)
        {
            spriteRenderer.flipX = !shouldFaceRight;
            facingRight = shouldFaceRight;
        }
    }
    
    // Get current facing direction
    public bool IsFacingRight()
    {
        return facingRight;
    }
    
    // Get current movement state
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // Get current shooting state
    public bool IsShooting()
    {
        return isShooting;
    }
    
    // Handle damage visual effects
    private void HandleDamageEffects()
    {
        if (spriteRenderer == null || healthComponent == null) return;
        
        // Check if player is currently invulnerable
        bool currentlyInvulnerable = healthComponent.IsInvulnerable;
        
        if (currentlyInvulnerable != isInvulnerable)
        {
            isInvulnerable = currentlyInvulnerable;
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
    
    // Called when player takes damage
    private void OnPlayerDamaged(float damageAmount)
    {
        // The visual effect will be handled by HandleDamageEffects() checking invulnerability
        Debug.Log($"PlayerDirectionController: Player took {damageAmount} damage");
    }
}