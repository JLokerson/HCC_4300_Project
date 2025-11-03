using UnityEngine;

public class ValidSpawnTypes : MonoBehaviour
{
    public bool canSpawnPlayer=true;
    public bool canSpawnSnail=true;
    public bool canSpawnEnemy=true;

    public float spawnRadius=5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = new Vector3(transform.position.x + randomPoint.x, transform.position.y, transform.position.z + randomPoint.y);
        return spawnPosition;
    }
}
