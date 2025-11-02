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
        // Try to get StatManager from this GameObject
        statManager = GetComponent<StatManager>();
        
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
        if (!canTakeDamage || invulnerable || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnDamaged?.Invoke(amount);

        if (currentHealth <= 0f)
        {
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
        if (hasDied) return; // ensure single fire
        hasDied = true;

        OnDeath?.Invoke();

        if (destroyOnDeath)
        {
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
