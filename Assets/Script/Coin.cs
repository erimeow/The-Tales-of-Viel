using UnityEngine;

public class Coin : MonoBehaviour
{
    public GameObject collectedParticles;
    public AudioClip collectSFX;
    private bool collected;

    public void PlayDropEffect()
    {
        if (collectedParticles != null)
            Destroy(Instantiate(collectedParticles, transform.position, Quaternion.identity), 0.6f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var wallet = gameManager.instance;
        if (collected || !collision.CompareTag("Player") || wallet == null)
            return;

        collected = true;
        wallet.coinCollected++;
        if (collectSFX != null)
            wallet.playSFX(collectSFX, 1, transform);
        PlayDropEffect();
        Destroy(gameObject);
    }
}
