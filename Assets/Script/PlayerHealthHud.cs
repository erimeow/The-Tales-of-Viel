using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PlayerHealthHud : MonoBehaviour
{
    private Slider slider;
    private Image fill;
    private TMP_Text value;

    public static void Attach(Slider slider)
    {
        if (slider == null || slider.TryGetComponent<PlayerHealthHud>(out _))
            return;

        var hud = slider.gameObject.AddComponent<PlayerHealthHud>();
        hud.slider = slider;
        hud.fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        hud.value = HudArt.Text(slider.transform, "HP values", "", new Vector2(.5f, .5f), Vector2.zero,
            new Vector2(180, 32), 16, Color.white);
        var rect = hud.value.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        hud.value.enableAutoSizing = true;
        hud.value.fontSizeMin = 7;
        hud.value.fontSizeMax = 16;
        hud.value.fontStyle = FontStyles.Bold;
        hud.value.outlineWidth = .18f;
        hud.value.outlineColor = Color.black;
        slider.onValueChanged.AddListener(hud.Refresh);
        hud.Refresh(slider.value);
    }

    private void Refresh(float hp)
    {
        value.text = $"{Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(slider.maxValue)} HP";
        if (fill != null)
            fill.color = HudArt.HealthColor(slider.normalizedValue);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(Refresh);
    }
}
