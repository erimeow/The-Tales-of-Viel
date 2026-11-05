using System;
using System.Linq;
using UnityEngine;

internal sealed class DragonAppearance
{
    private readonly SpriteRenderer renderer;
    private readonly Sprite[][] poses;
    private int previousPose = -1;
    private float poseStart;

    public DragonAppearance(Transform actor, string folder = "Emberwing", string visualName = "Emberwing art", float height = .35f)
    {
        string[] names = { "Idle", "Fly", "Attack", "Die" };
        var sprites = Resources.LoadAll<Sprite>(folder);
        poses = names.Select(name => sprites.Where(sprite => sprite.name.StartsWith(name, StringComparison.Ordinal))
            .OrderBy(sprite => sprite.name).ToArray()).ToArray();
        if (poses.Any(pose => pose.Length == 0))
            return;

        var originals = actor.GetComponentsInChildren<SpriteRenderer>(true);
        var visual = new GameObject(visualName);
        visual.transform.SetParent(actor, false);
        visual.transform.localPosition = new Vector3(0, height, 0);
        renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 10;
        renderer.sprite = poses[0][0];
        foreach (SpriteRenderer original in originals)
            original.forceRenderingOff = true;
    }

    public void Show(bool running, bool attacking, bool dead)
    {
        if (renderer == null)
            return;

        int pose = dead ? 3 : attacking ? 2 : running ? 1 : 0;
        if (pose != previousPose)
        {
            poseStart = Time.time;
            previousPose = pose;
        }

        Sprite[] frames = poses[pose];
        int index = Mathf.FloorToInt((Time.time - poseStart) * (dead ? 4f : 7f));
        renderer.sprite = frames[dead ? Mathf.Min(index, frames.Length - 1) : index % frames.Length];
    }
}
