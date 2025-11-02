using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Base max health (used if no StatManager present)")]
    public float baseMaxHealth = 100f;
    public float currentHealth;
    public bool destroyOnDeath = false;

    // ensure OnDeath only fires once
    private bool hasDied = false;

    [Header("Damage Rules")]
    public bool canTakeDamage = true;
    public float invulnSecondsAfterHit = 0.15f;  // i-frames after a hit

    public event Action<float> OnDamaged; // passes damage amount
    public event Action<float> OnHealed;  // passes heal amount
    public event Action OnDeath;

    private bool invulnerable;
    
    // Public property to check invulnerability status
    public bool IsInvulnerable => invulnerable;

    private AudioClip damageSound = null;
    private AudioSource audioSource = null;
    
    // Reference to stat manager
    private StatManager statManager;
    
    // Property to get current max health from stat manager
    public float maxHealth
    {
        get
        {
            if (statManager != null)
                return statManager.GetStatValue(StatType.MaxHealth);
            return baseMaxHealth;
        }
    }

    void Awake()
    {
        // Initialize health if it's not set or if it's 0
        if (currentHealth <= 0f)
        {
            // Try to get StatManager from this GameObject
        statManager = GetComponent<StatManager>();
        }
        
        // Initialize audio components
        
        audioSource = GetComponent<AudioSource>();
        damageSound = GetComponent<CharacterCore>()?.damageSound;
    }
    
    void Start()
    {
        // Initialize health AFTER StatManager has set up its stats in Awake()
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
    }
    
    void OnEnable()
    {
        // Subscribe to stat changes if manager exists
        if (statManager != null)
        {
            statManager.OnStatChanged += HandleStatChanged;
        }
    }
    
    void OnDisable()
    {
        if (statManager != null)
        {
            statManager.OnStatChanged -= HandleStatChanged;
        }
    }
    
    private void HandleStatChanged(StatType statType, float newValue)
    {
        // When max health increases, you have options:
        if (statType == StatType.MaxHealth)
        {
            // Option 1: Keep current health as-is (player still damaged)
            currentHealth = Mathf.Min(newValue, currentHealth);
            
            // Option 2: Scale health proportionally (uncomment if preferred)
            // float oldValue = maxHealth; // would need to cache the old value
            // float healthPercent = oldValue > 0 ? currentHealth / oldValue : 1f;
            // currentHealth = newValue * healthPercent;
            
            // Option 3: Fully heal on max health upgrade (uncomment if preferred)
            // currentHealth = newValue;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!canTakeDamage || invulnerable || amount <= 0f)
        {
            Debug.Log($"[Health] {gameObject.name} - Damage blocked: canTakeDamage={canTakeDamage}, invulnerable={invulnerable}, amount={amount}");
            return;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Debug.Log($"[Health] {gameObject.name} took {amount} damage. Health: {previousHealth} -> {currentHealth}");
        OnDamaged?.Invoke(amount);

        if (currentHealth <= 0f)
        {
            Debug.Log($"[Health] {gameObject.name} health reached 0. Calling Die()");
            Die();
            return;
        }

        if (invulnSecondsAfterHit > 0f)
            StartCoroutine(TempInvuln(invulnSecondsAfterHit));
        
        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) return;
        float before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealed?.Invoke(currentHealth - before);
    }

    void Die()
    {
        if (hasDied) // ensure single fire
        {
            Debug.Log($"[Health] {gameObject.name} - Die() called but already died");
            return; // ensure single fire
        }
        hasDied = true;

        Debug.Log($"[Health] {gameObject.name} - Die() executing. Invoking OnDeath event...");
        OnDeath?.Invoke();

        if (destroyOnDeath)
        {
            Debug.Log($"[Health] {gameObject.name} - Destroying gameObject");
            Destroy(gameObject);  // okay to destroy after we invoke
        }
    }

    // 👇 This must be OUTSIDE Die()
    IEnumerator TempInvuln(float secs)
    {
        invulnerable = true;
        yield return new WaitForSeconds(secs);
        invulnerable = false;
    }
}
