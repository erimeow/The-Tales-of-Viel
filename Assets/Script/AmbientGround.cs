using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

internal sealed class AmbientGround
{
    private readonly Collider2D shape;
    private readonly List<Tilemap> maps = new();
    private readonly RaycastHit2D[] hits = new RaycastHit2D[20];
    private readonly Collider2D[] overlaps = new Collider2D[20];
    private readonly ContactFilter2D filter = new ContactFilter2D { useTriggers = true };

    public AmbientGround(Collider2D shape)
    {
        this.shape = shape;
        foreach (var root in shape.gameObject.scene.GetRootGameObjects())
            maps.AddRange(root.GetComponentsInChildren<Tilemap>());
    }

    private bool HasGround(Vector2 point)
    {
        foreach (var map in maps)
        {
            var tile = map.GetTile(map.WorldToCell(point));
            if (tile != null && tile.name.IndexOf("water", StringComparison.OrdinalIgnoreCase) < 0)
                return true;
        }
        return maps.Count == 0;
    }

    private bool HasFooting(Vector2 point)
    {
        const float edge = .16f;
        return HasGround(point) && HasGround(point + Vector2.left * edge) &&
            HasGround(point + Vector2.right * edge) && HasGround(point + Vector2.up * edge) &&
            HasGround(point + Vector2.down * edge);
    }

    private bool Blocks(Collider2D other)
    {
        return other != null && other != shape &&
            (other.CompareTag("Barrier") || (!other.isTrigger &&
                !other.CompareTag("Player") && !other.CompareTag("Enemy") && !other.CompareTag("Villager")));
    }

    public bool CanStep(Vector2 position, Vector2 next)
    {
        if (!HasFooting(next))
            return false;

        Vector2 delta = next - position;
        if (delta.sqrMagnitude < .000001f)
            return true;
        int count = shape.Cast(delta.normalized, filter, hits, delta.magnitude + .06f);
        for (int i = 0; i < count; i++)
            if (Blocks(hits[i].collider))
                return false;
        return true;
    }

    public Vector2 FindHome(Vector2 position)
    {
        if (CanStand(position))
            return position;

        for (int ring = 1; ring <= 8; ring++)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI / 8;
                Vector2 point = position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (ring * .35f);
                if (CanStand(point))
                    return point;
            }
        }

        return position;
    }

    private bool CanStand(Vector2 point)
    {
        if (!HasFooting(point))
            return false;
        int count = Physics2D.OverlapCircle(point, .21f, filter, overlaps);
        for (int i = 0; i < count; i++)
            if (Blocks(overlaps[i]))
                return false;
        return true;
    }
}
