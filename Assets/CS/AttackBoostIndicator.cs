using UnityEngine;
using UnityEngine.UI;

public class AttackBoostIndicator : MonoBehaviour
{
    [Header("Target")]
    public PlayerAttack playerAttack;

    [Header("UI")]
    public GameObject indicatorRoot;
    public Text boostText;
    public Image timerFill;

    private int _baseDamage;

    private void Start()
    {
        if (playerAttack == null) playerAttack = FindObjectOfType<PlayerAttack>();
        if (playerAttack != null) _baseDamage = playerAttack.damage;

        if (indicatorRoot != null)
            indicatorRoot.SetActive(false);
    }

    private void Update()
    {
        if (playerAttack == null) return;

        int bonus = playerAttack.CurrentDamage - _baseDamage;
        bool hasBoost = bonus > 0;

        if (indicatorRoot != null)
            indicatorRoot.SetActive(hasBoost);

        if (hasBoost && boostText != null)
            boostText.text = string.Format("ATK +{0}", bonus);
    }
}
