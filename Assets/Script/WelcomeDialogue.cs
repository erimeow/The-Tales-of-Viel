using UnityEngine;

public class WelcomeDialogue : MonoBehaviour
{
    public GameObject welcomePanel;
    public QuestionManager questionManager;
    public GameObject playerManager;

    public static bool IsDialogueActive = false;

    private Rigidbody2D playerRb;
    private RigidbodyConstraints2D originalConstraints;
    private bool hasTriggered = false;

    private void Awake()
    {
        if (welcomePanel != null)
            welcomePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || hasTriggered)
            return;

        hasTriggered = true;
        IsDialogueActive = true;

        if (playerManager != null)
        {
            playerRb = playerManager
                .GetComponentInChildren<Rigidbody2D>();
        }

        if (playerRb != null)
        {
            originalConstraints = playerRb.constraints;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (welcomePanel != null)
            welcomePanel.SetActive(true);

        if (questionManager != null)
        {
            questionManager.welcomeDialogue = this;
            questionManager.OpenDialogue();
        }
        else
        {
            Debug.LogWarning("QuestionManager is not assigned!");
            ResumePlayer();
        }
    }

    public void ResumePlayer()
    {
        IsDialogueActive = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.constraints = originalConstraints;
        }

        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        Debug.Log("Player movement resumed!");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Keep the Player locked until the quiz is completed.
    }
}