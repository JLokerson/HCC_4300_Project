using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "LevelObjectiveDefinition", menuName = "Game/Level Objective Data")]
public class LevelObjectiveDefinition : ScriptableObject
{
    [Tooltip("True when objective complete")]
    public bool isCompleted;

    public GameObject enemyPrefab;
    [Tooltip("Maximum number of enemies allowed to be active at once")]
    public int maxEnemies;
    [Tooltip("Number of enemies that must be killed to complete the objective")]
    public int enemiesToKill;

    [Tooltip("Time between spawns in seconds")]
    public float spawnRate;
    [Tooltip("Number of enemies to spawn per wave")]
    public float spawnsPerWave;
    [HideInInspector]
    public int enemiesDefeated;
    [HideInInspector]
    public int currentEnemies;
}
