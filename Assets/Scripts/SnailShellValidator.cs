using UnityEngine;

/// <summary>
/// Temporary validation script to check if the snail has the correct shell sprite assigned.
/// Attach this to the snail prefab or instance to validate the setup.
/// </summary>
public class SnailShellValidator : MonoBehaviour
{
    void Start()
    {
        ValidateSnailShellSetup();
    }
    
    [ContextMenu("Validate Snail Shell Setup")]
    public void ValidateSnailShellSetup()
    {
        Debug.Log($"=== Snail Shell Validation for {gameObject.name} ===");
        
        // Check EnemyCore component
        EnemyCore enemyCore = GetComponent<EnemyCore>();
        if (enemyCore == null)
        {
            Debug.LogError("No EnemyCore component found!");
            return;
        }
        
        // Check if immortal (required for shell behavior)
        if (!enemyCore.immortal)
        {
            Debug.LogWarning("Snail is not set as immortal! Shell behavior won't work.");
        }
        else
        {
            Debug.Log("✓ Snail is immortal - shell behavior should work");
        }
        
        // Check shell sprite assignment
        var shellSpriteField = typeof(EnemyCore).GetField("shellSprite");
        if (shellSpriteField != null)
        {
            Sprite shellSprite = (Sprite)shellSpriteField.GetValue(enemyCore);
            if (shellSprite == null)
            {
                Debug.LogError("Shell sprite is not assigned! Please assign 'snail_shell_v1' in the inspector.");
            }
            else if (shellSprite.name.Contains("shell"))
            {
                Debug.Log($"✓ Shell sprite assigned: {shellSprite.name}");
            }
            else
            {
                Debug.LogWarning($"Assigned sprite '{shellSprite.name}' may not be the shell sprite!");
            }
        }
        
        // Check SnailDirectionController
        SnailDirectionController dirController = GetComponent<SnailDirectionController>();
        if (dirController == null)
        {
            Debug.LogError("No SnailDirectionController found!");
        }
        else
        {
            Debug.Log("✓ SnailDirectionController found");
        }
        
        // Check child renderer structure
        Transform rendererChild = transform.Find("Renderer");
        if (rendererChild == null)
        {
            Debug.LogError("No 'Renderer' child object found!");
        }
        else
        {
            SpriteRenderer spriteRenderer = rendererChild.GetComponent<SpriteRenderer>();
            Animator animator = rendererChild.GetComponent<Animator>();
            
            if (spriteRenderer == null)
            {
                Debug.LogError("No SpriteRenderer on Renderer child!");
            }
            else
            {
                Debug.Log($"✓ SpriteRenderer found with sprite: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
            }
            
            if (animator == null)
            {
                Debug.LogError("No Animator on Renderer child!");
            }
            else
            {
                Debug.Log("✓ Animator found on Renderer child");
            }
        }
        
        Debug.Log("=== Validation Complete ===");
    }
    
    [ContextMenu("Force Shell Mode Test")]
    public void ForceShellModeTest()
    {
        EnemyCore enemyCore = GetComponent<EnemyCore>();
        if (enemyCore != null)
        {
            enemyCore.DebugForceShellMode();
        }
    }
}