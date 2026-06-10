using UnityEngine;
using UnityEngine.UI;

public class PlayerManaBar : MonoBehaviour
{
    [Header("Target")]
    public PlayerMana playerMana;

    [Header("UI")]
    public Image fillImage;
    public Text manaText;

    [Header("Colors")]
    public Color fullBlue = new Color(0.2f, 0.4f, 0.9f);
    public Color lowBlue = new Color(0.1f, 0.2f, 0.5f);

    private void Start()
    {
        if (playerMana == null) playerMana = FindObjectOfType<PlayerMana>();
        if (playerMana == null) return;

        playerMana.ManaChanged += OnManaChanged;
        SetBar(playerMana.CurrentMana / playerMana.MaxMana);
    }

    private void OnManaChanged(float current, float max)
    {
        SetBar(current / max);
    }

    private void SetBar(float ratio)
    {
        if (float.IsNaN(ratio) || float.IsInfinity(ratio)) ratio = 0f;
        ratio = Mathf.Clamp01(ratio);

        if (fillImage != null)
        {
            fillImage.rectTransform.anchorMax = new Vector2(ratio, 1f);
            fillImage.color = Color.Lerp(lowBlue, fullBlue, ratio);
        }

        if (manaText != null && playerMana != null)
            manaText.text = string.Format("{0} / {1}",
                Mathf.CeilToInt(playerMana.CurrentMana),
                Mathf.CeilToInt(playerMana.MaxMana));
    }

    private void OnDestroy()
    {
        if (playerMana != null)
            playerMana.ManaChanged -= OnManaChanged;
    }
}
