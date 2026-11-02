
using UnityEngine;

public class Boundery : MonoBehaviour
{
    [Header("Death Settings")]
    public RuntimeAnimatorController deathController;
    public float destroyDelay = 1.5f;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public float gameOverDelay = 2f;

    private bool gameOver = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameOver)
            return;

        if (other.CompareTag("Player"))
        {
            gameOver = true;

            Animator anim = other.GetComponent<Animator>();
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            if (anim != null && deathController != null)
            {
                anim.runtimeAnimatorController = deathController;
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }

            // Destroy player after death animation
            Destroy(other.gameObject, destroyDelay);

            // Show Game Over panel after its own delay
            Invoke(nameof(ShowGameOver), gameOverDelay);
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game Over Panel is not assigned!");
        }
    }
    
}
