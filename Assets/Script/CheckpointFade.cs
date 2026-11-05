using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal sealed class CheckpointFade : MonoBehaviour
{
    private const float Duration = .65f;

    internal static void Load(int destination)
    {
        var canvas = HudArt.Overlay(null, "Checkpoint fade", 500, true);
        DontDestroyOnLoad(canvas.gameObject);
        var screen = HudArt.Rect(canvas.transform, "Black overlay", Vector2.one * .5f,
            Vector2.zero, Vector2.zero);
        screen.anchorMin = Vector2.zero;
        screen.anchorMax = Vector2.one;
        screen.offsetMin = screen.offsetMax = Vector2.zero;
        var image = HudArt.Image(screen, Color.clear);
        image.raycastTarget = true;
        var fade = canvas.gameObject.AddComponent<CheckpointFade>();
        fade.StartCoroutine(fade.Transition(image, destination));
    }

    private IEnumerator Transition(Image image, int destination)
    {
        yield return Fade(image, 0f, 1f);
        yield return SceneManager.LoadSceneAsync(destination);
        yield return Fade(image, 1f, 0f);
        Destroy(gameObject);
    }

    private static IEnumerator Fade(Image image, float from, float to)
    {
        for (float elapsed = 0; elapsed < Duration; elapsed += Time.unscaledDeltaTime)
        {
            image.color = new Color(0f, 0f, 0f, Mathf.SmoothStep(from, to, elapsed / Duration));
            yield return null;
        }
        image.color = new Color(0f, 0f, 0f, to);
    }
}

