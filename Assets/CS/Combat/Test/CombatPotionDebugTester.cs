using UnityEngine;

public class CombatPotionDebugTester : MonoBehaviour
{
    private bool showPanel = true;
    private PlayerHealth playerHealth;
    private PlayerMana playerMana;
    private PlayerAttack playerAttack;

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        playerMana = FindObjectOfType<PlayerMana>();
        playerAttack = FindObjectOfType<PlayerAttack>();
    }

    private void OnGUI()
    {
        if (!showPanel)
        {
            return;
        }

        int panelW = 320;
        int panelH = 220;
        Rect panelRect = new Rect(10, 10, panelW, panelH);

        GUI.Box(panelRect, "Combat Potion Debug Tester");
        GUI.BeginGroup(panelRect);

        int curY = 28;
        int pad = 10;
        int innerW = panelW - pad * 2;
        int btnH = 26;

        GUI.Label(new Rect(pad, curY, innerW, 20), string.Format("HP: {0}/{1}", GetCurrentHealth(), GetMaxHealth()));
        curY += 22;
        GUI.Label(new Rect(pad, curY, innerW, 20), string.Format("MP: {0}/{1}", GetCurrentMana(), GetMaxMana()));
        curY += 22;
        GUI.Label(new Rect(pad, curY, innerW, 20), string.Format("ATK: {0}", GetCurrentAttack()));
        curY += 28;

        if (GUI.Button(new Rect(pad, curY, innerW, btnH), "Use Health Potion"))
        {
            UsePotion("potion_health");
        }
        curY += btnH + 4;

        if (GUI.Button(new Rect(pad, curY, innerW, btnH), "Use Mana Potion"))
        {
            UsePotion("potion_mana");
        }
        curY += btnH + 4;

        if (GUI.Button(new Rect(pad, curY, innerW, btnH), "Use Attack Potion"))
        {
            UsePotion("potion_attack_boost");
        }

        GUI.EndGroup();
    }

    private void UsePotion(string itemId)
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        InventoryManager.Instance.AddItem(itemId, 1);
        InventoryManager.Instance.UsePotion(itemId);
    }

    private float GetCurrentHealth()
    {
        return playerHealth != null ? playerHealth.CurrentHealth : 0f;
    }

    private float GetMaxHealth()
    {
        return playerHealth != null ? playerHealth.MaxHealth : 0f;
    }

    private float GetCurrentMana()
    {
        return playerMana != null ? playerMana.CurrentMana : 0f;
    }

    private float GetMaxMana()
    {
        return playerMana != null ? playerMana.MaxMana : 0f;
    }

    private int GetCurrentAttack()
    {
        return playerAttack != null ? playerAttack.CurrentDamage : 0;
    }
}