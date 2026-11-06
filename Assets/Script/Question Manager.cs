
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    internal const string AldenTutorialKey = "Viel.Npc.AldenPracticalTutorialComplete";
    private const string AldenProgressKey = "Viel.Npc.AldenPracticalTutorialStep";

    [Header("Question UI")]
    public Text questionText;
    public GameObject questionPanel;
    public GameObject repliesPanel;

    [Header("Reply Buttons")]
    public Button[] replyButtons;

    [Header("Question Data")]
    public QtsData qtsData;

    [Header("Typing Effect")]
    public float typingSpeed = 0.02f;

    private int currentQuestion = 0;
    private Coroutine typingCoroutine;
    private Coroutine enableButtonsCoroutine;
    private Coroutine nextCoroutine;

    private bool isProcessingAnswer = false;
    private bool dialogueStarted = false;

    public WelcomeDialogue welcomeDialogue;
    internal event System.Action Completed;
    private NpcDialogueView conversation;
    private string selectedReply;
    private bool offeringReview;
    private bool offeringClassTraining;
    private bool showingReminder;
    private Practice practice;
    private bool awaitingPracticeReply;
    private Vector2 lastPracticePosition;
    private float walkedDistance;
    private int walkedKeys;
    private bool attackRequested;
    private float attackObservedAt;
    private float guardTime;

    private enum Practice { None, Walk, Attack, Guard }
    private bool IsAldenTutorial => qtsData != null &&
        qtsData.completionKey == AldenTutorialKey && gameObject.scene.name == "Scene 1";
    private CharacterRole CurrentRole => PlayerWeaponLoadout.Active != null
        ? PlayerWeaponLoadout.Active.Role
        : (CharacterRole)Mathf.Clamp(PlayerPrefs.GetInt("SelectedCharacter", 0), 0, 2);
    private string AldenRoleCompletionKey => AldenTutorialKey + "." + CurrentRole;
    private string AldenRoleProgressKey => AldenProgressKey + "." + CurrentRole;
    private bool HasTrainedCurrentRole => PlayerPrefs.GetInt(AldenRoleCompletionKey, 0) == 1;
    internal bool IsDialogueOpen => dialogueStarted;
    internal bool HasCompleted => qtsData != null && !string.IsNullOrEmpty(qtsData.completionKey) &&
        PlayerPrefs.GetInt(qtsData.completionKey) == 1;

    internal void PrepareConversation(SpriteRenderer speaker)
    {
        if (speaker == null || qtsData == null || string.IsNullOrEmpty(qtsData.speakerName))
            return;

        if (welcomeDialogue.welcomePanel != null)
            welcomeDialogue.welcomePanel.SetActive(false);
        conversation ??= new NpcDialogueView(transform, qtsData.speakerName, speaker.sprite, SelectConversationReply);
    }

    private void ShowConversationTopic()
    {
        var topic = qtsData.questions[currentQuestion];
        selectedReply = null;
        conversation.Show(topic.questionText, topic.replies);
    }

    private void SelectConversationReply(int replyIndex)
    {
        if (showingReminder)
        {
            showingReminder = false;
            conversation.Close();
            EndQuestion();
            return;
        }

        if (offeringClassTraining)
        {
            offeringClassTraining = false;
            if (replyIndex == 0)
            {
                currentQuestion = Mathf.Clamp(PlayerPrefs.GetInt(AldenRoleProgressKey, 0), 0,
                    qtsData.questions.Length - 1);
                ShowConversationTopic();
            }
            else
            {
                conversation.Close();
                EndQuestion();
            }
            return;
        }

        if (offeringReview)
        {
            offeringReview = false;
            if (replyIndex == 0)
            {
                showingReminder = true;
                string reminder = "Move with WASD or the arrow keys. Right-click to attack.";
                if (CurrentRole == CharacterRole.Warrior)
                    reminder += " Hold Space to raise your shield.";
                conversation.Show(reminder,
                    new[] { "Thanks, Alden" });
            }
            else
            {
                conversation.Close();
                EndQuestion();
            }
            return;
        }

        var topic = qtsData.questions[currentQuestion];
        if (selectedReply == null && topic.followUps != null && replyIndex < topic.followUps.Length)
        {
            selectedReply = topic.replies[replyIndex];
            conversation.Show(topic.followUps[replyIndex], new[] { "Continue" }, selectedReply);
            return;
        }

        if (IsAldenTutorial)
        {
            if (awaitingPracticeReply)
            {
                awaitingPracticeReply = false;
                AdvanceAldenTopic();
            }
            else if (currentQuestion == 1)
                StartPractice(Practice.Walk, "Your turn: walk using W, A, S, and D. Try every key.");
            else if (currentQuestion == 2)
                StartPractice(Practice.Attack, "Your turn: right-click and watch your attack.");
            else if (currentQuestion == 3)
                StartPractice(Practice.Guard, "Your turn: hold Space to guard for a moment.");
            else
                AdvanceAldenTopic();
            return;
        }

        if (++currentQuestion < qtsData.questions.Length)
        {
            ShowConversationTopic();
            return;
        }

        conversation.Close();
        if (!string.IsNullOrEmpty(qtsData.completionKey))
        {
            PlayerPrefs.SetInt(qtsData.completionKey, 1);
            PlayerPrefs.Save();
        }
        EndQuestion();
    }

    private void StartPractice(Practice step, string instruction)
    {
        practice = step;
        walkedDistance = 0f;
        walkedKeys = 0;
        attackRequested = false;
        attackObservedAt = -1f;
        guardTime = 0f;
        lastPracticePosition = PlayerWeaponLoadout.Active != null
            ? PlayerWeaponLoadout.Active.Body.position : Vector2.zero;
        conversation.Show(instruction, System.Array.Empty<string>());
        welcomeDialogue.ResumePlayer();
    }

    private void Update()
    {
        if (practice == Practice.None)
            return;

        var player = PlayerWeaponLoadout.Active;
        if (player == null || player.IsDead || player.Body == null || !player.Body.simulated)
        {
            practice = Practice.None;
            conversation.Close();
            return;
        }

        if (practice == Practice.Walk)
        {
            Vector2 position = player.Body.position;
            walkedDistance += Vector2.Distance(position, lastPracticePosition);
            lastPracticePosition = position;
            int previousKeys = walkedKeys;
            if (Input.GetKey(KeyCode.W)) walkedKeys |= 1;
            if (Input.GetKey(KeyCode.A)) walkedKeys |= 2;
            if (Input.GetKey(KeyCode.S)) walkedKeys |= 4;
            if (Input.GetKey(KeyCode.D)) walkedKeys |= 8;
            if (walkedKeys != previousKeys)
                conversation.Show("Try every movement key: W " + KeyProgress(1) +
                    "  A " + KeyProgress(2) + "  S " + KeyProgress(4) +
                    "  D " + KeyProgress(8), System.Array.Empty<string>());
            if (walkedKeys == 15 && walkedDistance >= 0.25f)
                FinishPractice("Good work. You tried every movement key.");
        }
        else if (practice == Practice.Attack)
        {
            if (Input.GetMouseButtonDown(1))
                attackRequested = true;
            if (attackRequested && attackObservedAt < 0f && IsPlayingAttack(player))
                attackObservedAt = Time.time;
            if (attackObservedAt >= 0f && Time.time - attackObservedAt >= 0.4f)
                FinishPractice("A clean attack. You do not need to hurt anyone to practice.");
        }
        else if (practice == Practice.Guard)
        {
            var warrior = player.GetComponent<PlayerMovement2>();
            guardTime = warrior != null && warrior.IsGuarding ? guardTime + Time.deltaTime : 0f;
            if (guardTime >= 0.6f)
                FinishPractice("Well guarded. Your shield can protect you when danger strikes.");
        }
    }

    private string KeyProgress(int key) => (walkedKeys & key) != 0 ? "OK" : "...";

    private static bool IsPlayingAttack(PlayerWeaponLoadout player)
    {
        var animator = player.GetComponent<Animator>();
        if (animator == null || !animator.isActiveAndEnabled)
            return false;

        string stateName = player.Role switch
        {
            CharacterRole.Warrior => "Warrior_Attack1_Red",
            CharacterRole.Archer => "Archer_Shoot_Red",
            CharacterRole.Spear => "Player2 Attack 1",
            _ => ""
        };
        int stateHash = Animator.StringToHash(stateName);
        for (int layer = 0; layer < animator.layerCount; layer++)
            if (animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == stateHash)
                return true;
        return false;
    }

    private void FinishPractice(string response)
    {
        practice = Practice.None;
        awaitingPracticeReply = true;
        welcomeDialogue.PausePlayer();
        conversation.Show(response, new[] { "Continue" });
    }

    private void AdvanceAldenTopic()
    {
        currentQuestion++;
        if (currentQuestion == 3 && PlayerWeaponLoadout.Active?.Role != CharacterRole.Warrior)
            currentQuestion = 4;

        if (currentQuestion < qtsData.questions.Length)
        {
            PlayerPrefs.SetInt(AldenRoleProgressKey, currentQuestion);
            PlayerPrefs.Save();
            ShowConversationTopic();
            return;
        }

        PlayerPrefs.SetInt(AldenTutorialKey, 1);
        PlayerPrefs.SetInt(AldenRoleCompletionKey, 1);
        PlayerPrefs.DeleteKey(AldenRoleProgressKey);
        PlayerPrefs.Save();
        conversation.Close();
        EndQuestion();
        foreach (var exit in FindObjectsByType<SceneController>(FindObjectsSortMode.None))
            if (exit.gameObject.scene == gameObject.scene)
                exit.RefreshCheckpoint();
    }

    private void Awake()
    {
        // Hide the dialogue UI at the beginning.
        if (questionPanel != null)
            questionPanel.SetActive(false);

        if (repliesPanel != null)
            repliesPanel.SetActive(false);

        // Disable reply buttons until a question is loaded.
        if (replyButtons != null)
        {
            foreach (Button button in replyButtons)
            {
                if (button != null)
                    button.interactable = false;
            }
        }
    }

    private void Start()
    {
        // Do NOT call SetQuestion() here.
        // The dialogue starts only when OpenDialogue() is called.
        if (qtsData == null)
        {
            Debug.LogError(
                "QuestionManager: QtsData is not assigned!", this);
        }
        else if (qtsData.questions == null ||
                 qtsData.questions.Length == 0)
        {
            Debug.LogError(
                "QuestionManager: Questions array is empty!", this);
        }
    }

    // Called by WelcomeDialogue when the Player enters the trigger.
    public void OpenDialogue()
    {
        if (dialogueStarted)
            return;

        if (qtsData == null ||
            qtsData.questions == null ||
            qtsData.questions.Length == 0)
        {
            Debug.LogError(
                "QuestionManager: No question data available!", this);
            return;
        }

        if (conversation != null)
        {
            dialogueStarted = true;
            offeringReview = IsAldenTutorial && HasTrainedCurrentRole;
            offeringClassTraining = IsAldenTutorial && HasCompleted && !HasTrainedCurrentRole;
            if (offeringReview)
                conversation.Show("Welcome back! You have already completed the training. Need a quick reminder?",
                    new[] { "Remind me", "I'm ready to go" });
            else if (offeringClassTraining)
                conversation.Show("You're traveling as a " + CurrentRole + " now. Would you like to practice this class with me?",
                    new[] { "Teach me", "I know the basics" });
            else
            {
                currentQuestion = IsAldenTutorial
                    ? Mathf.Clamp(PlayerPrefs.GetInt(AldenRoleProgressKey, 0), 0,
                        qtsData.questions.Length - 1)
                    : 0;
                ShowConversationTopic();
            }
            return;
        }

        if (questionText == null)
        {
            Debug.LogError(
                "QuestionManager: Question Text is not assigned!", this);
            return;
        }

        if (replyButtons == null || replyButtons.Length == 0)
        {
            Debug.LogError(
                "QuestionManager: Reply Buttons are not assigned!", this);
            return;
        }

        dialogueStarted = true;
        currentQuestion = 0;

        if (questionPanel != null)
            questionPanel.SetActive(true);

        if (repliesPanel != null)
            repliesPanel.SetActive(true);

        SetQuestion(currentQuestion);
    }

    private bool SetQuestion(int questionIndex)
    {
        if (qtsData == null ||
            qtsData.questions == null ||
            questionIndex < 0 ||
            questionIndex >= qtsData.questions.Length)
        {
            Debug.LogError(
                "QuestionManager: Invalid question index or data.", this);
            return false;
        }

        var questionData = qtsData.questions[questionIndex];

        if (questionData.replies == null ||
            questionData.replies.Length == 0)
        {
            Debug.LogError(
                "QuestionManager: Replies are missing for question " +
                questionIndex, this);
            return false;
        }

        if (replyButtons == null ||
            replyButtons.Length < questionData.replies.Length)
        {
            Debug.LogError(
                "QuestionManager: Add enough reply buttons. Required: " +
                questionData.replies.Length, this);
            return false;
        }

        // Stop previous typing/button routines.
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (enableButtonsCoroutine != null)
            StopCoroutine(enableButtonsCoroutine);

        isProcessingAnswer = false;

        // Configure the reply buttons.
        for (int i = 0; i < replyButtons.Length; i++)
        {
            Button button = replyButtons[i];

            if (button == null)
            {
                Debug.LogError(
                    "QuestionManager: Reply button " + i +
                    " is not assigned!", this);
                return false;
            }

            button.onClick.RemoveAllListeners();
            button.interactable = false;

            bool hasReply = i < questionData.replies.Length;
            button.gameObject.SetActive(hasReply);

            if (!hasReply)
                continue;

            Text replyText =
                button.GetComponentInChildren<Text>(true);

            if (replyText == null)
            {
                Debug.LogError(
                    "QuestionManager: No legacy UI Text found in " +
                    button.name, button);
                return false;
            }

            replyText.text = questionData.replies[i];

            int replyIndex = i;
            button.onClick.AddListener(
                () => CheckReply(replyIndex));
        }

        if (questionPanel != null)
            questionPanel.SetActive(true);

        if (repliesPanel != null)
            repliesPanel.SetActive(true);

        string question = questionData.questionText;

        if (string.IsNullOrEmpty(question))
            question = "Question " + (questionIndex + 1);

        typingCoroutine = StartCoroutine(TypeText(question));

        enableButtonsCoroutine =
            StartCoroutine(EnableButtonsAfterTyping(question.Length));

        return true;
    }

    private IEnumerator TypeText(string text)
    {
        questionText.text = "";

        foreach (char letter in text)
        {
            questionText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    private IEnumerator EnableButtonsAfterTyping(int textLength)
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, textLength * typingSpeed));

        if (replyButtons != null)
        {
            foreach (Button button in replyButtons)
            {
                if (button != null &&
                    button.gameObject.activeInHierarchy &&
                    !isProcessingAnswer)
                {
                    button.interactable = true;
                }
            }
        }

        enableButtonsCoroutine = null;
    }

    private void CheckReply(int replyIndex)
    {
        if (isProcessingAnswer || !dialogueStarted)
            return;

        isProcessingAnswer = true;

        // Prevent repeated clicks.
        foreach (Button button in replyButtons)
        {
            if (button != null)
                button.interactable = false;
        }

        Debug.Log("Selected reply: " + replyIndex);

        nextCoroutine = StartCoroutine(Next());
    }

    private IEnumerator Next()
    {
        yield return new WaitForSeconds(1.5f);

        currentQuestion++;

        if (qtsData != null &&
            qtsData.questions != null &&
            currentQuestion < qtsData.questions.Length)
        {
            SetQuestion(currentQuestion);
        }
        else
        {
            EndQuestion();
        }

        nextCoroutine = null;
    }

    private void EndQuestion()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (enableButtonsCoroutine != null)
            StopCoroutine(enableButtonsCoroutine);

        if (replyButtons != null)
        {
            foreach (Button button in replyButtons)
            {
                if (button != null)
                {
                    button.interactable = false;
                    button.gameObject.SetActive(false);
                }
            }
        }

        if (repliesPanel != null)
            repliesPanel.SetActive(false);

        if (questionPanel != null)
            questionPanel.SetActive(false);

        dialogueStarted = false;

        // Resume Player movement after all questions
        if (welcomeDialogue != null)
            welcomeDialogue.ResumePlayer();

        Completed?.Invoke();
        Debug.Log("Quiz Finished!");
    }
}
