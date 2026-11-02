
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;

    private bool hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Stop spawning if an enemy was already spawned
        if (hasSpawned)
            return;

        if (enemyPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Assign enemy prefab and spawn point!", this);
            return;
        }

        // Spawn the enemy only once
        Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        hasSpawned = true;

        Debug.Log("Enemy spawned once!");
    }
}
