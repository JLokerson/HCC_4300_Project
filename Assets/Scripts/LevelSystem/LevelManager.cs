using System;
using System.Collections.Generic;
using UnityEngine;
using static LevelManager;

public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public static int currentLevel = 1;

    public GameObject player;
    public GameObject snail;

    private List<GameObject> spawnNodes = new List<GameObject>();
    private List<GameObject> playerSpawnNodes = new List<GameObject>();
    private  List<GameObject> enemySpawnNodes = new List<GameObject>();
    private List<GameObject> snailSpawnNodes = new List<GameObject>();

    public List<LevelObjectiveDefinition> potentialObjectives=new List<LevelObjectiveDefinition>();
    [HideInInspector]
    public LevelObjectiveDefinition currentObjective;

    public Event OnObjectiveCompleted = new Event();



    void Awake()
    {
        //assign a random objective for the level from the list
        if(potentialObjectives!=null && potentialObjectives.Count>0)
        {
            currentObjective=potentialObjectives[UnityEngine.Random.Range(0,potentialObjectives.Count)];
            Debug.Log("Assigned Level Objective: "+currentObjective.name);
        }
        else
        {
            Debug.LogError("No level objectives defined in LevelManager.");
        }


        //get all spawn nodes in scene
        spawnNodes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Respawn"));
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
        StartCoroutine(SpawnCycle());
    }

    //starts spawning enemies
    private System.Collections.IEnumerator SpawnCycle()
    {     
        if (!currentObjective.isCompleted)
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
                    //spawn enemy
                    GameObject enemySpawnNode = enemySpawnNodes[UnityEngine.Random.Range(0, enemySpawnNodes.Count)];
                    Vector3 EnemySpawnPositionWithOffset = enemySpawnNode.GetComponent<ValidSpawnTypes>().GetRandomSpawnPosition();
                    GameObject spawnedEnemy = Instantiate(currentObjective.enemyPrefab, EnemySpawnPositionWithOffset, Quaternion.identity);
                }
            }
            yield return new WaitForSeconds(currentObjective.spawnRate);
            StartCoroutine(SpawnCycle());//do it again until objective is complete
        }       
    }

    public void checkForCompletion() 
    { 
        if (currentObjective.enemiesDefeated >= currentObjective.enemiesToKill)
        {
            currentObjective.isCompleted = true;
            Debug.Log("Level Objective Completed!");
            OnObjectiveCompleted.Invoke();            
        }
    }
}
