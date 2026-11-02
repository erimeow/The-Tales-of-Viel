
using UnityEngine;
using Unity.Cinemachine;

public class PlayerManagement : MonoBehaviour
{
    [Header("Player Selection")]
    public GameObject[] playerPrefabs;

    [Header("Camera Follow")]
    public CinemachineCamera cinemachineCamera;

    private GameObject currentPlayer;
    private int characterIndex;

    private void Awake()
    {
        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        // Validate the selected character index
        if (playerPrefabs == null ||
            playerPrefabs.Length == 0)
        {
            Debug.LogError("No player prefabs assigned!");
            return;
        }

        characterIndex = Mathf.Clamp(
            characterIndex, 0, playerPrefabs.Length - 1
        );

        // Spawn the selected character
        currentPlayer = Instantiate(
            playerPrefabs[characterIndex],
            transform.position,
            Quaternion.identity
        );
    }

    private void Start()
    {
        // Assign the spawned character as the camera target
        if (currentPlayer != null && cinemachineCamera != null)
        {
            cinemachineCamera.Target.TrackingTarget =
                currentPlayer.transform;

            Debug.Log("Camera following: " + currentPlayer.name);
        }
        else
        {
            Debug.LogError(
                "Player or Cinemachine Camera is missing!"
            );
        }
    }
}