
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    [SerializeField] private GameObject mutantCompanion;
    [SerializeField] private Vector2[] companionOffsets;

    [SerializeField] private bool trackDefeat;
    private bool hasSpawned = false;
    internal bool SpawnedEnemyDefeated { get; private set; }

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
        var spawnedEnemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        hasSpawned = true;
        if (trackDefeat)
        {
            var enemyToTrack = spawnedEnemy.GetComponentInChildren<Skeleton>();
            if (enemyToTrack != null)
                enemyToTrack.Defeated += () => SpawnedEnemyDefeated = true;
        }
        if (mutantCompanion != null && companionOffsets != null)
        {
            foreach (Vector2 offset in companionOffsets)
            {
                var companion = Instantiate(mutantCompanion, spawnPoint.position + (Vector3)offset, spawnPoint.rotation);
                if (companion.TryGetComponent<Skeleton>(out var enemy))
                    enemy.UseMutantAppearance();
            }
        }

        Debug.Log("Enemy spawned once!");
    }
}
