using UnityEngine;
using System.Collections.Generic;

/// Place on the Poison Puddle prefab. Uses a trigger collider to deal periodic damage.
[RequireComponent(typeof(Collider))]
public class PoisonPuddle : MonoBehaviour
{
    [Header("Poison Properties")]
    [Tooltip("Seconds the puddle persists before disappearing")]
    public float lifetime = 4f;
    [Tooltip("Damage per second dealt to the player standing in the puddle")]
    public float dps = 8f;
    [Tooltip("How often to apply damage ticks (seconds)")]
    public float tickRate = 0.25f;

    [Header("Visuals")]
    [Tooltip("Optional renderer to fade out (SpriteRenderer, MeshRenderer, etc.)")]
    public Renderer[] renderersToFade;
    [Tooltip("Scale from this to zero over its lifetime")]
    public Vector3 startScale = new Vector3(1.1f, 1f, 1.1f);

    private float spawnTime;
    private readonly Dictionary<Health, float> lastTick = new Dictionary<Health, float>();

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // We only want overlap damage
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        transform.localScale = startScale;
    }

    void Update()
    {
        // Lifetime & fade/shrink
        float t = Mathf.InverseLerp(spawnTime, spawnTime + lifetime, Time.time);

        // Shrink
        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

        // Fade
        if (renderersToFade != null)
        {
            foreach (var r in renderersToFade)
            {
                if (!r) continue;
                // Works for SpriteRenderer (via material color) and standard MeshRenderer with a color property
                if (r.material.HasProperty("_Color"))
                {
                    Color c = r.material.color;
                    c.a = 1f - t;
                    r.material.color = c;
                }
            }
        }

        // Despawn
        if (Time.time >= spawnTime + lifetime)
            Destroy(gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Damage the player's Health component (works with your Health/PlayerHealth setup)
        Health hp = other.GetComponentInParent<Health>(); // player may have Health on root or child  :contentReference[oaicite:5]{index=5}
        if (hp == null || hp.currentHealth <= 0f || hp.IsInvulnerable) return; // respect i-frames  :contentReference[oaicite:6]{index=6}

        float last;
        lastTick.TryGetValue(hp, out last);
        if (Time.time - last < tickRate) return;

        float damage = dps * tickRate;
        hp.TakeDamage(damage); // your Health system triggers PlayerHealth on death  :contentReference[oaicite:7]{index=7}
        lastTick[hp] = Time.time;
    }
}
