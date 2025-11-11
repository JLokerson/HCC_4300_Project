using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class SlimeSplit : MonoBehaviour
{
    [Header("Next Stage")]
    [Tooltip("Prefab asset for the NEXT stage (drag from Project, not from Hierarchy). Leave null on final stage.")]
    public GameObject nextStagePrefab;

    [Tooltip("Children to spawn on death (usually 2).")]
    public int splitCount = 2;

    [Header("Spawn Placement")]
    [Tooltip("Random radial offset so children don't overlap.")]
    public float spawnRadius = 0.6f;

    [Tooltip("Small upward Y offset to avoid ground/sprite clipping.")]
    public float yOffset = 0.0f;

    [Tooltip("Max distance for sampling nearest NavMesh point around the intended spawn position.")]
    public float navmeshSampleMaxDistance = 0.75f;

    [Header("Scale Settings")]
    [Tooltip("Enable scaling for spawned children")]
    public bool enableScaling = true;
    
    [Tooltip("Scale factor for spawned children (0.5 = half size, 0.75 = three-quarters size)")]
    public float childScaleFactor = 0.75f;
    
    [Tooltip("If true, children inherit the current object's scale and apply the scale factor on top")]
    public bool inheritParentScale = true;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool hasSplit = false;
    private bool isQuitting = false;

    // Health polling (fallback if a killer doesn't proactively notify us)
    private System.Func<float> healthGetter;
    private float lastKnownHealth = float.PositiveInfinity;

    void Awake()
    {
        // We only cache the getter; we won't rely on EnemyCore in OnDestroy.
        var core = GetComponent<EnemyCore>();
        if (core != null)
        {
            healthGetter = core.getCurrentHealth;
            lastKnownHealth = healthGetter();
        }
        else
        {
            healthGetter = () => float.PositiveInfinity;
        }
    }

    void Update()
    {
        // Fallback path: if health already <= 0 and nobody called us yet, split here.
        if (healthGetter != null)
        {
            lastKnownHealth = healthGetter();
            if (debugLogs) Debug.Log($"[SlimeSplit] {name}: Update - hasSplit={hasSplit}, health={lastKnownHealth}, hasPrefab={nextStagePrefab != null}"); // ADD THIS
            if (!hasSplit && nextStagePrefab != null && lastKnownHealth <= 0f)
            {
                if (debugLogs) Debug.Log($"[SlimeSplit] {name}: Health <= 0 in Update -> splitting (fallback).");
                SplitNow();
            }
        }
    }

    void OnApplicationQuit() => isQuitting = true;

    void OnDestroy()
    {
        // Avoid splitting on app quit or when we already spawned children.
        // We rely on ForceSplitIfEligible() or the Update() fallback instead of OnDestroy timing.
    }

    /// <summary>
    /// Public entry point you can call *immediately after dealing damage*.
    /// If this slime just died from damage and has a next stage, it will split right now.
    /// Safe to call multiple times; it runs once.
    /// </summary>
    public void ForceSplitIfEligible()
    {
        if (debugLogs) Debug.Log($"[SlimeSplit] {name}: ForceSplitIfEligible called - isQuitting={isQuitting}, hasSplit={hasSplit}, hasPrefab={nextStagePrefab != null}, health={healthGetter?.Invoke() ?? -999}"); // ADD THIS
        if (isQuitting || hasSplit) return;
        if (nextStagePrefab == null) return;               // final stage -> no split
        if (healthGetter != null && healthGetter() > 0f) return; // not dead -> ignore
        if (debugLogs) Debug.Log($"[SlimeSplit] {name}: ForceSplitIfEligible -> splitting now.");
        SplitNow();
    }

    public void SplitNow()
    {
        if (hasSplit) return;
        hasSplit = true;

        if (debugLogs) Debug.Log($"[SlimeSplit] SPAWNING {splitCount} children from {nextStagePrefab?.name ?? "NULL PREFAB"}"); // ADD THIS

        Vector3 origin = transform.position;
        Vector3 parentScale = inheritParentScale ? transform.localScale : Vector3.one;
        Vector3 childScale = enableScaling ? (parentScale * childScaleFactor) : parentScale;

        for (int i = 0; i < Mathf.Max(0, splitCount); i++)
        {
            // Random radial offset so children don't overlap
            Vector2 rnd = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 intended = new Vector3(origin.x + rnd.x, origin.y + yOffset, origin.z + rnd.y);

            // Snap to NavMesh if available
            Vector3 spawnPos = intended;
            if (NavMesh.SamplePosition(intended, out NavMeshHit hit, navmeshSampleMaxDistance, NavMesh.AllAreas))
                spawnPos = hit.position + Vector3.up * yOffset;

            GameObject child = Instantiate(nextStagePrefab, spawnPos, Quaternion.identity);
            
            // Apply scaling to the child (only if scaling is enabled)
            child.transform.localScale = childScale;

            // Tiny outward nudge for NavMeshAgents so they don't overlap
            var agent = child.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                Vector3 outward = (spawnPos - origin);
                outward.y = 0f;
                if (outward.sqrMagnitude < 0.0001f) outward = new Vector3(0.05f, 0f, 0f);
                else outward = outward.normalized * 0.05f;

                agent.Warp(child.transform.position + outward);
                
                // Adjust NavMeshAgent radius based on scale to prevent overlapping (only if scaling is enabled)
                if (enableScaling)
                {
                    agent.radius *= childScaleFactor;
                }
            }
        }
    }
}
