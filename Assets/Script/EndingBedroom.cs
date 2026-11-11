using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class EndingBedroom : MonoBehaviour
{
    [SerializeField] private SpriteRenderer roomArt;
    [SerializeField] private QtsData wakeUpDialogue;
    [SerializeField] private Font endingFont;
    private static readonly Vector2 Middle = new(.5f, .5f);
    private static readonly Vector2 Pillow = new(.874f, .37f);
    private static readonly Vector2 BesideBed = new(.716f, .34f);
    private static readonly Rect Floor = new(.072f, .115f, .852f, .571f);
    private static readonly Rect Doorway = new(.445f, .67f, .11f, .075f);
    private static readonly Rect[] Furniture =
    {
        new(.075f, .445f, .178f, .23f),
        new(.076f, .12f, .319f, .21f),
        new(.748f, .274f, .20f, .20f),
        new(.887f, .486f, .064f, .095f)
    };

    private SpriteRenderer avatar;
    private Sprite[] pajamas;
    private Vector2 roomSize;
    private Vector2 avatarPosition;
    private Canvas canvas;
    private QuestionManager dialogue;
    private GameObject endingPanel;
    private GameObject finalePanel;
    private Text finaleHeading;
    private Text finaleBody;
    private Text finalePage;
    private Text finaleButtonLabel;
    private Image fade;
    private bool canWalk;
    private int finalePageIndex;

    private void Start()
    {
        if (roomArt == null || roomArt.sprite == null || wakeUpDialogue == null || Camera.main == null)
        {
            Debug.LogError("Bedroom ending is missing its room, dialogue, or camera.", this);
            return;
        }

        roomSize = roomArt.sprite.bounds.size;
        var camera = Camera.main;
        camera.transform.SetPositionAndRotation(new Vector3(0, 0, -10), Quaternion.identity);
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(roomSize.y / 2, roomSize.x / (2 * camera.aspect)) * 1.025f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(10, 7, 19, 255);

        int character = Mathf.Clamp(PlayerPrefs.GetInt("SelectedCharacter"), 0, 2);
        string path = $"Bedroom/{character}";
        pajamas = new[] { Resources.Load<Sprite>(path + "_Idle"), Resources.Load<Sprite>(path + "_Walk1"),
            Resources.Load<Sprite>(path + "_Walk2") };
        var sleeper = new GameObject("Player in pajamas", typeof(SpriteRenderer));
        sleeper.transform.SetParent(transform, false);
        avatar = sleeper.GetComponent<SpriteRenderer>();
        avatar.sortingOrder = 10;
        sleeper.transform.localScale = Vector3.one * 1.65f;
        avatar.sprite = Resources.Load<Sprite>(path + "_Sleep");
        SetPosition(Pillow);

        BuildDialogue();
        StartCoroutine(WakeUp());
    }

    private void BuildDialogue()
    {
        canvas = HudArt.Overlay(null, "Bedroom ending HUD", 100, true);
        var lower = new Vector2(.5f, 0);
        var panel = HudArt.Rect(canvas.transform, "Wake-up dialogue", lower, new Vector2(0, 92), new Vector2(700, 158));
        HudArt.ShopPanel(HudArt.Image(panel, HudArt.Ink));
        HudArt.Text(panel, "Title", "BACK HOME", Middle, new Vector2(0, 52), new Vector2(640, 30), 24, HudArt.Gold);

        var line = HudArt.Rect(panel, "Dialogue line", Middle, new Vector2(0, 10), new Vector2(620, 60)).gameObject.AddComponent<Text>();
        line.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        line.fontSize = 22;
        line.alignment = TextAnchor.MiddleCenter;
        line.color = HudArt.Cream;
        line.raycastTarget = false;
        var replies = HudArt.Rect(panel, "Replies", Middle, new Vector2(0, -48), new Vector2(140, 36));
        var next = AddButton(replies, "Next", Middle, Vector2.zero, new Vector2(140, 36));
        var nextText = HudArt.Rect(next.transform, "Text", Middle, Vector2.up * 2, new Vector2(130, 28)).gameObject.AddComponent<Text>();
        nextText.font = line.font;
        nextText.fontSize = 18;
        nextText.alignment = TextAnchor.MiddleCenter;
        nextText.color = Color.white;
        nextText.raycastTarget = false;

        dialogue = canvas.gameObject.AddComponent<QuestionManager>();
        dialogue.questionText = line;
        dialogue.questionPanel = panel.gameObject;
        dialogue.repliesPanel = replies.gameObject;
        dialogue.replyButtons = new[] { next };
        dialogue.qtsData = wakeUpDialogue;
        dialogue.typingSpeed = .02f;
        dialogue.Completed += ShowEnding;
        panel.gameObject.SetActive(false);

        var summary = HudArt.Rect(canvas.transform, "Ending controls", lower, new Vector2(0, 47), new Vector2(600, 76));
        HudArt.ShopPanel(HudArt.Image(summary, HudArt.Ink));
        HudArt.Text(summary, "Title", "THE END", Middle, new Vector2(-140, 12), new Vector2(230, 32), 27, HudArt.Gold);
        HudArt.Text(summary, "Controls", "WASD or arrow keys to look around", Middle, new Vector2(-140, -15), new Vector2(278, 24), 14, HudArt.Cream);
        var back = AddButton(summary, "Return to Title", Middle, new Vector2(150, 0), new Vector2(210, 44));
        var backLabel = HudArt.Text(back.transform, "Label", "Return to Title", Middle, Vector2.up * 2, new Vector2(205, 36), 20, HudArt.Cream);
        HudArt.ShopButton(back, back.GetComponent<Image>(), backLabel, true);
        back.onClick.AddListener(ReturnToTitle);
        endingPanel = summary.gameObject;
        endingPanel.SetActive(false);

        BuildFinale();

        var cover = HudArt.Rect(canvas.transform, "Wake-up fade", Middle, Vector2.zero, Vector2.zero);
        cover.anchorMin = Vector2.zero;
        cover.anchorMax = Vector2.one;
        cover.offsetMin = cover.offsetMax = Vector2.zero;
        fade = HudArt.Image(cover, Color.black);
    }

    private void BuildFinale()
    {
        var frame = HudArt.Rect(canvas.transform, "Door ending", Middle, Vector2.zero, new Vector2(744, 510));
        HudArt.Image(frame, new Color32(54, 32, 27, 255)).raycastTarget = true;
        var trim = HudArt.Rect(frame, "Bronze edge", Middle, Vector2.zero, new Vector2(736, 502));
        HudArt.Image(trim, new Color32(157, 111, 61, 255));
        var page = HudArt.Rect(trim, "Paper", Middle, Vector2.zero, new Vector2(728, 494));
        HudArt.Image(page, new Color32(237, 216, 177, 255));
        finaleHeading = FinaleText(page, "Heading", new Vector2(0, 187), new Vector2(665, 62), 32,
            new Color32(66, 42, 32, 255), TextAnchor.MiddleCenter);
        var rule = HudArt.Rect(page, "Rule", Middle, new Vector2(0, 146), new Vector2(620, 2));
        HudArt.Image(rule, new Color32(157, 111, 61, 255));
        finaleBody = FinaleText(page, "Story and credits", new Vector2(0, 10), new Vector2(640, 252), 21,
            new Color32(62, 44, 35, 255), TextAnchor.MiddleCenter);
        finaleBody.resizeTextForBestFit = true;
        finaleBody.resizeTextMinSize = 16;
        finaleBody.resizeTextMaxSize = 21;
        finalePage = FinaleText(page, "Page number", new Vector2(-270, -202), new Vector2(125, 30), 15,
            new Color32(114, 76, 49, 255), TextAnchor.MiddleLeft);
        var next = AddButton(page, "Next page", Middle, new Vector2(252, -202), new Vector2(175, 42));
        finaleButtonLabel = FinaleText(next.transform, "Button label", Vector2.zero, new Vector2(165, 36), 19,
            HudArt.Cream, TextAnchor.MiddleCenter);
        next.GetComponent<Image>().color = new Color32(101, 62, 43, 255);
        next.onClick.AddListener(AdvanceFinale);
        finalePanel = frame.gameObject;
        finalePanel.SetActive(false);
    }

    private Text FinaleText(Transform parent, string name, Vector2 at, Vector2 size, int sizeInPoints,
        Color color, TextAnchor alignment)
    {
        var label = HudArt.Rect(parent, name, Middle, at, size).gameObject.AddComponent<Text>();
        label.font = endingFont != null ? endingFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = sizeInPoints;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        return label;
    }

    private void OpenFinale()
    {
        canWalk = false;
        endingPanel.SetActive(false);
        finalePageIndex = 0;
        ShowFinalePage();
        finalePanel.SetActive(true);
    }

    private void AdvanceFinale()
    {
        if (finalePageIndex == 3)
        {
            ReturnToTitle();
            return;
        }
        finalePageIndex++;
        ShowFinalePage();
    }

    private void ShowFinalePage()
    {
        switch (finalePageIndex)
        {
            case 0:
                finaleHeading.text = "THE TALES OF VIEL";
                finaleBody.text = "Back in the bedroom, the journey through Viel feels like a dream.\n\nAlden's lessons, the villages, the battles, and the final dragon all led home.";
                break;
            case 1:
                finaleHeading.text = "FIELD NOTES";
                finaleBody.text = "Movement and timing kept the traveler alive. Enemies dropped coins; animals provided meat. Both could be saved for equipment and food at the shops.\n\nEach flag marked the way forward. Preparation mattered as much as winning a fight.";
                break;
            case 2:
                finaleHeading.text = "GROUP 1  /  DEVELOPMENT CREDITS";
                finaleBody.text = "RENIER C. LABONITE  -  Project Leader / Documentation / Developer\n" +
                    "NORLITO D. DELA CRUZ  -  UI / Design / Documentation\n" +
                    "NIEL KRISTIAN I. MAGTOTO  -  Programmer / Diagrams\n" +
                    "JOHN PAUL A. MOSCOSA  -  Audio / Documentation\n" +
                    "JOHN JOVIT JAMES Q. NARES  -  Game Level Design / Documentation\n" +
                    "MARVIN G. TRAQUIÑA  -  Quality Assurance / Documentation";
                break;
            default:
                finaleHeading.text = "THE END";
                finaleBody.text = "Thank you for playing.";
                break;
        }
        finalePage.text = $"{finalePageIndex + 1:00} / 04";
        finaleButtonLabel.text = finalePageIndex == 3 ? "MAIN MENU" : "CONTINUE";
    }

    private static Button AddButton(Transform parent, string name, Vector2 anchor, Vector2 at, Vector2 size)
    {
        var rect = HudArt.Rect(parent, name, anchor, at, size);
        var image = HudArt.Image(rect, new Color32(112, 66, 61, 255));
        image.raycastTarget = true;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private IEnumerator WakeUp()
    {
        for (float t = 0; t < .8f; t += Time.unscaledDeltaTime)
        {
            fade.color = new Color(0, 0, 0, 1f - t / .8f);
            yield return null;
        }
        fade.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(.5f);
        avatar.sprite = pajamas[0];
        SetPosition(BesideBed);
        dialogue.OpenDialogue();
    }

    private void ShowEnding()
    {
        endingPanel.SetActive(true);
        canWalk = true;
    }

    private void Update()
    {
        if (!canWalk)
            return;

        Vector2 direction = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        direction = Vector2.ClampMagnitude(direction, 1);
        Vector2 step = direction * (2.4f * Time.unscaledDeltaTime);
        Vector2 proposed = avatarPosition + new Vector2(step.x / roomSize.x, step.y / roomSize.y);
        Vector2 current = avatarPosition;
        if (CanWalkTo(new Vector2(proposed.x, current.y)))
            current.x = proposed.x;
        if (CanWalkTo(new Vector2(current.x, proposed.y)))
            current.y = proposed.y;
        SetPosition(current);
        if (Doorway.Contains(current) && current.y >= .705f)
        {
            OpenFinale();
            return;
        }
        if (direction.x != 0)
            avatar.flipX = direction.x < 0;
        avatar.sprite = direction == Vector2.zero ? pajamas[0] : pajamas[1 + Mathf.FloorToInt(Time.unscaledTime * 8f) % 2];
    }

    private static bool CanWalkTo(Vector2 position)
    {
        if (!Floor.Contains(position) && !Doorway.Contains(position))
            return false;
        foreach (Rect furniture in Furniture)
            if (furniture.Contains(position))
                return false;
        return true;
    }

    private void SetPosition(Vector2 normalized)
    {
        avatarPosition = normalized;
        avatar.transform.localPosition = Vector2.Scale(normalized - Middle, roomSize);
    }

    private static void ReturnToTitle()
    {
        gameManager.instance?.ResetGame();
        SceneManager.LoadSceneAsync(0);
    }

    private void OnDestroy()
    {
        if (dialogue != null)
            dialogue.Completed -= ShowEnding;
        if (canvas != null)
            Destroy(canvas.gameObject);
    }
}
