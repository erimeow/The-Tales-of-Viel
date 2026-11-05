using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class TargetHealthHud : MonoBehaviour
{
    private static TargetHealthHud bossOwner;
    private RectTransform fill;
    private Image fillImage;
    private TMP_Text value;
    private Canvas canvas;
    private Coroutine animation;
    private float displayed;
    private bool boss;

    public static TargetHealthHud Create(Transform target, float offset, string bossName)
    {
        bool isBoss = !string.IsNullOrEmpty(bossName);
        Canvas canvas;
        if (isBoss)
        {
            canvas = HudArt.Overlay(null, "Boss health HUD", 55, false);
        }
        else
        {
            var root = HudArt.Rect(target, "World health bar", Vector2.one / 2, new Vector2(0, offset), new Vector2(82, 10));
            root.localScale = Vector3.one * .013f;
            canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 45;
        }

        var hud = canvas.gameObject.AddComponent<TargetHealthHud>();
        hud.canvas = canvas;
        hud.boss = isBoss;
        Vector2 anchor = isBoss ? new Vector2(.5f, 1) : Vector2.one / 2;
        Vector2 at = isBoss ? new Vector2(0, -56) : Vector2.zero;
        var frame = HudArt.Rect(canvas.transform, "Frame", anchor, at, isBoss ? new Vector2(440, 22) : new Vector2(82, 10));
        HudArt.Image(frame, isBoss ? HudArt.Gold : new Color32(54, 34, 44, 255));
        var track = HudArt.Rect(frame, "Track", Vector2.one / 2, Vector2.zero, frame.sizeDelta - new Vector2(6, 4));
        HudArt.Image(track, HudArt.Ink);
        hud.fill = HudArt.Rect(track, "HP fill", Vector2.one / 2, Vector2.zero, Vector2.zero);
        hud.fillImage = HudArt.Image(hud.fill, HudArt.HealthColor(1));
        if (isBoss)
        {
            HudArt.Text(canvas.transform, "Boss name", bossName, anchor, new Vector2(0, -32),
                new Vector2(440, 26), 20, HudArt.Gold).fontStyle = FontStyles.Bold;
            hud.value = HudArt.Text(frame, "Boss HP", "", Vector2.one / 2, Vector2.zero,
                new Vector2(425, 20), 14, Color.white);
        }
        hud.Paint(1);
        return hud;
    }

    public void SetHealth(float hp, float max)
    {
        if (hp <= 0 || max <= 0 || (!boss && hp >= max))
        {
            canvas.enabled = false;
            if (animation != null)
            {
                StopCoroutine(animation);
                animation = null;
            }
            return;
        }

        if (boss && bossOwner != this)
        {
            if (bossOwner != null)
                bossOwner.canvas.enabled = false;
            bossOwner = this;
        }
        canvas.enabled = true;
        float fraction = Mathf.Clamp01(hp / max);
        if (!boss)
        {
            Paint(fraction);
            return;
        }

        value.text = $"{Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(max)} HP";
        if (animation != null)
            StopCoroutine(animation);
        animation = StartCoroutine(Animate(fraction));
    }

    private IEnumerator Animate(float fraction)
    {
        float from = displayed;
        for (float t = 0; t < .22f; t += Time.deltaTime)
        {
            Paint(Mathf.Lerp(from, fraction, t / .22f));
            yield return null;
        }
        Paint(fraction);
        animation = null;
    }

    private void Paint(float fraction)
    {
        displayed = fraction;
        fill.anchorMin = new Vector2(boss ? 0 : .5f - fraction / 2, 0);
        fill.anchorMax = new Vector2(boss ? fraction : .5f + fraction / 2, 1);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        fillImage.color = HudArt.HealthColor(fraction);
    }

    private void OnDestroy()
    {
        if (bossOwner == this)
            bossOwner = null;
    }
}
