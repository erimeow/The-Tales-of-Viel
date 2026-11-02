
using UnityEngine;

public class Trivia : MonoBehaviour
{
    public GameObject trivia;

    private void Start()
    {
        if (trivia != null)
        {
            trivia.SetActive(false);
        }
        else
        {
            Debug.LogError("Trivia Panel is not assigned!", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (trivia != null)
            {
                trivia.SetActive(true);
                Debug.Log("Player entered Trivia!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (trivia != null)
            {
                trivia.SetActive(false);
                Debug.Log("Player exited Trivia!");
            }
        }
    }
}
