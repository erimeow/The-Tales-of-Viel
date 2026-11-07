using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalDungeon : MonoBehaviour
{
    [SerializeField] private Skeleton boss;
    [SerializeField] private GameObject alden;
    [SerializeField] private QuestionManager aldenDialogue;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Slider playerHealthBar;
    private bool started;
    private bool leaving;

    private void Awake()
    {
        if (boss != null)
            boss.gameObject.SetActive(false);
        if (alden != null)
            alden.SetActive(false);
    }

    private void OnEnable()
    {
        if (boss != null)
            boss.Defeated += BossDefeated;
        if (aldenDialogue != null)
            aldenDialogue.Completed += DialogueCompleted;
    }

    private void OnDisable()
    {
        if (boss != null)
            boss.Defeated -= BossDefeated;
        if (aldenDialogue != null)
            aldenDialogue.Completed -= DialogueCompleted;
    }

    private void Start()
    {
        var player = PlayerWeaponLoadout.Active;
        if (player == null)
            return;
        if (playerHealthBar == null)
        {
            var board = GameObject.Find("Game board (1)");
            if (board != null)
                playerHealthBar = board.GetComponentInChildren<Slider>(true);
        }
        if (player.TryGetComponent<PlayerMovement2>(out var warrior))
        {
            warrior.gameOverPanel = gameOverPanel;
            warrior.healthBar = playerHealthBar;
            SyncHealth(warrior.health, warrior.maxHealth);
        }
        else if (player.TryGetComponent<PlayerMovementArcher>(out var archer))
        {
            archer.gameOverPanel = gameOverPanel;
            archer.healthBar = playerHealthBar;
            SyncHealth(archer.health, archer.maxHealth);
        }
        else if (player.TryGetComponent<LanceMovement>(out var spear))
        {
            spear.gameOverPanel = gameOverPanel;
            spear.healthBar = playerHealthBar;
            SyncHealth(spear.health, spear.maxHealth);
        }
    }

    private void SyncHealth(float current, float maximum)
    {
        if (playerHealthBar == null)
            return;
        playerHealthBar.maxValue = maximum;
        playerHealthBar.value = current;
        PlayerHealthHud.Attach(playerHealthBar);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (started || !other.CompareTag("Player"))
            return;
        started = true;
        if (boss != null)
            boss.gameObject.SetActive(true);
    }

    private void BossDefeated()
    {
        if (alden == null)
            return;
        var player = PlayerWeaponLoadout.Active;
        if (player != null)
            alden.transform.position = player.Body.position + new Vector2(1.5f, 0.4f);
        alden.SetActive(true);
    }

    private void DialogueCompleted()
    {
        if (leaving)
            return;
        int bedroom = SceneManager.GetSceneByName("Bedroom").buildIndex;
        if (bedroom < 0)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                if (System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == "Bedroom")
                    bedroom = i;
        }
        if (bedroom < 0)
        {
            Debug.LogError("Bedroom is missing from Build Settings.", this);
            return;
        }
        leaving = true;
        var player = PlayerWeaponLoadout.Active;
        if (player != null)
            gameManager.instance?.CarryHealth(bedroom, player.CurrentHealth);
        CheckpointFade.Load(bedroom);
    }
}
