using UnityEngine;
using UnityEngine.AI;

public class EnemyCore : MonoBehaviour
{
    [Tooltip("The target GameObject to move towards. If left empty, will automatically search within scene for game objects with the Player tag")]
    public GameObject target=null;
    [Tooltip("Whether the target is stationary (true) or moving (false). Stationary only calculates the destination once")]
    public bool TargetIsStationary = false;
    [Tooltip("If the target is moving, how often (in seconds) to update the path. Does nothing if TargetIsStationary is true")]
    public float UpdateInterval = 0;
    
    private NavMeshPath path;
    private float elapsed = 1; //timer to determine when to update the path to a moving target
    private NavMeshAgent agent=null;

    public float maxHealth = 3f;
    private float currentHealth;

    [Tooltip("How much piercing resistance this enemy has (reduces how much piercing the projectile has left)")]
    public float maxPiercingResistance = 1f;
    private float currentPiercingResistance;

    public bool immortal = false;
    [Tooltip("How long the enemy is frozen for if immortal and health reaches 0 (snail in shell)")]
    public float stunDuration = 5;
    
    [Header("Shell Settings")]
    [Tooltip("The sprite to display when the snail is in shell mode")]
    [SerializeField] public Sprite shellSprite;
    private bool isInShell = false;
    
    // Damage flash system
    private bool isFlashingFromDamage = false;
    private float damageFlashDuration = 0.15f; // How long to flash after taking damage
    private float damageFlashTimer = 0f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip footstepSound = null;

    public AudioClip damageSound = null;
    private AudioSource audioSource = null;

    [Header("Footstep Settings")]
    [Tooltip("How often footsteps play while moving")]
    public float baseStepRate = 1f; // Time between steps at base speed
    public float minPitch = 0.9f;
    public float maxPitch = 1.2f;

    private float footstepTimer = 0f;
    private SnailDirectionController directionController;

    //the level manager mainly used to track enemies
    private LevelManager levelManager;
    private SlimeSplit slimeSplitComponent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = new NavMeshPath();

        //checks that there is a NavMeshAgent on this object to actually move
        try
        {
            agent = this.GetComponent<NavMeshAgent>();
        }
        catch
        {
            Debug.LogWarning("No NavMeshAgent found on " + this.name);
        }

        if (target == null)
        {
            target = GameObject.FindWithTag("Player");
        }

        //if it's stationary we only need to set the destination once
        if (TargetIsStationary)
        {
            try
            {
                agent.SetDestination(target.gameObject.transform.position);
            }
            catch
            {
                Debug.LogWarning("No target specifed on " + this.name);
            }
        }
        currentHealth = maxHealth;
        currentPiercingResistance = maxPiercingResistance;
        
        // Debug initial setup
        Debug.Log($"[{gameObject.name}] Initialized - Health: {currentHealth}/{maxHealth}, Immortal: {immortal}, Stun Duration: {stunDuration}s");

        //increment current enemies
        levelManager = GameObject.FindFirstObjectByType<LevelManager>();
        if(levelManager!=null)
        {
            levelManager.currentObjective.currentEnemies++;
        }
        

        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        
        // Get reference to direction controller (try both snail and slime controllers)
        directionController = GetComponent<SnailDirectionController>();
        
        // If SnailDirectionController not found, try SlimeDirectionController
        if (directionController == null)
        {
            var slimeController = GetComponent<SlimeDirectionController>();
            if (slimeController != null)
            {
                Debug.Log($"[{gameObject.name}] Found SlimeDirectionController instead of SnailDirectionController");
                // SlimeDirectionController doesn't have shell mode, so we'll handle this differently
                directionController = null; // We don't use shell mode for slimes
            }
        }
        
        // If not found, try a more thorough search for SnailDirectionController only
        if (directionController == null)
        {
            Debug.LogWarning($"[{gameObject.name}] SnailDirectionController not found with GetComponent, searching all components...");
            Component[] components = GetComponents<Component>();
            foreach (Component comp in components)
            {
                Debug.Log($"  - {comp.GetType().Name} (Full: {comp.GetType().FullName})");
                if (comp is SnailDirectionController)
                {
                    directionController = comp as SnailDirectionController;
                    Debug.Log($"[{gameObject.name}] Found SnailDirectionController via component search!");
                    break;
                }
            }
        }
        
        // Debug component setup
        if (directionController == null)
        {
            // Check if this is a slime (which doesn't need shell mode)
            if (GetComponent<SlimeDirectionController>() != null)
            {
                Debug.Log($"[{gameObject.name}] This is a slime - shell mode not applicable");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] No SnailDirectionController found after thorough search!");
                Debug.LogError("Shell mode will not work without SnailDirectionController!");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] SnailDirectionController found successfully!");
        }
        
        if (shellSprite == null && immortal)
        {
            Debug.LogWarning($"[{gameObject.name}] No shell sprite assigned but enemy is immortal!");
        }
        else if (shellSprite != null)
        {
            Debug.Log($"[{gameObject.name}] Shell sprite assigned: {shellSprite.name}");
        }

    }

    // Update is called once per frame
    void Update()
    {
        //if the target is a moving target every second the path is recalculated
        if (!TargetIsStationary)
        {
            //timer counts up every frame and once it's over UpdateInterval many seconds
            //the timer resets and it attempts to recalculate the path
            elapsed += Time.deltaTime;
            if (elapsed > UpdateInterval) 
            {
                elapsed = 0;
                
                if(target!=null && NavMesh.CalculatePath(this.transform.position, target.gameObject.transform.position, NavMesh.AllAreas, path))
                {
                    //Debug.Log("Updating path for " + this.name);
                    agent.SetPath(path);
                }
                else if(target==null)
                {
                    Debug.LogWarning("No target specifed on " + this.name);
                }
                else
                {
                    Debug.LogWarning("Failed to calculate path for " + this.name + " to " + target.name);
                }
            }
        }
        
        // Handle damage flash timer
        if (isFlashingFromDamage)
        {
            damageFlashTimer -= Time.deltaTime;
            if (damageFlashTimer <= 0f)
            {
                isFlashingFromDamage = false;
            }
        }                  

        // --- Footstep sound logic: runs every frame ---
        if (footstepSound != null && audioSource != null && agent != null && agent.velocity.magnitude > 0.1f)
        {
            float stepInterval = baseStepRate / agent.speed;
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= stepInterval)
            {
                audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, (agent.speed - 1f) / 4f);
                audioSource.PlayOneShot(footstepSound);
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
            if (audioSource != null)
                audioSource.pitch = 1f;
        }
        // --- End footstep sound logic ---
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Debug health information
        Debug.Log($"[{gameObject.name}] Took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}");
        
        // Trigger damage flash effect
        isFlashingFromDamage = true;
        damageFlashTimer = damageFlashDuration;
        
        // Play damage sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (currentHealth <= 0f && !immortal)
        {
            Debug.Log($"[{gameObject.name}] Died from damage. Health: {currentHealth}");
            //decrement current enemies and increment enemies defeated, then check for objective completion
            if (levelManager != null)
            {
                levelManager.currentObjective.currentEnemies--;
                levelManager.currentObjective.enemiesDefeated++;
                levelManager.checkForCompletion();
            }

            if(slimeSplitComponent!=null)
            {
                slimeSplitComponent.SplitNow();
                Debug.Log($"[{gameObject.name}] Called SplitNow on SlimeSplit component before destruction.");
            }

            Destroy(gameObject);
        }
        else if (currentHealth <= 0f && immortal && !isInShell) // Only enter shell if not already in shell
        {
            Debug.Log($"[{gameObject.name}] Health reached 0, entering shell mode. Immortal: {immortal}");
            StartCoroutine(StunEnemy());
        }
        
        if( slimeSplitComponent == null )
        {
            slimeSplitComponent = GetComponent<SlimeSplit>();

            if (slimeSplitComponent != null)
            {
                slimeSplitComponent.ForceSplitIfEligible();
                Debug.Log($"[{gameObject.name}] Called ForceSplitIfEligible on SlimeSplit component.");
            }
            else
            {
                Debug.Log($"[Health] {gameObject.name} - No SlimeSplit component found.");
            }
        }
    }
    private System.Collections.IEnumerator StunEnemy()
    {
        Debug.Log($"[{gameObject.name}] Enemy enters stun mode - Health: {currentHealth}, Agent stopped: true");
        agent.isStopped = true;
        isInShell = true;
        
        // Only use shell mode if this enemy has a SnailDirectionController
        if (directionController != null && shellSprite != null)
        {
            Debug.Log($"[{gameObject.name}] Setting shell mode ON with sprite: {shellSprite.name}");
            directionController.SetShellMode(true, shellSprite);
        }
        else if (shellSprite != null && GetComponent<SlimeDirectionController>() == null)
        {
            // Fallback for enemies without proper direction controller
            Debug.LogWarning($"[{gameObject.name}] DirectionController is null, using fallback shell mode");
            ApplyShellModeDirectly(true);
        }
        else
        {
            Debug.Log($"[{gameObject.name}] This enemy type doesn't use shell mode (likely a slime)");
        }
        
        Debug.Log($"[{gameObject.name}] Waiting {stunDuration} seconds in stun mode...");
        yield return new WaitForSeconds(stunDuration);
        
        Debug.Log($"[{gameObject.name}] Enemy exits stun mode - Restoring health to {maxHealth}");
        agent.isStopped = false;
        currentHealth = maxHealth;
        isInShell = false;
        
        // Exit shell mode if applicable
        if (directionController != null)
        {
            Debug.Log($"[{gameObject.name}] Setting shell mode OFF");
            directionController.SetShellMode(false, null);
        }
        else if (GetComponent<SlimeDirectionController>() == null)
        {
            Debug.LogWarning($"[{gameObject.name}] DirectionController is null, using fallback to exit shell mode");
            ApplyShellModeDirectly(false);
        }
    }

    public float getPiercingResistance()
    {
        return currentPiercingResistance;
    }
    
    public float getCurrentHealth()
    {
        return currentHealth;
    }
    
    public bool IsFlashingFromDamage()
    {
        return isFlashingFromDamage;
    }
    
    public bool IsInShell()
    {
        return isInShell;
    }
    
    // Debug method to log current health status
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugHealthStatus()
    {
        Debug.Log($"[{gameObject.name}] Health Status - Current: {currentHealth}/{maxHealth}, Immortal: {immortal}, In Shell: {isInShell}");
    }
    
    // Method to force shell mode for testing (only available in editor)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugForceShellMode()
    {
        if (immortal && !isInShell)
        {
            currentHealth = 0;
            Debug.Log($"[{gameObject.name}] Debug: Forcing shell mode");
            StartCoroutine(StunEnemy());
        }
    }
    
    // Method to validate shell sprite assignment
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void ValidateShellSprite()
    {
        if (shellSprite == null)
        {
            Debug.LogError($"[{gameObject.name}] Shell sprite is not assigned! Please assign 'snail_shell_v1' sprite in the inspector.");
        }
        else if (!shellSprite.name.Contains("shell"))
        {
            Debug.LogWarning($"[{gameObject.name}] Current shell sprite '{shellSprite.name}' may not be the correct shell sprite. Expected something containing 'shell'.");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Shell sprite validation passed: {shellSprite.name}");
        }
        
        // Also check what's currently displayed
        if (directionController != null)
        {
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                Debug.Log($"[{gameObject.name}] Currently displayed sprite: {(renderer.sprite != null ? renderer.sprite.name : "NULL")}");
            }
        }
    }
    
    // Debug method to manually test shell mode
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugTestShellMode()
    {
        Debug.Log($"[{gameObject.name}] Testing shell mode manually...");
        if (directionController != null && shellSprite != null)
        {
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                Debug.Log($"[{gameObject.name}] Before shell mode - Current sprite: {(renderer.sprite != null ? renderer.sprite.name : "NULL")}");
                directionController.SetShellMode(true, shellSprite);
                Debug.Log($"[{gameObject.name}] After shell mode - Current sprite: {(renderer.sprite != null ? renderer.sprite.name : "NULL")}");
            }
        }
        else
        {
            if (directionController == null)
                Debug.LogError($"[{gameObject.name}] Cannot test shell mode - directionController is null!");
            if (shellSprite == null)
                Debug.LogError($"[{gameObject.name}] Cannot test shell mode - shellSprite is null!");
        }
    }
    
    // Method to fix missing SnailDirectionController component
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void FixMissingDirectionController()
    {
        if (directionController == null)
        {
            Debug.Log($"[{gameObject.name}] Adding missing SnailDirectionController component...");
            directionController = gameObject.AddComponent<SnailDirectionController>();
            Debug.Log($"[{gameObject.name}] SnailDirectionController component added successfully!");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] SnailDirectionController component already exists.");
        }
    }
    
    // Fallback method to apply shell mode directly without SnailDirectionController
    private void ApplyShellModeDirectly(bool enterShellMode)
    {
        Debug.Log($"[{gameObject.name}] Applying shell mode directly (fallback method): {enterShellMode}");
        
        // Find the sprite renderer in child objects
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Animator animator = GetComponentInChildren<Animator>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}] Cannot apply shell mode - no SpriteRenderer found in children!");
            return;
        }
        
        if (enterShellMode)
        {
            // Disable animator
            if (animator != null)
            {
                animator.enabled = false;
                Debug.Log($"[{gameObject.name}] Animator disabled for shell mode (fallback)");
            }
            
            // Set shell sprite
            if (shellSprite != null)
            {
                Sprite previousSprite = spriteRenderer.sprite;
                spriteRenderer.sprite = shellSprite;
                Debug.Log($"[{gameObject.name}] Shell sprite applied (fallback): {previousSprite?.name} -> {shellSprite.name}");
            }
        }
        else
        {
            // Re-enable animator
            if (animator != null)
            {
                animator.enabled = true;
                Debug.Log($"[{gameObject.name}] Animator re-enabled after shell mode (fallback)");
            }
            
            // Note: We can't restore the original sprite without storing it, 
            // but the animator should handle sprite switching when re-enabled
            Debug.Log($"[{gameObject.name}] Shell mode exited (fallback) - animator will handle sprite restoration");
        }
    }
}
