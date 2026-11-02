using UnityEngine;

public class Coin : MonoBehaviour
{
    public GameObject collectedParticles;
    public AudioClip collectSFX;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameManager.instance.playSFX(collectSFX, 1, transform);
            GameObject p = Instantiate(collectedParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
            Destroy(p, .60f);
            gameManager.instance.coinCollected += 1;
        }
    }
}
