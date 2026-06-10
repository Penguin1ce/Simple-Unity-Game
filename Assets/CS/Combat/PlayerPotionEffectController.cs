using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPotionEffectController : MonoBehaviour
{
    [Header("Potion Buff Settings")]
    [SerializeField] private float attackBoostDuration = 30f;

    private PlayerHealth playerHealth;
    private PlayerMana playerMana;
    private PlayerAttack playerAttack;
    private bool isSubscribed;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void ResolveReferences()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PlayerHealth>();
            }
        }

        if (playerMana == null)
        {
            playerMana = GetComponent<PlayerMana>();
            if (playerMana == null)
            {
                playerMana = GetComponentInParent<PlayerMana>();
            }
        }

        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack == null)
            {
                playerAttack = GetComponentInParent<PlayerAttack>();
            }
        }
    }

    private void TrySubscribe()
    {
        ResolveReferences();

        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null || isSubscribed)
        {
            return;
        }

        inventoryManager.OnPotionUsed += HandlePotionUsed;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
        {
            return;
        }

        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager != null)
        {
            inventoryManager.OnPotionUsed -= HandlePotionUsed;
        }

        isSubscribed = false;
    }

    private void HandlePotionUsed(string itemId, PotionEffectType effectType, int effectValue)
    {
        switch (effectType)
        {
            case PotionEffectType.HealthRestore:
                if (playerHealth != null)
                {
                    playerHealth.Heal(effectValue);
                }
                else
                {
                    Debug.LogWarning(string.Format("[PlayerPotionEffectController] Missing PlayerHealth for {0}", itemId));
                }
                break;

            case PotionEffectType.ManaRestore:
                if (playerMana != null)
                {
                    playerMana.RestoreMana(effectValue);
                }
                else
                {
                    Debug.LogWarning(string.Format("[PlayerPotionEffectController] Missing PlayerMana for {0}", itemId));
                }
                break;

            case PotionEffectType.AttackBoost:
                if (playerAttack != null)
                {
                    playerAttack.ApplyTemporaryAttackBoost(effectValue, attackBoostDuration);
                }
                else
                {
                    Debug.LogWarning(string.Format("[PlayerPotionEffectController] Missing PlayerAttack for {0}", itemId));
                }
                break;
        }
    }
}