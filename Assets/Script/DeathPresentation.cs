using System.Collections;
using UnityEngine;

internal static class DeathPresentation
{
    public const float PlayerDelay = .8f;

    public static void Play(Animator animator, params string[] states)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        animator.SetBool("IsDying", true);
        foreach (string state in states)
        {
            int hash = Animator.StringToHash("Base Layer." + state);
            if (!animator.HasState(0, hash))
                continue;

            animator.Play(hash, 0, 0f);
            animator.Update(0f);
            break;
        }
    }

    public static void Player(MonoBehaviour owner, Rigidbody2D body, Animator animator,
        string state, GameObject panel, AudioClip sound)
    {
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        if (animator != null)
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        Play(animator, state);

        var wallet = gameManager.instance;
        if (wallet != null)
        {
            if (sound != null)
                wallet.playSFX(sound, 1, owner.transform);
            wallet.ResetGame();
        }

        owner.StartCoroutine(Finish(owner.gameObject, panel));
    }

    private static IEnumerator Finish(GameObject actor, GameObject panel)
    {
        yield return new WaitForSecondsRealtime(PlayerDelay);
        if (panel != null)
            panel.SetActive(true);
        Object.Destroy(actor);
    }
}
