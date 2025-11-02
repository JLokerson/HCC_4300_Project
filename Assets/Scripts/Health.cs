using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
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

    void Awake()
    {
        // Initialize health if it's not set or if it's 0
        if (currentHealth <= 0f)
        {
            currentHealth = Mathf.Max(1f, maxHealth);
        }
        
        // Initialize audio components
        audioSource = GetComponent<AudioSource>();
        var characterCore = GetComponent<CharacterCore>();
        if (characterCore != null)
        {
            damageSound = characterCore.damageSound;
        }
        
        Debug.Log($"[Health] {gameObject.name} initialized with health: {currentHealth}/{maxHealth}");
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
