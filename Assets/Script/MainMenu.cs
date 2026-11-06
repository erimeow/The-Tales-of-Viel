using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMPro.TextMeshProUGUI resetLabel;

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);


    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(QuestionManager.AldenTutorialKey);
        PlayerPrefs.DeleteKey("Viel.Npc.AldenTutorialComplete");
        PlayerPrefs.DeleteKey("Viel.Npc.AldenPracticalTutorialStep");
        foreach (CharacterRole role in System.Enum.GetValues(typeof(CharacterRole)))
        {
            PlayerPrefs.DeleteKey(QuestionManager.AldenTutorialKey + "." + role);
            PlayerPrefs.DeleteKey("Viel.Npc.AldenPracticalTutorialStep." + role);
        }
        PlayerPrefs.Save();
        gameManager.instance?.ResetGame();
        if (resetLabel != null)
            resetLabel.text = "RESET DONE";
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
