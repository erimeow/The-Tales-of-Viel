
using UnityEngine;
using UnityEngine.UI;

public class HealthbarBehaviour : MonoBehaviour
{
    [Header("Health Bar")]
    public Slider Slider;
    public Color Low = Color.red;
    public Color High = Color.green;

    [Header("Position")]
    public Vector3 Offset = new Vector3(0f, 1.2f, 0f);

    private Image fillImage;

    private void Start()
    {
        // Keep the health bar above the enemy.
        transform.localPosition = Offset;

        if (Slider != null && Slider.fillRect != null)
        {
            fillImage = Slider.fillRect.GetComponent<Image>();
        }
    }

    public void SetHealth(float health, float maxHealth)
    {
        if (Slider == null)
            return;

        Slider.maxValue = maxHealth;
        Slider.value = health;

        Slider.gameObject.SetActive(health < maxHealth);

        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(
                Low,
                High,
                Slider.normalizedValue
            );
        }
    }
}
