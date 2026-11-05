using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class TriviaJournalView
{
    private static readonly Vector2 Center = Vector2.one * .5f;
    private static readonly Color Brown = new Color32(62, 38, 25, 255);
    private static readonly Color Bronze = new Color32(151, 105, 55, 255);
    private readonly Canvas canvas;
    private readonly RectTransform panel;
    private readonly Vector3 triggerPosition;

    internal TriviaJournalView(Transform owner, Vector3 triggerPosition, string trivia, int page,
        Font font, Sprite texture, Action proceed)
    {
        this.triggerPosition = triggerPosition;
        canvas = HudArt.Overlay(owner, "Adventurer's Journal", 95, true);
        panel = HudArt.Rect(canvas.transform, "Journal page", Center, Vector2.zero, new Vector2(492, 230));
        HudArt.Image(panel, Brown);
        var trim = HudArt.Rect(panel, "Bronze trim", Center, Vector2.zero, new Vector2(486, 224));
        HudArt.Image(trim, Bronze);
        var paper = HudArt.Rect(panel, "Warm parchment", Center, Vector2.zero, new Vector2(480, 218));
        HudArt.Image(paper, new Color32(238, 213, 164, 255)).raycastTarget = true;
        var grain = HudArt.Image(HudArt.Rect(paper, "Paper grain", Center, Vector2.zero, paper.sizeDelta),
            new Color32(232, 198, 141, 62));
        grain.sprite = texture;

        Label(paper, "Field notes", $"ADVENTURER'S JOURNAL  /  FIELD NOTE {page:00}", 82, 442, 22, 14, font);
        Label(paper, "Title", "DID YOU KNOW?", 52, 442, 32, 26, font);
        HudArt.Image(HudArt.Rect(paper, "Divider", Center, new Vector2(0, 32), new Vector2(424, 2)), Bronze);

        var body = Label(paper, "Trivia text", trivia, -9, 424, 76, 21, font);
        body.resizeTextForBestFit = true;
        body.resizeTextMinSize = 16;
        body.resizeTextMaxSize = 21;

        var action = HudArt.Rect(paper, "Continue", Center, new Vector2(0, -77), new Vector2(154, 38));
        var background = HudArt.Image(action, new Color32(132, 84, 42, 255));
        background.raycastTarget = true;
        var button = action.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = background;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(255, 224, 173, 255);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color32(176, 156, 132, 255);
        button.colors = colors;
        Label(action, "Continue label", "Continue", 0, 150, 34, 21, font).color =
            new Color32(255, 234, 190, 255);
        button.onClick.AddListener(() =>
        {
            button.interactable = false;
            proceed();
        });
        Canvas.willRenderCanvases += PositionPanel;
        PositionPanel();
    }

    private static UnityEngine.UI.Text Label(Transform parent, string name, string value,
        float y, float width, float height, int size, Font font)
    {
        var rect = HudArt.Rect(parent, name, Center, new Vector2(0, y), new Vector2(width, height));
        var label = rect.gameObject.AddComponent<UnityEngine.UI.Text>();
        label.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = value;
        label.fontSize = size;
        label.color = Brown;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
        return label;
    }

    private void PositionPanel()
    {
        var camera = Camera.main;
        if (camera == null || canvas == null)
            return;

        var root = (RectTransform)canvas.transform;
        Vector3 screen = camera.WorldToScreenPoint(triggerPosition);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out Vector2 anchor))
            return;

        Vector2 half = panel.sizeDelta * .5f;
        Rect bounds = root.rect;
        float above = anchor.y + half.y + 22;
        float below = anchor.y - half.y - 22;
        float y = above + half.y <= bounds.yMax - 12 ? above : below;
        panel.anchoredPosition = new Vector2(
            Mathf.Clamp(anchor.x, bounds.xMin + half.x + 12, bounds.xMax - half.x - 12),
            Mathf.Clamp(y, bounds.yMin + half.y + 12, bounds.yMax - half.y - 12));
    }

    internal void Close()
    {
        Canvas.willRenderCanvases -= PositionPanel;
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }
}

