using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);


    }

    public void Menu()
    {
        Debug.Log("Main menu button click");
        SceneManager.LoadSceneAsync(0);
    }
    public void QuitGame()
    {
        Debug.Log("Game is quitting..."); // Only visible in editor
        Application.Quit();
    }
}
