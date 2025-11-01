using System;
using System.Collections.Generic;
using UnityEngine;
using static LevelManager;

public class LevelManager : MonoBehaviour
{
    private static int currentLevel = 1;

    public GameObject player;
    public GameObject snail;

    private List<GameObject> spawnNodes = new List<GameObject>();
    private List<GameObject> playerSpawnNodes = new List<GameObject>();
    private  List<GameObject> enemySpawnNodes = new List<GameObject>();
    private List<GameObject> snailSpawnNodes = new List<GameObject>();

    [SerializeField]public List<Objective> objectives = new List<Objective>();

    public static int CurrentLevel
    {
        get { return currentLevel; }
        set { currentLevel = value; }
    }

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
        //get all spawn nodes in scene
        spawnNodes=new List<GameObject>(GameObject.FindGameObjectsWithTag("Respawn"));
        if(spawnNodes==null || spawnNodes.Count<=0)
        {
            Debug.LogError("No spawn nodes found in the scene.");
            return;
        }
        //sort spawn nodes by valid spawn types
        foreach (GameObject node in spawnNodes)
        {
            ValidSpawnTypes validSpawnTypes=node.GetComponent<ValidSpawnTypes>();
            if(validSpawnTypes!=null)
            {
                if(validSpawnTypes.canSpawnPlayer)
                {
                    playerSpawnNodes.Add(node);
                }
                if(validSpawnTypes.canSpawnSnail)
                {
                    snailSpawnNodes.Add(node);
                }
                if(validSpawnTypes.canSpawnEnemy)
                {
                    enemySpawnNodes.Add(node);
                }
            }
        }
        //spawn player at random player spawn node with the random offset within spawn radius
        if (playerSpawnNodes.Count>0)
        {
            GameObject playerSpawnNode=playerSpawnNodes[UnityEngine.Random.Range(0,playerSpawnNodes.Count)];
            Vector3 spawnPositionWithOffset=playerSpawnNode.GetComponent<ValidSpawnTypes>().GetRandomSpawnPosition();
            Instantiate(player,spawnPositionWithOffset,Quaternion.identity);
        }
        else
        {
            Debug.LogError("No valid player spawn nodes found.");
        }
        //spawn snail at random snail spawn node with the random offset within spawn radius
        if(snailSpawnNodes.Count>0)
        {
            GameObject snailSpawnNode=snailSpawnNodes[UnityEngine.Random.Range(0,snailSpawnNodes.Count)];
            Vector3 spawnPositionWithOffset=snailSpawnNode.GetComponent<ValidSpawnTypes>().GetRandomSpawnPosition();
            Instantiate(snail,spawnPositionWithOffset,Quaternion.identity);
        }
        else
        {
            Debug.LogError("No valid snail spawn nodes found.");
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

    //starts spawning enemies
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
                    obj.CurrentEnemies++;
                    obj.AddSpawnedEnemy(spawnedEnemy);
                }
            }
            yield return new WaitForSeconds(obj.spawnRate);
            StartCoroutine(SpawnCycle(obj));//do it again until objective is complete
        }       
    }
}
