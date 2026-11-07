using UnityEngine;

internal sealed class EnemyAppearance
{
    private readonly Animator visualAnimator;

    public EnemyAppearance(Transform actor, GameObject prefab, bool armored)
    {
        if (prefab == null)
            return;

        SpriteRenderer[] originals = actor.GetComponentsInChildren<SpriteRenderer>(true);
        var visual = Object.Instantiate(prefab, actor, false);
        visual.name = "Skeleton art";
        visual.transform.localPosition = new Vector3(0f, .05f, 0f);
        visualAnimator = visual.GetComponentInChildren<Animator>();
        if (visualAnimator == null)
        {
            Object.Destroy(visual);
            return;
        }

        // The original actor still owns all animation damage events.
        visualAnimator.fireEvents = false;
        foreach (var sprite in originals)
            sprite.forceRenderingOff = true;
        if (armored)
            foreach (var sprite in visual.GetComponentsInChildren<SpriteRenderer>())
                sprite.color = new Color32(183, 177, 198, 255);
    }

    public void BeginDeath()
    {
        if (visualAnimator == null)
            return;

        visualAnimator.SetBool("IsAttacking", false);
        visualAnimator.SetBool("IsRunning", false);
        DeathPresentation.Play(visualAnimator, "Skeleton_Dying");
    }

    public void Show(bool running, bool attacking, bool dead)
    {
        if (visualAnimator == null)
            return;

        visualAnimator.SetBool("IsRunning", running);
        visualAnimator.SetBool("IsAttacking", attacking);
        visualAnimator.SetBool("IsDying", dead);
    }
}
