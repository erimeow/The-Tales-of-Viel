using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadSceneAsync(nextIndex);
        }
    }
}
