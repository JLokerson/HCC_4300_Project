using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class EnemyContactDamage : MonoBehaviour
{
    [Header("Contact Damage")]
    public float damage = 10f;
    public float damageCooldown = 0.5f; // time between hits to the same target

    // Track per-target cooldowns so an enemy doesn't melt the player every physics frame
    private readonly Dictionary<Health, float> lastHitTime = new Dictionary<Health, float>();

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // default suggestion
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Damage Health component on the player (can be on player root or child)
        Health h = other.GetComponentInParent<Health>();
        if (h == null || h.currentHealth <= 0f) return;

        float now = Time.time;
        float lastTime;
        lastHitTime.TryGetValue(h, out lastTime);
        if (now - lastTime < damageCooldown) return;

        h.TakeDamage(damage);
        lastHitTime[h] = now;
    }
}
