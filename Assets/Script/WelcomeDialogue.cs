using UnityEngine;

public class WelcomeDialogue : MonoBehaviour
{
    public GameObject welcomePanel;
    public QuestionManager questionManager;
    public GameObject playerManager;
    [SerializeField] private SpriteRenderer speaker;
    [SerializeField] private int journalPage;
    [SerializeField] private Font journalFont;
    [SerializeField] private Sprite journalPaper;
    [SerializeField] private Skeleton journalBoss;
    [SerializeField] private EnemySpawner journalBossSpawner;

    private TriviaJournalView journal;
    private bool journalContinued;
    internal bool BlocksStage => journalPage > 1 && !journalContinued ||
        journalPage == 1 && activeDialogue == this;

    private static WelcomeDialogue activeDialogue;
    public static bool IsDialogueActive => activeDialogue != null && activeDialogue.isActiveAndEnabled;

    internal static bool BlocksInput(Rigidbody2D body) => IsDialogueActive &&
        (activeDialogue.speaker != null || activeDialogue.journalPage > 0) &&
        activeDialogue.playerRb == body;
    private Rigidbody2D playerRb;
    private RigidbodyConstraints2D originalConstraints;
    private bool hasTriggered = false;

    private void Awake()
    {
        if (welcomePanel != null)
            welcomePanel.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (journalPage > 1 && !hasTriggered)
            OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (speaker != null && collision.attachedRigidbody == playerRb &&
            activeDialogue != this && questionManager != null && questionManager.HasCompleted &&
            !questionManager.IsDialogueOpen)
            hasTriggered = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || hasTriggered || IsDialogueActive)
            return;

        if (journalPage > 0 && (!isActiveAndEnabled || collision.attachedRigidbody == null ||
            (journalBoss != null && journalBoss.health > 0) ||
            (journalBossSpawner != null && !journalBossSpawner.SpawnedEnemyDefeated)))
            return;

        hasTriggered = true;

        if (playerManager != null)
        {
            playerRb = playerManager
                .GetComponentInChildren<Rigidbody2D>();
        }

        if (speaker != null || journalPage > 0)
            playerRb = collision.attachedRigidbody;

        PausePlayer();

        if (journalPage > 0)
        {
            var trigger = GetComponent<Collider2D>();
            journal = new TriviaJournalView(transform, trigger.bounds.center,
                questionManager.qtsData.questions[0].questionText, journalPage, journalFont, journalPaper, ContinueJournal);
            return;
        }

        if (welcomePanel != null)
            welcomePanel.SetActive(true);

        if (questionManager != null)
        {
            questionManager.welcomeDialogue = this;
            questionManager.PrepareConversation(speaker);
            questionManager.OpenDialogue();
        }
        else
        {
            Debug.LogWarning("QuestionManager is not assigned!");
            ResumePlayer();
        }
    }

    private void ContinueJournal()
    {
        if (journalContinued)
            return;
        journalContinued = true;
        journal.Close();
        ResumePlayer();

        if (journalPage > 1)
        {
            foreach (var exit in FindObjectsByType<SceneController>(FindObjectsSortMode.None))
            {
                if (exit.gameObject.scene == gameObject.scene)
                    exit.RefreshCheckpoint();
            }
        }
    }

    public void ResumePlayer()
    {
        if (activeDialogue == this)
            activeDialogue = null;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.constraints = originalConstraints;
        }

        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        if (speaker != null && questionManager != null && questionManager.HasCompleted &&
            !questionManager.IsDialogueOpen)
            hasTriggered = false;

        Debug.Log("Player movement resumed!");
    }

    internal void PausePlayer()
    {
        if (playerRb == null)
            return;

        if (activeDialogue != this)
            originalConstraints = playerRb.constraints;
        activeDialogue = this;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.angularVelocity = 0f;
        playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void OnDisable()
    {
        journal?.Close();
        // A scene transition can unload the dialogue before its final reply.
        if (activeDialogue == this)
            ResumePlayer();
    }
}
