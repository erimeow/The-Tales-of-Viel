using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal sealed class DragonAppearance
{
    private static readonly Dictionary<string, Sprite[][]> CachedPoses = new();
    private static readonly string[] PoseNames = { "Idle", "Fly", "Attack", "Die" };
    private readonly SpriteRenderer renderer;
    private readonly Sprite[][] poses;
    private int previousPose = -1;
    private float poseStart;

    internal static IEnumerator Preload(string folder, int idle, int fly, int attack, int die)
    {
        if (CachedPoses.ContainsKey(folder))
            yield break;
        int[] counts = { idle, fly, attack, die };
        var loaded = new Sprite[4][];
        for (int pose = 0; pose < loaded.Length; pose++)
        {
            loaded[pose] = new Sprite[counts[pose]];
            for (int frame = 0; frame < loaded[pose].Length; frame++)
            {
                var request = Resources.LoadAsync<Sprite>($"{folder}/{PoseNames[pose]}{frame}");
                yield return request;
                loaded[pose][frame] = request.asset as Sprite;
                if (loaded[pose][frame] == null)
                    yield break;
            }
        }
        CachedPoses[folder] = loaded;
    }
    public DragonAppearance(Transform actor, string folder = "Emberwing", string visualName = "Emberwing art", float height = .35f)
    {
        if (!CachedPoses.TryGetValue(folder, out poses))
        {
            var sprites = Resources.LoadAll<Sprite>(folder);
            poses = PoseNames.Select(name => sprites.Where(sprite => sprite.name.StartsWith(name, StringComparison.Ordinal))
                .OrderBy(sprite => sprite.name).ToArray()).ToArray();
            CachedPoses[folder] = poses;
        }
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
