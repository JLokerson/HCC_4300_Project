using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public static int currentLevel = 15;

    public GameObject player;
    private static bool playerInstantiated = false; //used to make sure only one player is created
    public GameObject snail;

    private List<GameObject> spawnNodes = new List<GameObject>();
    private List<GameObject> playerSpawnNodes = new List<GameObject>();
    private List<GameObject> enemySpawnNodes = new List<GameObject>();
    private List<GameObject> snailSpawnNodes = new List<GameObject>();

    // Keep the old objective system for backward compatibility
    public List<LevelObjectiveDefinition> potentialObjectives = new List<LevelObjectiveDefinition>();
    [HideInInspector]
    public LevelObjectiveDefinition currentObjective;
    public Event OnObjectiveCompleted = new Event();

    // Add the new objective system as well
    [SerializeField] public List<Objective> objectives = new List<Objective>();

    [Serializable]
    public class Objective
    {
        [Tooltip("True when objective complete")]
        public bool isCompleted;

        public GameObject enemyPrefab;
        public int maxEnemies;

        [Tooltip("Time between spawns in seconds")]
        public float spawnRate;
        [Tooltip("Number of enemies to spawn per wave")]
        public float spawnsPerWave;
        private int enemiesDefeated;
        private int currentEnemies;

        public int EnemiesDefeated
        {
            get { return enemiesDefeated; }
            set { enemiesDefeated = value; }
        }
        public int CurrentEnemies
        {
            get { return currentEnemies; }
            set { currentEnemies = value; }
        }

        public Objective(int maxEnemies)
        {
            this.maxEnemies = maxEnemies;
            this.enemiesDefeated = 0;
            this.isCompleted = false;
        }

        private List<GameObject> spawnedEnemiesThisObjective = new List<GameObject>();
        public void AddSpawnedEnemy(GameObject enemy)
        {
            spawnedEnemiesThisObjective.Add(enemy);
        }
        public void EnemyDefeated()
        {
            enemiesDefeated++;
            currentEnemies--;
            if (enemiesDefeated >= maxEnemies)
            {
                isCompleted = true;
            }
        }
    }

    void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += levelStart;
    }
    void OnDestroy()
    {
        // important to unsubscribe to avoid stale handlers from instances that should be gone
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= levelStart;
    }

    //void waitForSceneStabilization(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    //{
    //    Debug.Log("Scene " + scene.name + " has been fully loaded.");
    //    StartCoroutine(levelInitStart());
    //}

    //private IEnumerator teleportPlayer(Vector3 pos)
    //{
    //    yield return new WaitForSeconds(1f);
    //    Debug.Log("Teleporting Player to: " + pos);
    //    player = GameObject.FindWithTag("Player");
    //    player.transform.position = pos;
    //    yield return new WaitForSeconds(5);
    //    Debug.Log("Player's actual position is"+player.transform.position);
    //}

    private void levelStart(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log("Scene fully loaded: "+scene.name);
        //assign a random objective for the level from the list (old system)
        if (potentialObjectives != null && potentialObjectives.Count > 0)
        {
            currentObjective = Instantiate(potentialObjectives[UnityEngine.Random.Range(0, potentialObjectives.Count)]);
            Debug.Log("Assigned Level Objective: " + currentObjective.name);
        }
        else if (objectives == null || objectives.Count == 0)
        {
            Debug.LogError("No level objectives defined in LevelManager in scene " + gameObject.scene.name);
        }

        //get all spawn nodes in scene
        spawnNodes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Respawn"));
        if (spawnNodes == null || spawnNodes.Count <= 0)
        {
            Debug.LogError("No spawn nodes found in the scene.");
            return;
        }
        //sort spawn nodes by valid spawn types
        foreach (GameObject node in spawnNodes)
        {
            ValidSpawnTypes validSpawnTypes = node.GetComponent<ValidSpawnTypes>();
            if (validSpawnTypes != null)
            {
                if (validSpawnTypes.canSpawnPlayer)
                {
                    playerSpawnNodes.Add(node);
                }
                if (validSpawnTypes.canSpawnSnail)
                {
                    snailSpawnNodes.Add(node);
                }
                if (validSpawnTypes.canSpawnEnemy)
                {
                    enemySpawnNodes.Add(node);
                }
            }
        }
        //spawn player at random player spawn node with the random offset within spawn radius
        if (playerSpawnNodes.Count > 0)
        {
            GameObject playerSpawnNode = playerSpawnNodes[UnityEngine.Random.Range(0, playerSpawnNodes.Count)];
            Vector3 spawnPositionWithOffset = playerSpawnNode.GetComponent<ValidSpawnTypes>().GetRandomSpawnPosition();
            if (player != null && !playerInstantiated) //creates the player if it hasn't been created yet
            {
                Debug.Log("Spawning Player at: " + spawnPositionWithOffset);
                Instantiate(player, spawnPositionWithOffset, Quaternion.identity);
                togglePlayerInstantiation(true);
            }
            else if (player != null && playerInstantiated) //teleports the player to the spawn point if they have already been created
            {
                Debug.Log("Teleporting Player to: " + spawnPositionWithOffset);

                //this whole mess with disabling/enabling the CharacterController is because it was calculating the position,
                //I was teleporting the player, then it was applying its calculated position or something
                //overriding where I just teleported it. 
                player = GameObject.FindWithTag("Player");
                Debug.Log($"Player found? {player!=null}");
                var controller=player.GetComponent<CharacterController>();
                Debug.Log($"Found controller {controller!=null}");
                controller.enabled = false;
                Debug.Log("Controller enabled: " +controller.enabled);
                player.transform.position = spawnPositionWithOffset;
                controller.enabled = true;
                Debug.Log("Controller enabled: " + controller.enabled);
                Debug.Log("Player's actual position="+player.transform.position);
            }

        }
        else
        {
            Debug.LogError("No valid player spawn nodes found.");
        }
        //spawn snail at random snail spawn node with the random offset within spawn radius
        if (snailSpawnNodes.Count > 0)
        {
            GameObject snailSpawnNode = snailSpawnNodes[UnityEngine.Random.Range(0, snailSpawnNodes.Count)];
            Vector3 spawnPositionWithOffset = snailSpawnNode.GetComponent<ValidSpawnTypes>().GetRandomSpawnPosition();
            if (snail != null)
            {
                Instantiate(snail, spawnPositionWithOffset, Quaternion.identity);
            }

        }
        else
        {
            Debug.LogError("No valid snail spawn nodes found.");
        }

        // Start spawn cycles for both objective systems
        if (currentObjective != null)
        {
            StartCoroutine(SpawnCycle());
        }

        foreach (Objective objective in objectives)
        {
            StartCoroutine(SpawnCycle(objective));
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    //starts spawning enemies (old system)
    private System.Collections.IEnumerator SpawnCycle()
    {
        if (currentObjective != null && !currentObjective.isCompleted)
        {
            for (int i = 0; i < currentObjective.spawnsPerWave; i++)
            {
                if (currentObjective.isCompleted)
                {
                    break; //exit if objective completed during wave
                }
                //check if current enemies is less than max enemies
                if (currentObjective.currentEnemies < currentObjective.maxEnemies)
                {
                    bool nearPlayer = true;
                    while (nearPlayer)
                    {
                        //check if spawn node is near player
                        GameObject enemySpawnNode = enemySpawnNodes[UnityEngine.Random.Range(0, enemySpawnNodes.Count)];
                        Vector3 EnemySpawnPositionWithOffset = enemySpawnNode.GetComponent<ValidSpawnTypes>().GetRandomSpawnPosition();
                        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                        if (playerObj != null)
                        {
                            float distanceToPlayer = Vector3.Distance(EnemySpawnPositionWithOffset, playerObj.transform.position);
                            if (distanceToPlayer > 5f) //distance check player
                            {
                                //spawn enemy
                                GameObject spawnedEnemy = Instantiate(selectWeightedEnemy(), EnemySpawnPositionWithOffset, Quaternion.identity);
                                nearPlayer = false;
                            }
                            else
                            {
                                Debug.Log("Spawned enemy too close to player, retrying...");
                            }
                        }

                    }
                }
            }
            yield return new WaitForSeconds(currentObjective.spawnRate);
            StartCoroutine(SpawnCycle());//do it again until objective is complete
        }
    }

    //starts spawning enemies (new system)
    private System.Collections.IEnumerator SpawnCycle(Objective obj)
    {
        if (!obj.isCompleted)
        {
            for (int i = 0; i < obj.spawnsPerWave; i++)
            {
                if (obj.isCompleted)
                {
                    break; //exit if objective completed during wave
                }
                //check if current enemies is less than max enemies
                if (obj.CurrentEnemies < obj.maxEnemies)
                {
                    //spawn enemy
                    GameObject enemySpawnNode = enemySpawnNodes[UnityEngine.Random.Range(0, enemySpawnNodes.Count)];
                    Vector3 EnemySpawnPositionWithOffset = enemySpawnNode.GetComponent<ValidSpawnTypes>().GetRandomSpawnPosition();
                    GameObject spawnedEnemy = Instantiate(obj.enemyPrefab, EnemySpawnPositionWithOffset, Quaternion.identity);
                    obj.AddSpawnedEnemy(spawnedEnemy);
                }
            }
            yield return new WaitForSeconds(obj.spawnRate);
            StartCoroutine(SpawnCycle(obj));//do it again until objective is complete
        }
    }

    // Keep the old completion check function for backward compatibility
    public void checkForCompletion()
    {
        if (currentObjective != null && currentObjective.enemiesDefeated >= currentObjective.enemiesToKill)
        {
            currentObjective.isCompleted = true;
            Debug.Log("Level Objective Completed!");
            OnObjectiveCompleted.Invoke();
        }
    }

    public GameObject selectWeightedEnemy()
    {
        if (currentObjective == null || currentObjective.enemyWeights == null || currentObjective.enemyWeights.Count == 0)
        {
            Debug.LogWarning("selectWeightedEnemy: no enemyWeights available on currentObjective.");
            return null;
        }

        // Compute total of non-negative weights and collect valid entries
        int totalWeight = 0;
        var validEntries = new List<EnemySpawnWeights>();
        foreach (var enemy in currentObjective.enemyWeights)
        {
            if (enemy == null || enemy.enemyPrefab == null)
                continue;
            int weight = Math.Max(0, enemy.weight);
            if (weight > 0)
            {
                totalWeight += weight;
                validEntries.Add(enemy);
            }
            else
            {
                // keep zero-weight entries too in case all weights are zero
                validEntries.Add(enemy);
            }
        }

        if (validEntries.Count == 0)
        {
            Debug.LogWarning("selectWeightedEnemy: no valid enemy prefabs found in enemyWeights.");
            return null;
        }

        // If totalWeight is zero (all weights were 0), pick uniformly
        if (totalWeight <= 0)
        {
            return validEntries[UnityEngine.Random.Range(0, validEntries.Count)].enemyPrefab;
        }

        int randomNumber = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        foreach (var enemy in currentObjective.enemyWeights)
        {
            if (enemy == null || enemy.enemyPrefab == null)
                continue;
            int weight = Math.Max(0, enemy.weight);
            cumulativeWeight += weight;
            if (randomNumber < cumulativeWeight)
                return enemy.enemyPrefab;
        }

        // Fallback: return last valid prefab
        return validEntries.Last().enemyPrefab;
    }
    public void increaseLevelCount()
    {
        currentLevel++;
    }

    public void resetLevelCount()
    {
        currentLevel = 1;
    }

    //toggles the state of the player instantiated bool
    public static void togglePlayerInstantiation(bool value)
    {
        playerInstantiated = value;
    }
}