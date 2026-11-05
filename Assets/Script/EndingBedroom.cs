using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class EndingBedroom : MonoBehaviour
{
    [SerializeField] private SpriteRenderer roomArt;
    [SerializeField] private QtsData wakeUpDialogue;
    private static readonly Vector2 Middle = new(.5f, .5f);
    private static readonly Vector2 Pillow = new(.874f, .37f);
    private static readonly Vector2 BesideBed = new(.716f, .34f);
    private static readonly Rect Floor = new(.072f, .115f, .852f, .571f);
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
    private Image fade;
    private bool canWalk;

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

        var cover = HudArt.Rect(canvas.transform, "Wake-up fade", Middle, Vector2.zero, Vector2.zero);
        cover.anchorMin = Vector2.zero;
        cover.anchorMax = Vector2.one;
        cover.offsetMin = cover.offsetMax = Vector2.zero;
        fade = HudArt.Image(cover, Color.black);
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
        if (direction.x != 0)
            avatar.flipX = direction.x < 0;
        avatar.sprite = direction == Vector2.zero ? pajamas[0] : pajamas[1 + Mathf.FloorToInt(Time.unscaledTime * 8f) % 2];
    }

    private static bool CanWalkTo(Vector2 position)
    {
        if (!Floor.Contains(position))
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
