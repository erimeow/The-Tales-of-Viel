using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer checkpointFlag;
    [SerializeField] private bool showLockedFlag;
    [SerializeField] private bool requireTrivia = true;
    [SerializeField] private EnemySpawner checkpointBossSpawner;
    private bool loading;

    private void Start() => RefreshCheckpoint();

    internal void RefreshCheckpoint()
    {
        if (checkpointFlag != null)
            checkpointFlag.enabled = showLockedFlag || !StageBlocked();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            TryAdvance(collision.attachedRigidbody);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Recheck an occupied flag if its boss/trivia gate has just unlocked.
        if (checkpointFlag != null && !loading)
            OnTriggerEnter2D(collision);
    }

    private bool StageBlocked()
    {
        if (checkpointBossSpawner != null && !checkpointBossSpawner.SpawnedEnemyDefeated)
            return true;
        if (!requireTrivia)
            return false;

        foreach (var dialogue in FindObjectsByType<WelcomeDialogue>(FindObjectsSortMode.None))
        {
            if (dialogue.gameObject.scene == gameObject.scene && dialogue.BlocksStage)
                return true;
        }

        return false;
    }

    private void TryAdvance(Rigidbody2D body)
    {
        if (loading || !isActiveAndEnabled || StageBlocked())
            return;

        int nextIndex = gameObject.scene.buildIndex + 1;
        if (nextIndex <= 0 || nextIndex >= SceneManager.sceneCountInBuildSettings)
            return;

        var player = PlayerWeaponLoadout.Active;
        if (player != null && player.Body == body)
        {
            if (player.IsDead)
                return;
            gameManager.instance?.CarryHealth(nextIndex, player.CurrentHealth);
        }

        loading = true;
        if (checkpointFlag == null)
        {
            SceneManager.LoadSceneAsync(nextIndex);
            return;
        }

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        CheckpointFade.Load(nextIndex);
    }
}
