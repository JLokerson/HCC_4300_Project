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

    [Header("Audio")]
    [SerializeField]
    private AudioClip footstepSound = null;

    public AudioClip damageSound = null;
    private AudioSource audioSource = null;

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

        // Set up audio source
        audioSource = GetComponent<AudioSource>();

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

                if(footstepSound != null && audioSource != null && agent.velocity.magnitude > 0.1f && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(footstepSound);
                }
            }
        }                  
    }   

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        audioSource.PlayOneShot(damageSound);
        if (currentHealth <= 0f && !immortal)
        {
            Destroy(gameObject);
        }
        else if (currentHealth<=0f && immortal) //temporary snail test code
        {
            StartCoroutine(StunEnemy());
        }
    }
    private System.Collections.IEnumerator StunEnemy()
    {
        Debug.Log("Snail enters shell");
        agent.isStopped = true;
        yield return new WaitForSeconds(stunDuration);
        Debug.Log("Snail exits shell");
        agent.isStopped = false;
        currentHealth = maxHealth;
    }

    public float getPiercingResistance()
    {
        return currentPiercingResistance;
    }
}
