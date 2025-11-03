using UnityEngine;
using UnityEngine.AI;

/// Add this ONLY to the Snail enemy prefab (not shared with other enemies)
[RequireComponent(typeof(NavMeshAgent))]
public class SnailPoisonTrail : MonoBehaviour
{
    [Header("Puddle Spawning")]
    [Tooltip("Prefab with PoisonPuddle component")]
    public GameObject poisonPuddlePrefab;
    [Tooltip("Minimum world distance moved before dropping next puddle")]
    public float minDistanceBetweenDrops = 0.75f;
    [Tooltip("Optional: also rate-limit drops by time (seconds)")]
    public float minSecondsBetweenDrops = 0.15f;
    [Tooltip("Spawn height offset to avoid z-fighting with floor")]
    public float yOffset = 0.01f;

    [Header("When to emit")]
    [Tooltip("Minimum agent speed to count as moving")]
    public float velocityThreshold = 0.05f;

    private NavMeshAgent agent;
    private EnemyCore enemyCore; // used to check IsInShell()
    private Vector3 lastDropPos;
    private float lastDropTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCore = GetComponent<EnemyCore>(); // snail has this
    }

    private void Start()
    {
        lastDropPos = transform.position;
        lastDropTime = -999f;
    }

    private void Update()
    {
        // If stunned/in shell, don't emit a trail
        if (enemyCore != null && enemyCore.IsInShell()) return; // uses EnemyCore.IsInShell()  :contentReference[oaicite:4]{index=4}

        // Only drop when actually moving
        if (agent == null || agent.velocity.magnitude < velocityThreshold) return;

        bool farEnough = Vector3.Distance(transform.position, lastDropPos) >= minDistanceBetweenDrops;
        bool longEnough = (Time.time - lastDropTime) >= minSecondsBetweenDrops;

        if (farEnough && longEnough)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
            if (poisonPuddlePrefab != null)
            {
                Instantiate(poisonPuddlePrefab, spawnPos, Quaternion.identity);
                lastDropPos = transform.position;
                lastDropTime = Time.time;
            }
        }
    }
}
