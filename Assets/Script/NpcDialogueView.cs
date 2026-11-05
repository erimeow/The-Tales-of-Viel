using System;
using TMPro;
using UnityEngine;

internal sealed class NpcDialogueView
{
    private static readonly Color32 Rim = new(123, 162, 213, 255);
    private static readonly Color32 Moss = new(30, 46, 27, 215);
    private static readonly Color32 Parchment = new(249, 239, 213, 255);
    private readonly Canvas canvas;
    private readonly TMP_Text speech;
    private readonly TMP_Text response;
    private readonly UnityEngine.UI.Button[] buttons = new UnityEngine.UI.Button[2];
    private readonly TMP_Text[] labels = new TMP_Text[2];

    internal NpcDialogueView(Transform owner, string name, Sprite portrait, Action<int> select)
    {
        canvas = HudArt.Overlay(owner, name + " conversation", 90, true);
        var dialogue = Frame(canvas.transform, "Dialogue frame", new Vector2(.5f, 0),
            new Vector2(0, 68), new Vector2(894, 108), Moss);
        var nameplate = Frame(dialogue, "Nameplate", new Vector2(0, 1),
            new Vector2(74, 17), new Vector2(144, 34), Moss);
        HudArt.Text(nameplate, "Speaker", name, Vector2.one / 2, Vector2.zero,
            new Vector2(132, 29), 22, Parchment);

        var face = HudArt.Rect(dialogue, "Portrait", new Vector2(0, .5f),
            new Vector2(74, 3), new Vector2(190, 190));
        var art = HudArt.Image(face, Color.white);
        art.sprite = portrait;
        art.preserveAspect = true;

        speech = HudArt.Text(dialogue, "NPC speech", "", new Vector2(0, .5f),
            new Vector2(505, 2), new Vector2(716, 84), 24, Parchment);
        speech.alignment = TextAlignmentOptions.MidlineLeft;
        speech.enableAutoSizing = true;
        speech.fontSizeMin = 18;
        speech.fontSizeMax = 24;

        var choices = Frame(dialogue, "Player choices", new Vector2(1, 1),
            new Vector2(-300, 47), new Vector2(600, 88), Moss);
        response = HudArt.Text(choices, "Selected response", "", new Vector2(.5f, 1),
            new Vector2(0, -23), new Vector2(567, 34), 20, HudArt.Gold);
        response.alignment = TextAlignmentOptions.MidlineLeft;

        for (int i = 0; i < buttons.Length; i++)
        {
            var row = HudArt.Rect(choices, "Response " + (i + 1), new Vector2(.5f, 1),
                new Vector2(0, -23 - i * 40), new Vector2(584, 36));
            var fill = HudArt.Image(row, Color.white);
            fill.raycastTarget = true;
            var button = row.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = fill;
            var colors = button.colors;
            colors.normalColor = new Color32(49, 65, 48, 255);
            colors.highlightedColor = new Color32(75, 92, 65, 255);
            colors.selectedColor = new Color32(75, 92, 65, 255);
            colors.pressedColor = new Color32(117, 114, 63, 255);
            button.colors = colors;
            int index = i;
            button.onClick.AddListener(() => select(index));
            buttons[i] = button;
            var label = HudArt.Text(row, "Response text", "", Vector2.one / 2, Vector2.zero,
                new Vector2(566, 35), 20, Parchment);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            labels[i] = label;
        }

        Close();
    }

    internal void Show(string line, string[] replies, string selection = null)
    {
        canvas.gameObject.SetActive(true);
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null &&
            eventSystem.currentSelectedGameObject.transform.IsChildOf(canvas.transform))
            eventSystem.SetSelectedGameObject(null);

        speech.text = line;
        response.gameObject.SetActive(selection != null);
        response.text = selection == null ? "" : "You: " + selection;
        for (int i = 0; i < buttons.Length; i++)
        {
            var rect = (RectTransform)buttons[i].transform;
            rect.anchoredPosition = new Vector2(0, -23 - (i + (selection != null ? 1 : 0)) * 40);
            bool visible = i < replies.Length;
            buttons[i].gameObject.SetActive(visible);
            if (visible)
                labels[i].text = replies[i];
        }
    }

    internal void Close() => canvas.gameObject.SetActive(false);

    private static RectTransform Frame(Transform parent, string name, Vector2 anchor,
        Vector2 position, Vector2 size, Color fill)
    {
        var frame = HudArt.Rect(parent, name, anchor, position, size);
        HudArt.Image(frame, fill);
        HudArt.Image(HudArt.Rect(frame, "Top border", new Vector2(.5f, 1),
            Vector2.down, new Vector2(size.x, 2)), Rim);
        HudArt.Image(HudArt.Rect(frame, "Bottom border", new Vector2(.5f, 0),
            Vector2.up, new Vector2(size.x, 2)), Rim);
        HudArt.Image(HudArt.Rect(frame, "Left border", new Vector2(0, .5f),
            Vector2.right, new Vector2(2, size.y)), Rim);
        HudArt.Image(HudArt.Rect(frame, "Right border", new Vector2(1, .5f),
            Vector2.left, new Vector2(2, size.y)), Rim);
        return frame;
    }
}

