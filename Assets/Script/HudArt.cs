using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class HudArt
{
    public static readonly Color Gold = new Color32(248, 210, 124, 255);
    public static readonly Color Ink = new Color32(38, 25, 32, 255);
    public static readonly Color Cream = new Color32(252, 235, 200, 255);

    private static readonly Sprite Wood = Resources.Load<Sprite>("ShopTheme/Wood");
    private static readonly Sprite ButtonSkin = Resources.Load<Sprite>("ShopTheme/Button");
    private static readonly Sprite CloseSkin = Resources.Load<Sprite>("ShopTheme/Close");
    private static readonly TMP_FontAsset Display = Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald Bold SDF");
    private static readonly TMP_FontAsset Body = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");

    public static void Typography(TMP_Text label, bool display = false)
    {
        var font = display ? Display : Body;
        if (font != null)
            label.font = font;
        if (display)
            label.characterSpacing = 1.5f;
    }

    public static void ShopPanel(Image image)
    {
        if (Wood == null)
            return;
        image.sprite = Wood;
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 3;
        image.color = new Color32(132, 103, 104, 255);
    }

    public static void ShopButton(Button button, Image image, TMP_Text label, bool close)
    {
        Sprite skin = close ? CloseSkin : ButtonSkin;
        if (skin != null)
        {
            image.sprite = skin;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 3.5f;
            image.color = Color.white;
        }
        var colors = button.colors;
        colors.highlightedColor = new Color32(255, 225, 160, 255);
        colors.pressedColor = new Color32(190, 151, 115, 255);
        colors.disabledColor = new Color32(118, 104, 112, 205);
        button.colors = colors;
        Typography(label, true);
    }

    public static Color HealthColor(float fraction) => fraction >= .6f
        ? new Color32(94, 188, 107, 255)
        : fraction >= .3f ? new Color32(232, 185, 76, 255) : new Color32(214, 79, 80, 255);

    public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 at, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = at;
        rect.sizeDelta = size;
        return rect;
    }

    public static Image Image(RectTransform rect, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static TMP_Text Text(Transform parent, string name, string text, Vector2 anchor,
        Vector2 at, Vector2 size, float fontSize, Color color)
    {
        var label = Rect(parent, name, anchor, at, size).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        Typography(label, name == "Title" || name == "Boss name");
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    public static Canvas Overlay(Transform parent, string name, int order, bool interactive)
    {
        var rect = Rect(parent, name, Vector2.one / 2, Vector2.zero, Vector2.zero);
        var canvas = rect.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        var scaler = rect.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 600);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        if (interactive)
            rect.gameObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }
}
