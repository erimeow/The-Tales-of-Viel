
using TMPro;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public TextMeshProUGUI meatText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI attkText;

    public int meatCollected = 0;
    public int coinCollected = 0;
    public int attkCollected = 0;

    // Sound
    public GameObject SFXinstance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            instance.meatText = meatText;
            instance.coinText = coinText;
            instance.attkText = attkText;

            Destroy(gameObject);
        }
    }

    public void ResetGame()
    {
        meatCollected = 0;
        coinCollected = 0;
        attkCollected = 0;

        UpdateUI();

        Debug.Log("Game Manager reset! All collected items are now 0.");
    }

    public void playSFX(AudioClip clip, int volume, Transform t)
    {
        AudioSource aud = Instantiate(
            SFXinstance, t.position, Quaternion.identity
        ).GetComponent<AudioSource>();

        aud.clip = clip;
        aud.volume = volume;
        aud.Play();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (meatText != null)
            meatText.text = meatCollected.ToString();

        if (coinText != null)
            coinText.text = coinCollected.ToString();

        if (attkText != null)
            attkText.text = attkCollected.ToString();
    }
}
