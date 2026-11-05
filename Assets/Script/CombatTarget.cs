using UnityEngine;

internal static class CombatTarget
{
    public static bool TryDamage(Collider2D hit, float damage, System.Collections.Generic.HashSet<Component> hitTargets = null)
    {
        var animal = hit.GetComponentInParent<Animal>();
        if (animal != null)
        {
            if (hitTargets == null || hitTargets.Add(animal))
                animal.TakeDamage(damage);
            return true;
        }

        var skeleton = hit.GetComponentInParent<Skeleton>();
        if (skeleton != null)
        {
            if (hitTargets == null || hitTargets.Add(skeleton))
                skeleton.TakeDamage(damage);
            return true;
        }

        var enemy = hit.GetComponentInParent<Enemy>();
        if (enemy == null)
            return false;

        if (hitTargets == null || hitTargets.Add(enemy))
            enemy.TakeDamage(damage);
        return true;
    }
}
