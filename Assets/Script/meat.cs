using UnityEngine;

public class meat : MonoBehaviour
{
    public GameObject collectedParticles;
    public AudioClip collectSFX;
    private int amount = 1;
    private bool collected;

    public void SetDropAmount(int quantity)
    {
        amount = Mathf.Max(1, quantity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var wallet = gameManager.instance;
        if (collected || !collision.CompareTag("Player") || wallet == null)
            return;

        collected = true;
        wallet.meatCollected += amount;
        if (collectSFX != null)
            wallet.playSFX(collectSFX, 1, transform);
        if (collectedParticles != null)
            Destroy(Instantiate(collectedParticles, transform.position, Quaternion.identity), 0.6f);
        Destroy(gameObject);
    }
}
