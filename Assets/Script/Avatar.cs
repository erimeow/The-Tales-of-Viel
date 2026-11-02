using UnityEngine;

public class Avatar : MonoBehaviour
{
    [Header("Health Restore Settings")]
    public float healthRestore = 20f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        bool collected = false;

        // Player 1
        PlayerMovement2 player1 =
            collision.GetComponent<PlayerMovement2>();

        if (player1 != null)
        {
            player1.health = Mathf.Min(
                player1.health + healthRestore,
                player1.maxHealth
            );

            if (player1.healthBar != null)
                player1.healthBar.value = player1.health;

            collected = true;
        }

        // Player 2: Lance
        LanceMovement lance =
            collision.GetComponent<LanceMovement>();

        if (lance != null)
        {
            lance.health = Mathf.Min(
                lance.health + healthRestore,
                lance.maxHealth
            );

            if (lance.healthBar != null)
                lance.healthBar.value = lance.health;

            collected = true;
        }

        // Player 3: Archer
        PlayerMovementArcher archer =
            collision.GetComponent<PlayerMovementArcher>();

        if (archer != null)
        {
            archer.health = Mathf.Min(
                archer.health + healthRestore,
                archer.maxHealth
            );

            if (archer.healthBar != null)
                archer.healthBar.value = archer.health;

            collected = true;
        }

        // Collect only if a supported player was found
        if (collected)
        {
            if (gameManager.instance != null)
                gameManager.instance.coinCollected--;

            Destroy(gameObject);
        }
    }
}