using UnityEngine;

public class ItemAsset : MonoBehaviour
{
    public static ItemAsset Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public Transform pfItemWorld;
    public Sprite healthPotionSprite;
   
    public Sprite coinSprite;
    public Sprite medkitSprite;
}
