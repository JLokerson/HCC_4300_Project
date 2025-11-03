using UnityEngine;

/// <summary>
/// Simple test script to manually trigger and verify shell mode functionality.
/// Add this script to any GameObject and use the context menu to test.
/// </summary>
public class ShellModeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private EnemyCore targetSnail;
    
    void Start()
    {
        // Auto-find snail if not assigned
        if (targetSnail == null)
        {
            targetSnail = FindFirstObjectByType<EnemyCore>();
        }
    }
    
    [ContextMenu("Test Shell Mode ON")]
    public void TestShellModeOn()
    {
        if (targetSnail == null)
        {
            Debug.LogError("No EnemyCore (snail) assigned or found!");
            return;
        }
        
        Debug.Log("=== TESTING SHELL MODE ON ===");
        
        // Get the shell sprite from the EnemyCore
        var shellSpriteField = typeof(EnemyCore).GetField("shellSprite");
        if (shellSpriteField != null)
        {
            Sprite shellSprite = (Sprite)shellSpriteField.GetValue(targetSnail);
            
            if (shellSprite == null)
            {
                Debug.LogError("Shell sprite is null on the EnemyCore!");
                return;
            }
            
            Debug.Log($"Found shell sprite: {shellSprite.name}");
            
            // Get the direction controller
            SnailDirectionController dirController = targetSnail.GetComponent<SnailDirectionController>();
            if (dirController == null)
            {
                Debug.LogError("No SnailDirectionController found!");
                return;
            }
            
            // Get the sprite renderer to check before/after
            SpriteRenderer spriteRenderer = targetSnail.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("No SpriteRenderer found in children!");
                return;
            }
            
            Debug.Log($"Current sprite before shell mode: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
            
            // Trigger shell mode
            dirController.SetShellMode(true, shellSprite);
            
            Debug.Log($"Current sprite after shell mode: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
            
            // Check if it worked
            if (spriteRenderer.sprite == shellSprite)
            {
                Debug.Log("✓ SUCCESS: Shell mode activated correctly!");
            }
            else
            {
                Debug.LogError("✗ FAILED: Shell mode did not work correctly!");
            }
        }
    }
    
    [ContextMenu("Test Shell Mode OFF")]
    public void TestShellModeOff()
    {
        if (targetSnail == null)
        {
            Debug.LogError("No EnemyCore (snail) assigned or found!");
            return;
        }
        
        Debug.Log("=== TESTING SHELL MODE OFF ===");
        
        SnailDirectionController dirController = targetSnail.GetComponent<SnailDirectionController>();
        if (dirController != null)
        {
            SpriteRenderer spriteRenderer = targetSnail.GetComponentInChildren<SpriteRenderer>();
            Debug.Log($"Current sprite before exiting shell: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
            
            dirController.SetShellMode(false, null);
            
            Debug.Log($"Current sprite after exiting shell: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
        }
    }
    
    [ContextMenu("Force Snail Health to Zero")]
    public void ForceHealthZero()
    {
        if (targetSnail == null)
        {
            Debug.LogError("No EnemyCore (snail) assigned or found!");
            return;
        }
        
        Debug.Log("=== FORCING SNAIL HEALTH TO ZERO ===");
        
        // Deal enough damage to bring health to 0
        float currentHealth = targetSnail.getCurrentHealth();
        Debug.Log($"Current health: {currentHealth}");
        
        if (currentHealth > 0)
        {
            targetSnail.TakeDamage(currentHealth);
            Debug.Log("Damage dealt to bring health to zero.");
        }
        else
        {
            Debug.Log("Health is already at zero.");
        }
    }
    
    [ContextMenu("Check Snail Status")]
    public void CheckSnailStatus()
    {
        if (targetSnail == null)
        {
            Debug.LogError("No EnemyCore (snail) assigned or found!");
            return;
        }
        
        Debug.Log("=== SNAIL STATUS CHECK ===");
        Debug.Log($"Health: {targetSnail.getCurrentHealth()}");
        Debug.Log($"Immortal: {targetSnail.immortal}");
        Debug.Log($"In Shell: {targetSnail.IsInShell()}");
        
        SpriteRenderer spriteRenderer = targetSnail.GetComponentInChildren<SpriteRenderer>();
        Debug.Log($"Current Sprite: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
        
        Animator animator = targetSnail.GetComponentInChildren<Animator>();
        Debug.Log($"Animator Enabled: {(animator != null ? animator.enabled.ToString() : "NULL")}");
    }
}