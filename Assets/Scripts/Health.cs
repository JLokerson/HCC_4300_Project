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

    private AudioClip damageSound = null;
    private AudioSource audioSource = null;

    void Awake()
    {
        currentHealth = Mathf.Max(1f, maxHealth);
        audioSource = GetComponent<AudioSource>();
        damageSound=GetComponent<CharacterCore>()?.damageSound;
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
