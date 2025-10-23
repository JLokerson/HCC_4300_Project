using UnityEngine;

public class EnemyDropXP : MonoBehaviour
{
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int orbCount = 1;
    [SerializeField] private float spawnSpread = 0.5f;

    public void DropOrbs()
    {
        if (!xpOrbPrefab) return;
        for (int i = 0; i < orbCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-spawnSpread, spawnSpread),
                0.1f,
                Random.Range(-spawnSpread, spawnSpread)
            );
            Instantiate(xpOrbPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}
