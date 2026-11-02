
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
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

        Debug.Log("Quiz Finished!");
    }
}