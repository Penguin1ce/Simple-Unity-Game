using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("Target")]
    public PlayerHealth playerHealth;

    [Header("UI")]
    public Image fillImage;
    public Text hpText;

    [Header("Colors")]
    public Color green = new Color(0.3f, 0.8f, 0.3f);
    public Color yellow = new Color(0.9f, 0.85f, 0.2f);
    public Color red = new Color(0.85f, 0.15f, 0.15f);

    private float _displayedRatio = 1f;

    private void Start()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.HealthChanged += OnHealthChanged;
        SetBar(playerHealth.CurrentHealth / playerHealth.MaxHealth);
    }

    private void OnHealthChanged(float current, float max)
    {
        SetBar(current / max);
    }

    private void Update()
    {
        if (fillImage == null) return;

        float currentRatio = fillImage.rectTransform.anchorMax.x;
        float newRatio = Mathf.Lerp(currentRatio, _displayedRatio, 0.1f);
        fillImage.rectTransform.anchorMax = new Vector2(newRatio, 1f);
        fillImage.color = Color.Lerp(red, Color.Lerp(yellow, green, newRatio * 2f - 0.5f), Mathf.Clamp01(newRatio * 3f));
    }

    private void SetBar(float ratio)
    {
        _displayedRatio = Mathf.Clamp01(ratio);

        if (hpText != null && playerHealth != null)
            hpText.text = string.Format("{0} / {1}",
                Mathf.CeilToInt(playerHealth.CurrentHealth),
                Mathf.CeilToInt(playerHealth.MaxHealth));
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= OnHealthChanged;
    }
}
